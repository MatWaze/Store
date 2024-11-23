using Azure.AI.Translation.Text;

namespace Store.Infrastructure
{
    public interface IAzureTranslation
    {
        public Task<TranslatedTextItem?> TranslateTextAsync(string text, string from, string to = "ru");
        public Task<TranslatedTextItem?> TranslateTextAsync(string text, string to = "ru");
        public Task<string> DetectedLanguage(string text);
    }
}
