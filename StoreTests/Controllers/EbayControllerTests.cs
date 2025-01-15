using System.Security.Claims;
using Store.Models;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using eBay.ApiClient.Auth.OAuth2;
using eBay.ApiClient.Auth.OAuth2.Model;
using Store.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Store.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceStack.Redis;
using Azure.Storage.Blobs;

namespace StoreTests
{
    public class EbayControllerTests
    {
        public static Mock<UserManager<ApplicationUser>> GetUserManager()
        {
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

            mockUserManager
                .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Success));

            mockUserManager
                .Setup(userManager => userManager
                .AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()));

            return mockUserManager;
        }

        public async Task<Mock<OAuth2Api>> GetEbayToken()
        {
            var blob = new BlobStorageService(
                new BlobServiceClient(Environment.GetEnvironmentVariable("BlobConnection"))
            );

            using (Stream fileStream = await blob.GetFileStreamAsync("ebay.yml"))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    CredentialUtil.Load(streamReader);
                }
            }

            Mock<OAuth2Api> mockOauth = new Mock<OAuth2Api>();
            mockOauth
                .Setup(oa => oa
                .GetApplicationToken(OAuthEnvironment.PRODUCTION, new List<string>
                {
                        "https://api.ebay.com/oauth/api_scope"
                })
                .AccessToken.ToString())
                .Returns("v^1.1#i^1#p^1#r^0#I^3#f^0#t^H4sIAAAAAAAAAOVYe2wURRjvtVcIQoEEUw0BPBZEQrN3u7e391h7B9drSw/6OLhraQsC+5htt73bPXZmaU+NHgUhDcaY+IjEhIdorMQAiQL+AcSigIkaxUT7h4YQY40PkGBQEYx1d+8o10p49RKbeP9c5ptvvvn9fvN9M7NDpCdMWrS1ZusfJZaJhbvTRLrQYiEnE5MmFJdNLSqcWVxA5DhYdqfnp609RT+UQzYRTzIrAUwqMgS27kRchoxp9GOaKjMKCyXIyGwCQAbxTDRYV8s47QSTVBWk8Eocs4Ur/ZhACW4Px3sEj9tFuFivbpWvx4wper/HxQoU5XNzPOcUgNEPoQbCMkSsjPyYk3DSOEHipCtG+hjay1C03eVxt2K2JqBCSZF1FzuBBUy4jDlWzcF6a6gshEBFehAsEA5WRxuC4cqq+li5IydWIKtDFLFIgyNbIUUAtiY2roFbTwNNbyaq8TyAEHMEMjOMDMoEr4O5B/im1G6O89Je0UUQtNNJ0+68SFmtqAkW3RqHYZEEXDRdGSAjCaVup6iuBtcBeJRt1eshwpU242+FxsYlUQKqH6uqCLYEIxEsUMcisFGBQTyKFBXgkZWVuOAjSI9LoADu89I+N+t0Z2fJhMpqPGqakCILkqEYtNUrqALokMFoYVw5wuhODXKDGhSRASfHz0leF9DtbTVWNLOEGmqXjUUFCV0Fm9m8vfzDoxFSJU5DYDjC6A5THz/GJpOSgI3uNBMxmzvd0I+1I5RkHI6uri57F2VX1DaHkyBIR3NdbZRvBwkWy/gata77S7cfgEsmFR7oI6HEoFRSx9KtJ6oOQG7DAjRBUYQrq/tIWIHR1n8Zcjg7RpZDvsrD5aNE3sNxIuBZkiKJfJRHIJuhDgMH4NgUnmDVToCScZYHOK/nmZYAqiTosUQn5RUBLrh9Iu7yiSLO0YIbJ0UACAA4jvd5/zdVcqd5HgW8ClC+Ej0/SR5dXtcU8nAr1GYP+TiEajW9SkuF6knR2cI7lVSHL9LJNZAbNjjLGv13Wgo3JR+KS7oyMX3+/Alg1Ho+RKhRIALCmOhFeSUJIkpc4lPja4EpVYiwKkpFQTyuG8ZEMphMhvO2UeeH3t3sEfdGOq+n039xMt2UFTTydXyxMsZDPQCblOzG2WPnlYRDYfVLh2Eyan2diXpMvCX9wjquWOskM2wlIXPTtJuU7XAjb1cBVDRVv2TbG4y7V0zpBLJ+mCFViceB2kSOuZgTCQ2xXByMt6rOQ4JL7Dg7aUkP5fZ6XR6SHhMv3jxH1423LSmv+7D10bu4TTtGftgHCswf2WM5QfRYjhdaLEQ58TA5j5g7oajRWjRlJpQQsEusaIdSm6x/r6rA3glSSVZSC2cUfDa1VthUU/tbmtOOrLq82FtQkvOusPsx4sHhl4VJReTknGcGYtaNnmJy2gMlTpogSRfpo70U3UrMu9FrJUut91f3TxHXYydfm1nx9HRm1tW106pi9xElw04WS3GBtcdSsKN5oOHAj3vXrjzybvHCsv5XBj54y7L5au+pOZ/va28Z6olcnb5z05sfPbOw+cuhU/sOdSztXz176rFzfSHl7JTvtK39By5NP7Fs57eV67ddeLFvve+RF2rev/jNitTxay+1DZ2+8NevHw++93NVxdkrxLMnbdHgqYu+Nenzh6u/vjTwIbtkaXt744Le2IzNT+w4/fvs2cEvZq3Zt6eyt3T/rr6Kw3v/nr96+felx7aUSeDl50IHQWP40OtvzPDtn/sVs6fvp+eXbl82r9a2axv+6jvWsnOXKwYjHUcXbarf0jv01JyyifR5IYB/st/950OD/g0Lip88uOTtKwPXSstbrnwKonN/6T56pvpI6xltcPGhzFr+AwVx27XxEQAA");
            return mockOauth;
        }

        public static Mock<IProductRepository> GetRepo()
        {
            var mockRepo = new Mock<IProductRepository>();

            var categories = new Category[]
            {
                new Category { EbayCategoryId = 0, Name = "c1"},
                new Category { EbayCategoryId = 1, Name = "c2"},
            };

            var products = new Product[]
            {
                new() { Name = "P0", Category = categories[0], ProductId = 1, ItemCountry = "AM" },
                new() { Name = "P1", Category = categories[0], ProductId = 2, ItemCountry = "AM" },
                new() { Name = "P2", Category = categories[0], ProductId = 3, ItemCountry = "AM" },
                new() { Name = "P3", Category = categories[1], ProductId = 4, ItemCountry = "AM" },
                new() { Name = "P4", Category = categories[1], ProductId = 4, ItemCountry = "AM" },
                new() { Name = "P5", Category = categories[1], ProductId = 4, ItemCountry = "AM" },
                new() { Name = "P6", Category = categories[0], ProductId = 4, ItemCountry = "AM" },
            };

            mockRepo
                .Setup(repo => repo.Products)
                .Returns(products.AsQueryable());
            
            mockRepo
                .Setup(repo => repo.Categories)
                .Returns(categories.AsQueryable());

            return mockRepo;
        }

        //public Mock<IOutputCacheStore> GetRedisCache()
        //{
        //    var mockRedis = new Mock<CacheService>();

        //    mockRedis.Setup(cache => cache.)
        //}

        [Fact]
        public async void Can_Get_Item()
        {
            var httpClient = new HttpClient();

            var ebaySrv = new EbayService(httpClient);
            string tok = "v^1.1#i^1#p^1#f^0#r^0#I^3#t^H4sIAAAAAAAAAOVYa2wURRzv9UWa0qKivMTkXNDwcPdm93p3e5v25LhSe6GPg2sLrY86uzvbbnu3e+zMtpzRUkpKNPDBxBhAo4AffOEXBUVI4INRY4wPjEHjByBiQDQgQXlEoujs9cG1EkB6MU28L5f5z3/+8/v95v+fmR3QX1yyaGPtxktlrin5O/pBf77LxZeCkuKixeUF+XOK8kCWg2tH//z+woGCU5UYJhMpaSXCKdPAyL02mTCwlDFWMbZlSCbEOpYMmERYIooUD9fXSQIHpJRlElMxE4w7Wl3FQChAgUcBWdOQCJFIrcZIzCaziuGBF4oVUFb91EUGXtqPsY2iBibQIFWMAAQfC3iWr2gSgAREiQecGPC2Me4WZGHdNKgLB5hQBq6UGWtlYb0+VIgxsggNwoSi4Zp4YzhavayhqdKTFSs0rEOcQGLjsa2IqSJ3C0zY6PrT4Iy3FLcVBWHMeEJDM4wNKoVHwNwC/IzUoqD5ZVUFgOrtD+ZEyBrTSkJyfRSORVdZLeMqIYPoJH0jPakWchdSyHCrgYaIVrudvxU2TOiajqwqZtnScGs4FmNC9ZCgHhOH2TgxLcTGVlazahDwgQrVi9ig6Av6oeAfnmUo1LDC46aJmIaqO3phd4NJliIKGY0Rhg9KvixhqFOj0WiFNeLAyRZQGBHQH2hz1nNoAW3SaThLipJUBXemeWP5R0cTYumyTdBohPEdGX1oRaVSusqM78yk4XDmrMVVTCchKcnj6e3t5Xq9nGl1eAQAeM/q+rq40omSkBnxdWod6zcewOoZKgqiI7EukXSKYllL05QCMDqYkA94vaBiWPexsELjrf8wZHH2jC2GXBWHDAO8D2k+v+oXAxVBLRflERrOUI+DA8kwzSah1Y1IKgEVxCo0z+wksnRV8vo0wStqiFX9QY2ls2us7FP9LK8hBBCSZSUo/m+q5GbzPI4UC5HcJXoukjy+vL4lEpBXWKsD/BMYWzW+VXY60sBrQqsimOmuYKxbbuTXrBEWN1fdbClck3wkoVNlmuj8uRTAqfWJi1BrYoLUCdGLK2YKxcyErqQn1wJ7LTUGLZKOo0SCGiZEMpxKRXO4UeeC3r/ZI26NdI5Pp//+ZLomK+zk6+Ri5YzHNABM6Zxz9nCKmfSYkF46HFM7dmrdQT0h3jq9rk4q1pTkEFtdHbppchnKHO5ROAth07boFZtrdO5eTWY3MuhhRiwzkUBWCz/hYk4mbQLlBJpsVZ2DBNfhJDtp+YDXL4o+IQgmxEvJnKPtk21Lyuk+7MGFwZu+TXvGftaH8jI/fsD1ARhwHcx3uUAluI+fB+4tLmguLJg6B+sEcTrUOKx3GPRr1UJcN0qnoG7lT8/7srxOXV9bd6FftveuOv+gmFeW9aqw41Ewa/RdoaSAL816ZABzr/YU8dNmlgk+wPMVVESRB21g3tXeQn5G4Z1P3v/F3sdiW7TLg48v3rf8wrn5Tbs3gLJRJ5erKK9wwJW3/aEz656KnUi9MnuR9/y5tr5P5q3r1Q+T90o3Bb7/Nn9L79unm4/MLn9gLul6tT5afnzPS8/sjIe2ffj+T/sDtZff/PzAlK3yPVvV7l/FA6cLYu27vvvhYk3PnKOPbL27enND5Otj33y17a+Fnwb3zdx1qpgvtdrv+L3yuSvMEs/52w69fmnA9UvJwe0nOv44HCxpsz871fXjbyft6dyeNc1FSfHKW9t6qtc/fLbvZc+x1Xln8LTjT28off6NVmaB9toL7uqp8NmzZWznzM2DC3oi76yTL7ac271+74xNRwKzjpYP3h5Z8fTJny/VDO48vqRrj+vCXS9qh/r2VxwLfNz5UR9/sHWW/9252/9cuHJ659Ba/g06SbRa7xEAAA==";
            
            var items = await ebaySrv.SearchItemsAsync(tok, "DT Spare Parts", 0, 1000);
        }

    }
}
