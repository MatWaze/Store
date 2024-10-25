using Newtonsoft.Json.Linq;

namespace Store.Infrastructure
{
    public interface IEbayService
    {
        public Task<JObject> SearchItemsAsync(
            string accessToken, 
            string query, 
            int priceLow, 
            int priceUpper, 
            int limit = 5, 
            string categoryId = ""
        );

        public Task<JObject> GetItemInfoAsync(string accessToken, string id);

        public Task<string?> TranslateFromToAsync(string accessToken, string text, string to,
            string translationContent = "ITEM_TITLE", string from = "en");
    }
}
