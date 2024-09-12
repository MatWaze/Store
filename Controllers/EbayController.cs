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

namespace Store.Controllers
{
    public class EbayController : Controller
    {
        private IProductRepository context;
        private OAuth2Api oauth;
        private GetLocation location;
        private HttpClient httpClient;
        private string token;

        public EbayController(OAuth2Api oauthApi, GetLocation loc,
            HttpClient client, IProductRepository dataContext)
        {
            oauth = oauthApi;
            location = loc;
            httpClient = client;
            context = dataContext;
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


        //public IActionResult Auth2()
        //{
        //    string clientId = "uhedu6u4q1b6mp2tvimjads6bng8s72q";
        //    string apiUrl = $"https://yookassa.ru/oauth/v2/authorize?response_type=code&client_id={clientId}&state=test-user";
        //    return Redirect(apiUrl);
        //}

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
            //ViewBag.UserCountry = await location.GetCountry();
            var accessToken = oauth.GetApplicationToken(OAuthEnvironment.PRODUCTION, Scopes)
                .AccessToken.Token;

            //var accessToken = oauth.ExchangeCodeForAccessToken(OAuthEnvironment.PRODUCTION, "").AccessToken.Token; 
            JToken item = await EbayService.GetItemInfoAsync(httpClient, accessToken, id);
            //Product newProduct = new Product
            //{
            //    Name = item["title"].ToString(),

            //};
            return View(item);
        }

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
