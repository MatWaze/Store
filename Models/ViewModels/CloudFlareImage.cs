using System.Text.Json.Serialization;

namespace Store.Models.ViewModels
{
    public class CloudflareImage
    {
        [JsonPropertyName("result")]
        public CloudflareImageResult Result { get; set; }
    }

    // Image result object
    public class CloudflareImageResult
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("meta")]
        public Dictionary<string, string> Meta { get; set; }

        [JsonPropertyName("requireSignedURLs")]
        public bool RequireSignedURLs { get; set; }

        [JsonPropertyName("uploaded")]
        public DateTime Uploaded { get; set; }

        [JsonPropertyName("variants")]
        public List<string> Variants { get; set; }
    }
}
