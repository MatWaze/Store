using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using Azure.Core;

namespace Store.Infrastructure
{
    public class EbayService : IEbayService
    {

        private HttpClient httpClient;

        public EbayService(HttpClient httpClnt)
        {
            httpClient = httpClnt;
        }

        public async Task<JObject> SearchItemsAsync(
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

        public async Task<JObject> GetItemInfoAsync(string accessToken, string id)
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

        public async Task<string?> TranslateFromToAsync(string accessToken, string text, string to,
            string translationContent = "ITEM_TITLE", string from = "en")
        {
            string apiUrl = "https://api.ebay.com/commerce/translation/v1_beta/translate";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders
                .Add("X-EBAY-C-ENDUSERCTX", $"contextualLocation=country=AM");

            var response = await httpClient.PostAsJsonAsync(apiUrl, new
            {
                from = from,
                to = to,
                text = new List<string> { text },
                translationContext = translationContent,
            });

            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(responseBody);
            string? translatedString = jsonObject["translations"]?.First?["translatedText"]?.ToString();

            return translatedString;
        }

    }
}
