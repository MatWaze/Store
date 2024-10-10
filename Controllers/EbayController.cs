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

namespace Store.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class EbayController : Controller
    {
        private IProductRepository context;
        private OAuth2Api oauth;
        private GetLocation location;
        private HttpClient httpClient;
        private string token;
		private UserManager<ApplicationUser> userManager;
        private readonly IMemoryCache _memoryCache;

        public EbayController(OAuth2Api oauthApi, GetLocation loc,
            HttpClient client, IProductRepository dataContext,
			UserManager<ApplicationUser> usrManager, IMemoryCache memoryCache = null)
        {
            oauth = oauthApi;
            location = loc;
            httpClient = client;
            context = dataContext;
            userManager = usrManager;
            _memoryCache = memoryCache;
        }

        public List<string> Scopes = new List<string>()
        {
            "https://api.ebay.com/oauth/api_scope"
        };

        public int PageSize = 6;

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

        public async Task<IActionResult> GetItem(string id)
        {
            //ViewBag.IsAdmin = User.IsInRole("admin");
			ViewBag.Disabled = false;

			//ViewBag.UserCountry = await location.GetCountry();
			Product? prod = context.Products.FirstOrDefault(p => p.EbayProductId == id);
            if (prod != null)
            {
                return View(prod);
            }
            else
            {
                var accessToken = oauth.GetApplicationToken(OAuthEnvironment.PRODUCTION, Scopes)
                    .AccessToken.Token;
                //var accessToken = oauth.ExchangeCodeForAccessToken(OAuthEnvironment.PRODUCTION, "").AccessToken.Token; 

                JToken item = await EbayService.GetItemInfoAsync(httpClient, accessToken, id);

                string? quan = item["estimatedAvailabilities"]?.First()?["estimatedAvailableQuantity"]?.ToString()
                    ?? null;
                string? ship = item["shippingOptions"]?.FirstOrDefault()?["shippingCost"]?["value"]?.ToString()
                    ?? null;

                ApplicationUser? admin = await userManager.FindByNameAsync("admin");
				var culture = CultureInfo.GetCultureInfo("en-US");
				string category = item["categoryPath"].ToString().Split('|').Last();
                long categoryId = long.Parse(item["categoryId"].ToString(), culture);

                var additionalImages = item["additionalImages"];
                string imageUrls = String.Empty;
                
                if (additionalImages != null)
                {
                    foreach (var image in additionalImages)
                    {
                        string imageUrl = image["imageUrl"].ToString();
                        imageUrls += (imageUrl + " ");
                    }
                }

                Category? cat = null;
                if (context.Categories.Count() < 10)
                {
                    cat = context.Categories
                        .FirstOrDefault(c => c.EbayCategoryId == categoryId);    
                }
                else
                {
                    cat = context.Categories
                        .FirstOrDefault(c => c.EbayCategoryId == 6000);
                }
                
                Product newProduct = new Product
                {
                    Name = item["title"].ToString(),
                    EbayProductId = id,
                    Category = cat ?? new() { Name = category, EbayCategoryId = categoryId },
                    Quantity = quan != null ? int.Parse(quan, culture) : 0,
			        ItemCountry = item["itemLocation"]["country"].ToString()!,
					Description = item["condition"].ToString(),
					ImageLink = item["image"]["imageUrl"].ToString().Replace("225.", "500."),
					Price = decimal.Parse(item["price"]["value"].ToString(), culture),
					ShippingPrice = ship != null ? decimal.Parse(ship, culture) : -100,
                    //UserId = admin.Id,
                    ImageUrls = imageUrls,
				};
                context.AddProduct(newProduct);
                Console.WriteLine("Saved product");
                return View(newProduct);
			}
        }

        [OutputCache(PolicyName = "default", VaryByRouteValueNames = new[] { "queryName", "categoryNumber", "priceLow", "priceHigh" })]
        public async Task<IActionResult> Results(string queryName, int categoryNumber, int priceLow, int priceHigh, int itemPage = 1)
        {
            if (priceLow > priceHigh)
            {
                int temp = priceLow;
                priceLow = priceHigh;
                priceHigh = temp;
            }
           
            ViewBag.QueryName = queryName;
            ViewBag.EbayCategory = categoryNumber;
            ViewBag.Low = priceLow;
            ViewBag.Up = priceHigh;

            string cacheKey = $"{queryName}_{categoryNumber}_{priceLow}_{priceHigh}";

            if (!_memoryCache.TryGetValue(cacheKey, out JObject keyValuePairs))
            {
                var accessToken = oauth.GetApplicationToken(OAuthEnvironment.PRODUCTION, Scopes).AccessToken.Token;
                keyValuePairs = await EbayService.SearchItemsAsync(httpClient, accessToken, queryName, priceLow, priceHigh, 50, categoryNumber.ToString());
                _memoryCache.Set(cacheKey, keyValuePairs, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
            }

            JToken? itemSummaries = keyValuePairs["itemSummaries"];
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
            else
            {
                _memoryCache.Remove(cacheKey);
                cacheKey = $"{queryName}_{priceLow}_{priceHigh}";

                if (!_memoryCache.TryGetValue(cacheKey, out keyValuePairs))
                {
                    var accessToken = oauth.GetApplicationToken(OAuthEnvironment.PRODUCTION, Scopes).AccessToken.Token;
                    keyValuePairs = await EbayService.SearchItemsAsync(httpClient, accessToken, queryName, priceLow, priceHigh, 50);
                    _memoryCache.Set(cacheKey, keyValuePairs, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
                }
                itemSummaries = keyValuePairs["itemSummaries"];
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
            }
            Console.WriteLine("No results were found. Try again.");
            ViewBag.Categories = context.Categories.ToArray();
            return RedirectToAction("Home", "Index")/*View("Search", new QueryProduct())*/;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchForm(QueryProduct queryProduct, int pageNumber = 1, int pageSize = 2)
        {
            if (queryProduct.PriceLow > queryProduct.PriceHigh)
            {
                int temp = queryProduct.PriceLow;
                queryProduct.PriceLow = queryProduct.PriceHigh;
                queryProduct.PriceHigh = temp;
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("Results", new
                {
                    queryName = queryProduct.QueryName,
                    categoryNumber = queryProduct.CategoryNumber,
                    priceLow = queryProduct.PriceLow,
                    priceHigh = queryProduct.PriceHigh
                });
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
