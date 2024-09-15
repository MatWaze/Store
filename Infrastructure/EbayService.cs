using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Store.Infrastructure
{
    public class EbayService
    {
        public static async Task<JObject> SearchItemsAsync(HttpClient httpClient,
            string accessToken, string query, int priceLow, int priceUpper, int limit = 5, string categoryId = "")
        {
            string apiUrl =
                $"https://api.ebay.com/buy/browse/v1/item_summary/search?q={query}" +
                $"&category_ids={categoryId}&limit={limit}" +
                $"&filter=price:[{priceLow}..{priceUpper}],priceCurrency:USD";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders
               .Add("X-EBAY-C-ENDUSERCTX", $"contextualLocation=country=AM");

            var response = await httpClient.GetAsync(apiUrl);

            string responseBody = await response.Content.ReadAsStringAsync();
            //Console.WriteLine("resp:" + responseBody);
            var jsonObject = JObject.Parse(responseBody);
            return jsonObject;
        }

        public static async Task<JObject> GetItemInfoAsync(HttpClient httpClient, string accessToken, string id)
        {
            string apiUrl = $"https://api.ebay.com/buy/browse/v1/item/{id}?fieldgroups=PRODUCT";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders
                .Add("X-EBAY-C-ENDUSERCTX", $"contextualLocation=country=AM");

            var response = await httpClient.GetAsync(apiUrl);

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(responseBody);
            return jsonObject;
        }
    }
}
