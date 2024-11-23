using Azure;
using Azure.AI.Translation.Text;
using Microsoft.AspNetCore.Components.Forms;
using Azure.AI.TextAnalytics;

namespace Store.Infrastructure
{
    public class AzureTranslation : IAzureTranslation
    {
        private readonly TextTranslationClient clientTranslation;
        private readonly TextAnalyticsClient clientAnalytics;

        public AzureTranslation(
            TextTranslationClient clnt,
            TextAnalyticsClient client)
        {
			clientTranslation = clnt;
            clientAnalytics = client;
        }

		public async Task<string> DetectedLanguage(string text)
		{
            var lang = await clientAnalytics.DetectLanguageAsync(text);

            return lang.Value.Iso6391Name;
		}

		public async Task<TranslatedTextItem?> TranslateTextAsync(string text, string? from = null, string to = "ru")
        {
			Response<IReadOnlyList<TranslatedTextItem>> response = await clientTranslation
				.TranslateAsync(to, text, from);

            IReadOnlyList<TranslatedTextItem> translations = response.Value;
            TranslatedTextItem? translation = translations.FirstOrDefault();
            return translation;
        }

        public async Task<TranslatedTextItem?> TranslateTextAsync(string text, string to)
        {
            Response<IReadOnlyList<TranslatedTextItem>> response = await clientTranslation
			   .TranslateAsync(to, text);

            IReadOnlyList<TranslatedTextItem> translations = response.Value;
            TranslatedTextItem? translation = translations.FirstOrDefault();
            return translation;
        }
    }
}
