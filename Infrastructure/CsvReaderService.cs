using Serilog;
using TelegramSink;
using Serilog.Core;
using System.Globalization;
using CsvHelper;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Store.Infrastructure
{

    public class CsvReaderService : BackgroundService
    {
        private readonly string telegramApi = "6671853391:AAFoua3k3os-N3WS5UD3lQO-re84Zvml07A";
        private readonly string telegramId = "5660342708";

        private readonly HttpClient httpClientFactory;
        private readonly BlobStorageService blob;
        private readonly Logger log;

        public CsvReaderService(
            HttpClient httpClient,
            BlobStorageService blobStorage
            )
        {
            log = CreateLogger();
            httpClientFactory = httpClient;
            blob = blobStorage;
        }

        private Logger CreateLogger()
        {
            var log = new LoggerConfiguration()
               .MinimumLevel.Information()
               .WriteTo
               .TeleSink(
                  telegramApiKey: telegramApi,
                  telegramChatId: telegramId)
               .CreateLogger();
            return log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cultureUS = new CultureInfo("en-US");
            IEnumerable<Skin> skins = await ReadSkinsFromCsv("skins_factory.csv");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var skin in skins)
                {
                    try
                    {
                        var items = await GetItemsFromBuff(httpClientFactory, skin);

                        var goodsInfos = items["data"]["goods_infos"][skin.Index.ToString()];

                        float basePrice = float.Parse(goodsInfos["sell_min_price"].ToString(), cultureUS);

                        int counter = 1;

                        foreach (JToken item in items["data"]["items"])
                        {
                            float itemPrice = float.Parse(item["price"].ToString(), cultureUS);
                            decimal paintWear = decimal.Parse(item["asset_info"]["paintwear"].ToString(), cultureUS);

                            if (CheckCondition(paintWear))
                            {
                                log.Information("BUFF");
                                log.Information("You can buy {skinName}, {itemPrice}, {paintWear} with index {counter}",
                                    skin.Name, itemPrice, paintWear, counter);
                            }
                            counter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Information("BUFF");        
                        log.Error("Error getting listings for {skinName}: {errorMessage}",
                            skin.Name, ex.Message);
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                }
            }
        }

        public async Task<JObject> GetItemsFromBuff(HttpClient httpClient, Skin skin)
        {
            var response = await httpClient
                .GetAsync($"https://buff.163.com/api/market/goods/sell_order?game=csgo&goods_id={skin.Index}&page_num=1&sort_by=default");

            string jsonResponse = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(jsonResponse);
            return jsonObject;
        }

        public bool CheckCondition(decimal paintWear)
        {
            bool ans = false;

            if (paintWear <= 0.007M)
                ans = true;

            return ans;
        }

        public class Skin
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }

        public async Task<IEnumerable<Skin>> ReadSkinsFromCsv(string filePath)
        {
            using (Stream fileStream = await blob.GetFileStreamAsync(filePath))
            {
                using StreamReader reader = new StreamReader(fileStream);

                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                return csv.GetRecords<Skin>().ToList();
            }
        }
    }
}
