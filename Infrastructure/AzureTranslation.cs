using Azure;
using Azure.AI.Translation.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace Store.Infrastructure
{
    public class AzureTranslation : IAzureTranslation
    {
        private TextTranslationClient client;


        public AzureTranslation(TextTranslationClient clnt)
        {
            client = clnt;
        }

        public async Task<TranslatedTextItem?> TranslateTextAsync(string text, string from, string to = "ru")
        {
            Response<IReadOnlyList<TranslatedTextItem>> response = await client
                .TranslateAsync(to, text, from);

            IReadOnlyList<TranslatedTextItem> translations = response.Value;
            TranslatedTextItem? translation = translations.FirstOrDefault();
            return translation;
        }

        public async Task<TranslatedTextItem?> TranslateTextAsync(string text, string to)
        {
            Response<IReadOnlyList<TranslatedTextItem>> response = await client
               .TranslateAsync(to, text);

            IReadOnlyList<TranslatedTextItem> translations = response.Value;
            TranslatedTextItem? translation = translations.FirstOrDefault();
            return translation;
        }
    }
}
