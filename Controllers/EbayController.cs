using eBay.ApiClient.Auth.OAuth2.Model;
using eBay.ApiClient.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Store.Models;
using System.Net.Http.Headers;
using Store.Models.ViewModels;
using static System.Formats.Asn1.AsnWriter;
using Store.Infrastructure;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Globalization;
using Microsoft.VisualStudio.CodeCoverage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authorization;

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

		public EbayController(OAuth2Api oauthApi, GetLocation loc,
            HttpClient client, IProductRepository dataContext,
			UserManager<ApplicationUser> usrManager)
        {
            oauth = oauthApi;
            location = loc;
            httpClient = client;
            context = dataContext;
            userManager = usrManager;
        }

        public List<string> Scopes = new List<string>()
        {
            "https://api.ebay.com/oauth/api_scope"
        };


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
            ViewBag.IsAdmin = User.IsInRole("admin");
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

                Product newProduct = new Product
                {
                    Name = item["title"].ToString(),
                    EbayProductId = id,
                    Category = new() { Name = category, EbayCategoryId = categoryId },
                    Quantity = quan != null ? int.Parse(quan, culture) : 0,
			        ItemCountry = item["itemLocation"]["country"].ToString()!,
					Description = item["condition"].ToString(),
					ImageLink = item["image"]["imageUrl"].ToString().Replace("225.", "500."),
					Price = decimal.Parse(item["price"]["value"].ToString(), culture),
					ShippingPrice = ship != null ? decimal.Parse(ship, culture) : -100,
                    UserId = admin.Id,
                    ImageUrls = imageUrls,
				};
                return View(newProduct);
			}
        }

        [OutputCache(PolicyName = "default", VaryByRouteValueNames = new[] { "queryName", "categoryNumber", "priceLow", "priceHigh" })]
        public async Task<IActionResult> Results(string queryName, int categoryNumber, int priceLow, int priceHigh)
        {
            if (priceLow > priceHigh)
            {
                int temp = priceLow;
                priceLow = priceHigh;
                priceHigh = temp;
            }

            var accessToken = oauth.GetApplicationToken(OAuthEnvironment.PRODUCTION, Scopes)
                .AccessToken.Token;

            JObject keyValuePairs = await EbayService.SearchItemsAsync(httpClient,
                accessToken, queryName, categoryNumber, priceLow, priceHigh);

            JToken? itemSummaries = keyValuePairs["itemSummaries"];
            if (itemSummaries != null)
            {
                var items = itemSummaries.AsEnumerable();

                return View("Results", items);
            }

            ViewBag.Categories = context.Categories.ToArray();

            return View("Search", new QueryProduct());
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
