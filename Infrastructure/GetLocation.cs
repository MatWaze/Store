using Newtonsoft.Json.Linq;

namespace Store.Infrastructure
{
    public class GetLocation
    {
        private HttpClient httpClient; 
        public GetLocation(HttpClient client)
        {
            httpClient = client;
        }

        public string ApiUrl = "https://api.geoapify.com/v1/ipinfo?&apiKey=bbccc8fe5ec746d48bc691ac6c5dc231";

        public async Task<string> GetCountry()
        {
            var response = await httpClient.GetAsync(ApiUrl);

            string responseBody = await response.Content.ReadAsStringAsync();

            var jsonObject = JObject.Parse(responseBody);
            string countryName = jsonObject["country"]!["name"]!.ToString();

            return countryName;
        }
    }
}
