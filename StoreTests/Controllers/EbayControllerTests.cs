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

namespace StoreTests
{
    public class EbayControllerTests
    {
        public Mock<UserManager<ApplicationUser>> GetUserManager()
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

        public Mock<OAuth2Api> GetEbayToken()
        {
            CredentialUtil.Load(@"..\..\ebay.yml");

            Mock<OAuth2Api> mockOauth = new Mock<OAuth2Api>();
            mockOauth
                .Setup(oa => oa
                .GetApplicationToken(OAuthEnvironment.PRODUCTION, new List<string>
                {
                        "https://api.ebay.com/oauth/api_scope"
                })
                .AccessToken.ToString())
                .Returns("v^1.1#i^1#p^1#I^3#r^0#f^0#t^H4sIAAAAAAAAAOVYe2wURRi/6wMsUBtEQIXguZIYOPd1e9verfTM9ZVeQh9wbVMbCJ3dnW233ds9dmbbXkygNgaCCQoaotXUVBsTNEr8QwISRUOIQokmNSox0UBIoATR+EjEqFFn70q5VoRCL7GJ989lZ77vm+/3m+8xM1z/vKK1O2p3XCn2zs8b7uf687xefiFXNK/Qf2d+3n2FHi5LwDvcv7q/YCD/4joEEkZS2ghR0jIR9PUlDBNJ6cFyyrFNyQJIR5IJEhBJWJHi0br1UoDhpKRtYUuxDMoXqyqnBLlUFNQwFIKiHIRiiIyaV202WWReUWUuKABFhbIoQJHMI+TAmIkwMHE5FeACQZrnaU5o4gWJC0i8yAT5UBvla4E20i2TiDAcFUm7K6V17Sxfb+wqQAjamBihIrFoTbwhGquqrm9ax2bZikzwEMcAO2jqV6WlQl8LMBx442VQWlqKO4oCEaLYSGaFqUal6FVnbsP9NNWaAECIE4NKmSpqfCg3VNZYdgLgG/vhjugqraVFJWhiHaduxihhQ+6CCp74qicmYlU+92+DAwxd06FdTlVXRB+LNjZSkTqAYY+FonQcWzak4xWttBrmZKiWAZEuA2GxVODliVUypiY4nrZMpWWqussY8tVbuAISl+FUYgKSmEUMEWowG+yohl13suXESQL5NndHM1vo4E7T3VSYICz40p83p39SG2Nblx0MJy1Mn0jzU06BZFJXqemT6UCciJ0+VE51YpyUWLa3t5fpFRjL7mADHMezrXXr40onTAAqI+vmOpHXb65A62koCiSaSJdwKkl86SOBShwwO6hIMFAWCHATvE91KzJ99B8DWZjZqemQq/QIy4Isa2JY4QNCKa+Fc5EekYkIZV0/oAxSdALY3RAnDaBAWiFx5iSgrauSIGoBIaRBWi0Na3QwrGm0LKqlNK9ByEEoy0o49L/JkpnGeRwqNsS5CvTcBHmDzjWguh5ODMeaWSccUqrlFtFvtnUJWkNdhdbY0bqxBWxQ+Gqht3ymqXBd8JWGTphpIuvnjgA313NBQq2FMFRnBS+uWEnYaBm6kppbGyzYaiOwcarCSZHvODQM8jcrqNFkMpazcp0bkLdSKW4PdE571H/Rn66LCrlRO7dQufqIGABJnXE7EKNYCdYCjpvruNMd3pL22vdvgtlCrOykmA4HIkw8Ucnxb8ZKOqnkDGlm6sxVMq2SgJi5CrlbqI6Cb2uhdE9mCJt6RydGt7Rm32xIkR2je+YqKgTGrEJUJzeMORWgBGkGsq5mrgZMGjeDehTGhshybHIrYhrcw3KT1Q1NcvrAtmUY0G7hZ113EwkHA9mAc60A56AW6WDyaFQw4L08N3DxZQJXKoTCgjgrbEr68LNlrnWQnLbNW7gAsVPfYiKe9I8f8I5yA96P8rxeroqjeT+3Zl5+c0H+IgqRwsMgYKqy1cfoQGNIzTMBdmzIdMNUEuh23hLPqa2eh/sX1LJvP7VpwN/UlfLckfUkNLyZu2fyUagon1+Y9ULErbw2U8iXLC8OkOM9J/ACF+DFNu7Ba7MF/LKCuw+t2lWzt/DH0fPnxuZXKq/1Dr/XPs4VTwp5vYUeEr+e9hN7upq//+yLitPRU+Gjb5WdTq2z/1ykvLLb1obFw0f3DIUfcE4uOZ5/wP97V/uL/ud+0PeGtj3x67EDW46PMdJLI8N/ica7yUe3DR8pfX3wk51v8mvK9j09op3h8dDL+9fC7YP9j3zQ+t29Gy4uP1MUuXBh6UPom+axkqbVa6o/PLFg9NOF2y80s4OD7Wcv1e544dKlni+XfbX2p0X7v/628v0nY/fv8t91bPHjvncCFb+A8wdPH9rzOes5snvfqPnGsysWr7qyYGzvuJYaKREurhwq6C6MhahjK/b3LTs8/szSzp83f9w98vzQzoP++pHiFllVNpUEo1vP/CH/VtN46Kw+3kefO/lqsOVyZk//Bq7bhiGsEwAA");

            return mockOauth;
        }

        public Mock<IProductRepository> GetRepo()
        {
            var mockRepo = new Mock<IProductRepository>();

            var products = new Product[]
            {
                new() { Name = "P1", EbayProductId = "1", ProductId = 1, ItemCountry = "AM" },
                new() { Name = "P2", EbayProductId = "2", ProductId = 2, ItemCountry = "AM" },
                new() { Name = "P3", EbayProductId = "3", ProductId = 3, ItemCountry = "AM" },
                new() { Name = "P4", EbayProductId = "4", ProductId = 4, ItemCountry = "AM" },
            };

            mockRepo
                .Setup(repo => repo.Products)
                .Returns(products.AsQueryable());

            return mockRepo;
        }

        //public Mock<IOutputCacheStore> GetRedisCache()
        //{
        //    var mockRedis = new Mock<CacheService>();

        //    mockRedis.Setup(cache => cache.)
        //}

        //[Fact]
        //public async void Can_Get_Item()
        //{
        //    // Arrange
        //    var mockUserManager = GetUserManager();
        //    var mockEbayOauth = GetEbayToken();
        //    var mockContext = GetRepo();
        //    var mockRedisCache = GetRedisCache();

        //    var mockEbayService = new Mock<IEbayService>();


        //    List<string> Scopes = new List<string>()
        //    {
        //        "https://api.ebay.com/oauth/api_scope"
        //    };

        //    // Act
        //    EbayController ebay = new EbayController(
        //        mockEbayOauth.Object,
        //        new EbayService(new HttpClient()), 
        //        mockContext.Object, 
        //        mockUserManager.Object, 
        //        new, 
        //        null);

        //    Product? result = ((await ebay.GetItem("v1|315684160193|0") as ViewResult)?
        //        .ViewData.Model as Product) ?? null;


        //    // Assert
        //    Assert.True(result != null);
        //    Assert.Equal("Genuine Audi RS5 (B9) Blackline Fender Badge Door Emblems 4-Set Black", result.Name, );
        //    Assert.NotEqual("Used", result.Description);
        //    Assert.Equal("New", result.Description);
        //}

    }
}
