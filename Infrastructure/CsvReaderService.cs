using Serilog;
using TelegramSink;
using Serilog.Core;
using System.Globalization;
using CsvHelper;
using Newtonsoft.Json.Linq;

namespace Store.Infrastructure
{

    public class CsvReaderService : BackgroundService
    {
        private readonly string telegramApi = "6671853391:AAFoua3k3os-N3WS5UD3lQO-re84Zvml07A";
        private readonly string telegramId = "5660342708";
        
        private readonly IHttpClientFactory httpClientFactory;
        private readonly BlobStorageService blob;
        private readonly Logger log;

        public CsvReaderService(
            IHttpClientFactory httpClient,
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
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IEnumerable<Skin> skins = await ReadSkinsFromCsv("skins.csv");
                    var httpClient = httpClientFactory.CreateClient();

                    foreach (var skin in skins)
                    {
                        var items = await GetItemsFromBuff(httpClient, skin);

                        var goodsInfos= items["data"]["goods_infos"][skin.Index.ToString()];

                        float basePrice = float.Parse(goodsInfos["sell_min_price"].ToString(), cultureUS);

                        string skinName = goodsInfos["market_hash_name"].ToString();
                        int counter = 1;

                        foreach (JToken item in items["data"]["items"])
                        {
                            float itemPrice = float.Parse(item["price"].ToString(), cultureUS);
                            decimal paintWear = decimal.Parse(item["asset_info"]["paintwear"].ToString(), cultureUS);
                            
                            if (CheckCondition(basePrice, itemPrice, paintWear))
                            {
                                log.Information("You can buy {skinName}, {itemPrice}, {paintWear} with index {counter}",
                                    skinName, itemPrice, paintWear, counter);
                                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                            }
                            counter++;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(9), stoppingToken);
                    }

                }
                catch (Exception ex)
                {
                    log.Error("An error occurred while running SkinCheckerService: {error}",
                        ex.Message);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
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

        public bool CheckCondition(float basePrice, float targetPrice, decimal paintWear)
        {
            bool ans = false;

            if (paintWear <= 0.007M && (basePrice * 1.06) <= targetPrice)
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
            using (Stream fileStream = await blob.GetFileStreamAsync("skins_factory.csv"))
            {
                using StreamReader reader = new StreamReader(fileStream);
              
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                return csv.GetRecords<Skin>().ToList();
            }
        }
    }
}
