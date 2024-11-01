using eBay.ApiClient.Auth.OAuth2.Model;
using eBay.ApiClient.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Store.Models;
using Store.Models.ViewModels;
using Store.Infrastructure;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Azure.Core;
using Azure.AI.Translation.Text;
using System.Text;
using System.Threading;

namespace Store.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class EbayController : Controller
    {
        private IProductRepository context;
        private OAuth2Api oauth;
        private GetLocation location;
        private IEbayService ebayService;
        private string? token;
		private UserManager<ApplicationUser> userManager;
        private readonly IOutputCacheStore _memoryCache;
        private IAzureTranslation azureTranslate;

        public EbayController(
            OAuth2Api oauthApi, 
            GetLocation loc,
            IEbayService ebaySrv, 
            IProductRepository dataContext,
			UserManager<ApplicationUser> usrManager,
            IOutputCacheStore memoryCache,
            IAzureTranslation azureTranslation)
        {
            oauth = oauthApi;
            location = loc;
            ebayService = ebaySrv;
            context = dataContext;
            userManager = usrManager;
            _memoryCache = memoryCache;
            azureTranslate = azureTranslation;
        }

        public List<string> Scopes = new List<string>()
        {
            "https://api.ebay.com/oauth/api_scope"
        };

        public int PageSize = 6;

        public string currentCulture = CultureInfo.CurrentCulture.Name;

        public IActionResult Auth()
        {
            string s = oauth.GenerateUserAuthorizationUrl(OAuthEnvironment.PRODUCTION, Scopes, "State");
            return Redirect(s);  
        }
        
        public IActionResult Policy()
        {
            return View();
        }

        public async Task<IActionResult> Callback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Error");
            }

            token = oauth.ExchangeCodeForAccessToken(OAuthEnvironment.PRODUCTION, code).AccessToken.Token;
            return RedirectToAction("Index", "Home"); // Redirect to a secure page or home page
        }

        public async Task<IActionResult> GetItem(string id, string returnUrl = "/")
        {
            if (id == null )
                return RedirectToAction("PageNotFound", "Home");
			ViewBag.Disabled = false;
            ViewBag.ReturnUrl = returnUrl;

			//ViewBag.UserCountry = await location.GetCountry();
			Product? prod = context.Products.FirstOrDefault(p => p.EbayProductId == id);
            if (prod != null)
            {
                return View(prod);
            }
            else
            {
                string accessToken = await GetTokenFromCacheOrDefault();
                //var accessToken = oauth.ExchangeCodeForAccessToken(OAuthEnvironment.PRODUCTION, "").AccessToken.Token; 

                JToken item = await ebayService.GetItemInfoAsync(accessToken, id);

                string? quan = item["estimatedAvailabilities"]?.First()?["estimatedAvailableQuantity"]?.ToString()
                    ?? null;
                string? ship = item["shippingOptions"]?.FirstOrDefault()?["shippingCost"]?["value"]?.ToString()
                    ?? null;

                ApplicationUser? admin = await userManager.FindByNameAsync("admin");
				var culture = CultureInfo.GetCultureInfo("en-US");
				string category = item["categoryPath"].ToString().Split('|').Last();
                long categoryId = long.Parse(item["categoryId"].ToString(), culture);

                var additionalImages = item["additionalImages"];
                StringBuilder imageUrls = new StringBuilder();
                
                if (additionalImages != null)
                {
                    foreach (var image in additionalImages)
                    {
                        string imageUrl = image["imageUrl"].ToString();
                        imageUrls.Append(imageUrl + " ");
                    }
                }

                Category? cat = context.Categories
                    .FirstOrDefault(c => c.EbayCategoryId == categoryId);    
                if (cat == null)
                {
                    cat = context.Categories
                        .FirstOrDefault(c => c.EbayCategoryId == 6000);
                }
                Product newProduct = new Product
                {
                    Name = item["title"].ToString(),
                    NameRu = (await azureTranslate
                        .TranslateTextAsync(item["title"].ToString()))?
                        .Translations
                        .FirstOrDefault()?
                        .Text,
                    EbayProductId = id,
                    Category = cat,
                    Quantity = quan != null ? int.Parse(quan, culture) : 0,
			        ItemCountry = item["itemLocation"]["country"].ToString()!,
					Description = item["condition"].ToString(),
					ImageLink = item["image"]["imageUrl"].ToString().Replace("225.", "500."),
					Price = decimal.Parse(item["price"]["value"].ToString(), culture),
					ShippingPrice = ship != null ? decimal.Parse(ship, culture) : -100,
                    //UserId = admin.Id,
                    ImageUrls = imageUrls.ToString(),
				};
                context.AddProduct(newProduct);
                return View(newProduct);
			}
        }

        public async Task<string> GetTokenFromCacheOrDefault()
        {
            string? accessToken = null;
            CancellationToken cancellationToken = new();
            byte[]? bytes = await _memoryCache.GetAsync("EbayApplicationToken", cancellationToken);

            if (bytes != null)
            {
                accessToken = Encoding.ASCII.GetString(bytes);
            }
            if (accessToken == null)
            {
                accessToken = oauth
                    .GetApplicationToken(OAuthEnvironment.PRODUCTION, Scopes)
                    .AccessToken.Token;

                await _memoryCache.SetAsync(
                    "EbayApplicationToken", 
                    Encoding.ASCII.GetBytes(accessToken),
                    new string[] { "tag" }, 
                    TimeSpan.FromMinutes(90),
                    cancellationToken
                );
            }
            return accessToken!;
        }

        public async Task TranslateTitles(JObject json, CancellationToken cancellationToken)
        {
            foreach (var item in json["itemSummaries"])
            {
                string? transText = await ebayService
                    .TranslateFromToAsync(
                    await GetTokenFromCacheOrDefault(), 
                    item["title"].ToString(), 
                    "ru"
                );
                    
                item["title"] = transText;
            }
        }

        public async Task<IActionResult> Results(string queryName, int? priceLow, int? priceHigh, int itemPage = 1, int? categoryNumber = 0)
        {
            if (queryName == null)
            {
                return RedirectToAction("PageNotFound", "Home");
            }
            if (priceLow > priceHigh)
            {
                int temp = (int) priceLow;
                priceLow = priceHigh;
                priceHigh = temp;
            }
           
            ViewBag.QueryName = queryName;
            ViewBag.EbayCategory = categoryNumber;
            ViewBag.Low = priceLow;
            ViewBag.Up = priceHigh;

            string cacheKey = $"{queryName}_{categoryNumber}_{priceLow}_{priceHigh}";
            CancellationToken cancellationToken = new();
            JObject? keyValuePairs = null;
            byte[]? array = await _memoryCache.GetAsync(cacheKey, cancellationToken);

            if (array == null)
            {
                string accessToken = await GetTokenFromCacheOrDefault();

                keyValuePairs = await ebayService.SearchItemsAsync(accessToken, queryName, 
                    (int) priceLow!, (int) priceHigh!, 50, (categoryNumber != 0 ? categoryNumber.ToString() : "")!);

                if (keyValuePairs.ContainsKey("itemSummaries"))
                {
                    if (currentCulture == "ru-RU")
                    {
                       await TranslateTitles(keyValuePairs, cancellationToken);
                    }
                    string json = JsonConvert.SerializeObject(keyValuePairs);

                    await _memoryCache.SetAsync(
                        cacheKey,
                        Encoding.UTF8.GetBytes(json),
                        new string[] { "tag" },
                        TimeSpan.FromMinutes(90),
                        cancellationToken
                    );
                }
            }
            else
            {
                string byteString = Encoding.UTF8.GetString(array);
                keyValuePairs = JObject.Parse(byteString);
            }
            JToken? itemSummaries = keyValuePairs?["itemSummaries"];
            
            if (itemSummaries != null)
            {
                var items = itemSummaries
                    .Skip((itemPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToArray();

                return View("Results", new ProductTarget<JToken>
                {
                    Products = items,
                    PagingInfo = new PagingInfo
                    {
                        ItemsPerPage = PageSize,
                        CurrentPage = itemPage,
                        ItemsCount = itemSummaries.Count()
                    }
                });
            }
            ViewBag.Categories = context.Categories.ToArray();
            return RedirectToAction("Search")/*View("Search", new QueryProduct())*/;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchForm(QueryProduct queryProduct, int pageNumber = 1, int pageSize = 2)
        {
            if (queryProduct.PriceLow > queryProduct.PriceHigh)
            {
                int? temp = queryProduct.PriceLow;
                queryProduct.PriceLow = queryProduct.PriceHigh;
                queryProduct.PriceHigh = temp;
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("Results", new
                {
                    queryName = queryProduct.QueryName,
                    //queryName = (await azureTranslate.TranslateTextAsync(
                    //    queryProduct.QueryName!,
                    //    "ru",
                    //    "en"
                    //))?.Translations?.FirstOrDefault()?.Text,
                    categoryNumber = queryProduct.CategoryNumber,
                    priceLow = queryProduct.PriceLow,
                    priceHigh = queryProduct.PriceHigh
                });;
            }
            ViewBag.Categories = context.Categories.ToArray();
            return View("Search", queryProduct);
        }

        public IActionResult Search()
        {
            ViewBag.Categories = context.Categories.ToArray();

            var queryProduct = new QueryProduct();

            return View(queryProduct);
        }
    }
}
