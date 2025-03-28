﻿using Serilog;
using TelegramSink;
using Serilog.Core;
using System.Globalization;
using CsvHelper;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Store.Infrastructure
{
    public class CsvReaderHelperUU : BackgroundService
    {
        private readonly string telegramApi = "6671853391:AAFoua3k3os-N3WS5UD3lQO-re84Zvml07A";
        private readonly string telegramId = "5660342708";
        private Dictionary<string, object> uuRequestBody = new Dictionary<string, object>
        {
            { "autoDelivery", 0 },
            { "hasSold", "true" },
            { "haveBuZhangType", 0 },
            { "listSortType", "1" },
            { "listType", 10 },
            { "mergeFlag", 0 },
            { "pageIndex", 1 },
            { "pageSize", 50 },
            { "sortType", "1" },
            { "status", "20" },
            { "stickerAbrade", 0 },
            { "stickersIsSort", false },
            { "templateId", "" },
            { "ultraLongLeaseMoreZones", 0 },
            { "userId", "8932132" },
            { "Sessionid", "Z55fw1C71TsDAOhjo2WHtwqz" }
        };

        private readonly HttpClient httpClientFactory;
        private readonly BlobStorageService blob;
        private readonly Logger log;

        public CsvReaderHelperUU(
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
            IEnumerable<Skin> skins = await ReadSkinsFromCsv("skins_factory_uu.csv");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var skin in skins)
                {
                    try
                    {
                        var items = await GetItemsFromUU(skin);

                        float basePrice = 0.0f;
                        int counter = 1;

                        foreach (JToken item in items["Data"]["CommodityList"])
                        {
                            float itemPrice = float.Parse(item["Price"].ToString(), cultureUS);

                            if (basePrice == 0.0f)
                            {
                                basePrice = itemPrice;
                            }
                            decimal paintWear = decimal.Parse(item["Abrade"].ToString(), cultureUS);
                            
                            if (itemPrice > basePrice * 1.15)
                                break;
                            if (CheckCondition(paintWear))
                            {
                                log.Information("YOUPIN898");
                                log.Information("You can buy {skinName}, {itemPrice}, {paintWear} with index {counter}",
                                    skin.Name, itemPrice, paintWear, counter);
                            }
                            counter++;
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Information("YOUPIN898");
                        log.Error("Error getting listings for {skinName}",
                            skin.Name);
                    }
                    finally
                    {
                        var random = (new Random().Next(10, 16));
                        await Task.Delay(TimeSpan.FromSeconds(random), stoppingToken);
                    }
                }
            }
        }

        public async Task<JObject> GetItemsFromUU(Skin skin)
        {
            uuRequestBody["templateId"] = skin.Index.ToString();

            var json = JsonConvert.SerializeObject(uuRequestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.youpin898.com/api/homepage/v2/detail/commodity/list/sell");

            request.Headers.Add("user-agent", "okhttp/3.14.9");
            request.Headers.Add("uk", "5CobKZLp29kCJK8PGYetex72N5rnW6Ita0gCZvdyuTcpMMo6PenvnHcS553VIjt1K");
            request.Headers.Add("tracestate", "bnro=android/13_android/8.12.1_okhttp/3.14.9");
            request.Headers.Add("platform", "android");
            request.Content = requestContent;
            
            var response = await httpClientFactory.SendAsync(request);

            string jsonResponse = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(jsonResponse);
            JObject jsonObject = JObject.Parse(jsonResponse);
            return jsonObject;
        }
        public static bool CheckCondition(decimal paintWear)
        {
            bool ans = false;

            if (paintWear <= 0.006M)
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

    public class CsvReaderHelperUU2 : BackgroundService
    {
        private readonly string telegramApi = "6671853391:AAFoua3k3os-N3WS5UD3lQO-re84Zvml07A";
        private readonly string telegramId = "5660342708";
        private Dictionary<string, object> uuRequestBody = new Dictionary<string, object>
        {
            { "autoDelivery", 0 },
            { "haveBuZhangType", 0 },
            { "listSortType", "1" },
            { "listType", 10 },
            { "mergeFlag", 0 },
            { "pageIndex", 1 },
            { "pageSize", 50 },
            { "sortType", "1" },
            { "status", "20" },
            { "stickerAbrade", 0 },
            { "stickersIsSort", false },
            { "templateId", "" },
            { "ultraLongLeaseMoreZones", 0 },
            { "Sessionid", "ZtjYIZV10sQDAE9kxLYtzGuF" }
        };

        private readonly HttpClient httpClientFactory;
        private readonly BlobStorageService blob;
        private readonly Logger log;

        public CsvReaderHelperUU2(
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
            IEnumerable<Skin> skins = await ReadSkinsFromCsv("skins_factory_uu_fade.csv");

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var skin in skins)
                {
                    try
                    {
                        var items = await GetItemsFromUU(skin);

                        float basePrice = 0.0f;
                        int counter = 1;

                        foreach (JToken item in items["Data"]["CommodityList"])
                        {
                            float itemPrice = float.Parse(item["Price"].ToString(), cultureUS);

                            if (basePrice == 0.0f)
                            {
                                basePrice = itemPrice;
                            }
                            int paintSeed = int.Parse(item["paintSeed"].ToString());

                            if (CheckCondition(skin, paintSeed, itemPrice))
                            {
                                log.Information("YOUPIN898");
                                log.Information("You can buy {skinName}, {itemPrice}, {paintSeed} with index {counter}",
                                    skin.Name, itemPrice, paintSeed, counter);
                            }
                            counter++;
                        }

                    }
                    catch (Exception ex)
                    {
                        //log.Information("YOUPIN898");
                        //log.Error("Error getting listings for {skinName}: {exMessage}",
                        //    skin.Name, ex.Message);
                    }
                    finally
                    {
                        var random = (new Random().Next(60, 90));
                        await Task.Delay(TimeSpan.FromSeconds(random), stoppingToken);
                    }
                }
            }
        }

        private List<int> Lst = new List<int> { 80, 73, 449, 666, 576, 774, 382 };

        public bool CheckCondition(Skin skin, int seed, float itemPrice)
        {
            if (Lst.Contains(seed))
            {
                switch (skin.Index)
                {
                    // new
                    case 5473:
                        if (itemPrice <= 54)
                            return true;
                        break;
                    // min
                    case 44676:
                        if (itemPrice <= 32)
                            return true;
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

        public class Skin
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }

        public async Task<JObject> GetItemsFromUU(Skin skin)
        {
            uuRequestBody["templateId"] = skin.Index.ToString();

            var json = JsonConvert.SerializeObject(uuRequestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.youpin898.com/api/homepage/v2/detail/commodity/list/sell");

            request.Headers.Add("user-agent", "okhttp/3.14.9");
            request.Headers.Add("uk", "5CobKZLp29kCJK8PGYetex72N5rnW6Ita0gCZvdyuTcpMMo6PenvnHcS553VIjt1K");
            request.Headers.Add("tracestate", "bnro=android/13_android/8.12.1_okhttp/3.14.9");
            request.Headers.Add("platform", "android");
            request.Content = requestContent;

            var response = await httpClientFactory.SendAsync(request);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            JObject jsonObject = JObject.Parse(jsonResponse);
            return jsonObject;
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


    public class CsvReaderHelperUU3 : BackgroundService
    {
        private readonly string telegramApi = "6671853391:AAFoua3k3os-N3WS5UD3lQO-re84Zvml07A";
        private readonly string telegramId = "5660342708";
        private Dictionary<string, object> uuRequestBody = new Dictionary<string, object>
        {
            { "autoDelivery", 0 },
            { "haveBuZhangType", 0 },
            { "listSortType", "1" },
            { "listType", 10 },
            { "mergeFlag", 0 },
            { "pageIndex", 1 },
            { "pageSize", 50 },
            { "pendentTemplate", "0-1000" },
            { "pendentType", "templateRange" },
            { "sortType", "1" },
            { "status", "20" },
            { "stickerAbrade", 0 },
            { "stickersIsSort", false },
            { "templateId", "" },
            { "ultraLongLeaseMoreZones", 0 },
            { "userId", "8932132" },
            { "Sessionid", "ZtjYIZV10sQDAE9kxLYtzGuF" },
        };

        private readonly HttpClient httpClientFactory;
        private readonly BlobStorageService blob;
        private readonly Logger log;

        public CsvReaderHelperUU3(
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
            IEnumerable<Skin> skins = new List<Skin>
            {
                new Skin { Name = "Chicken Lil'", Index = 109703 },
                new Skin { Name = "That's Bananas", Index = 109725 },
                new Skin { Name = "Hot Sauce", Index = 109619 },
                new Skin { Name = "Pinch O' Salt", Index = 109608 },
                new Skin { Name = "Lil' Whiskers", Index = 109610 },
                new Skin { Name = "Lil' Squatch", Index = 109819 },
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var skin in skins)
                {
                    try
                    {
                        var items = await GetItemsFromUU(skin);

                        float basePrice = 0.0f;
                        int counter = 1;

                        foreach (JToken item in items["Data"]["CommodityList"])
                        {
                            float itemPrice = float.Parse(item["Price"].ToString(), cultureUS);
                            int paintSeed = int.Parse(item["paintSeed"].ToString());

                            if (CheckCondition(skin, paintSeed, itemPrice))
                            {
                                log.Information("YOUPIN898");
                                log.Information("You can buy {skinName}, {itemPrice}, {paintSeed} with index {counter}",
                                    skin.Name, itemPrice, paintSeed, counter);
                            }
                            counter++;
                        }

                    }
                    catch (Exception ex)
                    {
                        //log.Information("YOUPIN898");
                        //log.Error("Error getting listings for {skinName}: {exMessage}",
                        //    skin.Name, ex.Message);
                    }
                    finally
                    {
                        var random = (new Random().Next(60, 90));
                        await Task.Delay(TimeSpan.FromSeconds(random), stoppingToken);
                    }
                }
            }
        }

        private List<int> Lst = new List<int> { 109703, 109725, 109619, 109608, 109610, 109819 };

        public bool CheckCondition(Skin skin, int seed, float itemPrice)
        {
            if (Lst.Contains(seed))
            {
                switch (skin.Index)
                {
                    // Chicken Lil'
                    case 109703:
                        if (itemPrice <= 22 || seed <= 100)
                            return true;
                        break;

                    // That's Bananas
                    case 109725:
                        if ((itemPrice <= 15 && seed < 300) || (itemPrice <= 19 && seed < 200) || seed <= 100)
                            return true;
                        break;

                    // Hot Sauce
                    case 109619:
                        if ((itemPrice <= 11 && seed < 300) || (itemPrice <= 15 && seed < 200) || seed <= 100)
                            return true;
                        break;

                    // Pinch O' Salt
                    case 109608:
                        if ((itemPrice < 9 && seed < 200) || (itemPrice <= 18.5 && seed <= 100))
                            return true;
                        break;

                    // Lil' Whiskers
                    case 109610:
                        if ((itemPrice < 15.5 && seed < 300) || (itemPrice <= 17.5 && seed < 200) || seed <= 100)
                            return true;
                        break;

                    // Lil' Squatch
                    case 109819:
                        if ((itemPrice < 12.5 && seed < 200) || seed <= 100)
                            return true;
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

        public class Skin
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }

        public async Task<JObject> GetItemsFromUU(Skin skin)
        {
            uuRequestBody["templateId"] = skin.Index.ToString();

            var json = JsonConvert.SerializeObject(uuRequestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://api.youpin898.com/api/homepage/v2/detail/commodity/list/sell");

            request.Headers.Add("user-agent", "okhttp/3.14.9");
            request.Headers.Add("uk", "5CobKZLp29kCJK8PGYetex72N5rnW6Ita0gCZvdyuTcpMMo6PenvnHcS553VIjt1K");
            request.Headers.Add("tracestate", "bnro=android/13_android/8.12.1_okhttp/3.14.9");
            request.Headers.Add("platform", "android");
            request.Content = requestContent;

            var response = await httpClientFactory.SendAsync(request);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            JObject jsonObject = JObject.Parse(jsonResponse);
            return jsonObject;
        }
    }
}
