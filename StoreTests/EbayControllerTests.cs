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

namespace StoreTests
{
    public class TestOAuthToken : OAuthResponse
    {
        public new string Token { get; set; }  // Use 'new' to hide the base class member
    }


    public class EbayControllerTests
    {
        [Fact]
        public async void Can_Get_Item()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
               .UseInMemoryDatabase(databaseName: "FakeConnectionString2")
               .Options;

            using (var context = new DataContext(options))
            {
                CredentialUtil.Load(@"D:\files\aspnet\Store\ebay.yml");
                Mock<OAuth2Api> mockOauth = new Mock<OAuth2Api>();
                Mock<HttpClient> mockClient = new Mock<HttpClient>();

                var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                   Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

                ApplicationUser appUser = new ApplicationUser { Id = "test-user-id", UserName = "admin" };
                mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                    .ReturnsAsync(appUser);

                mockUserManager.Setup(um => um.AddToRoleAsync(appUser, "admin"))
                    .ReturnsAsync(IdentityResult.Success);

                IProductRepository repo = new EFProductRepository(context, mockUserManager.Object);

                var products = new Product[]
                {
                    new() { Name = "P1", EbayProductId = "1", ProductId = 1, ItemCountry = "AM" },
                    new() { Name = "P2", EbayProductId = "2", ProductId = 2, ItemCountry = "AM" },
                    new() { Name = "P3", EbayProductId = "3", ProductId = 3, ItemCountry = "AM" },
                    new() { Name = "P4", EbayProductId = "4", ProductId = 4, ItemCountry = "AM" },
                };

                context.AddRange(products);
                context.SaveChanges();

                List<string> Scopes = new List<string>()
                {
                    "https://api.ebay.com/oauth/api_scope"
                };

                // Act
                EbayController ebay = new EbayController(mockOauth.Object,
                    null, mockClient.Object, repo, mockUserManager.Object, null);

                Product? result = ((await ebay.GetItem("v1|315684160193|0") as ViewResult)?
                    .ViewData.Model as Product) ?? null;

                //Product? result2 = ((await ebay.GetItem("1") as ViewResult)?
                //    .ViewData.Model as Product) ?? null;

                // Assert
                Assert.True(result != null);
                Assert.Equal(result.Name, "Genuine Audi RS5 (B9) Blackline Fender Badge Door Emblems 4-Set Black");
                Assert.NotEqual(result.Description, "Used");
                Assert.Equal(result.Description, "New");
            }
        }

        [Fact]
        public async void Can_Search_Items()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
               .UseInMemoryDatabase(databaseName: "FakeConnectionString3")
               .Options;

            using (var context = new DataContext(options))
            {
                CredentialUtil.Load(@"D:\files\aspnet\Store\ebay.yml");
                Mock<OAuth2Api> mockOauth = new Mock<OAuth2Api>();
                Mock<HttpClient> mockClient = new Mock<HttpClient>();

                var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                   Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

                ApplicationUser appUser = new ApplicationUser { Id = "test-user-id", UserName = "admin" };
                mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                    .ReturnsAsync(appUser);

                mockUserManager.Setup(um => um.AddToRoleAsync(appUser, "admin"))
                    .ReturnsAsync(IdentityResult.Success);

                IProductRepository repo = new EFProductRepository(context, mockUserManager.Object);

                var products = new Product[]
                {
                    new() { Name = "P1", EbayProductId = "1", ProductId = 1, ItemCountry = "AM" },
                    new() { Name = "P2", EbayProductId = "2", ProductId = 2, ItemCountry = "AM" },
                    new() { Name = "P3", EbayProductId = "3", ProductId = 3, ItemCountry = "AM" },
                    new() { Name = "P4", EbayProductId = "4", ProductId = 4, ItemCountry = "AM" },
                };

                context.AddRange(products);
                context.SaveChanges();

                List<string> Scopes = new List<string>()
                {
                    "https://api.ebay.com/oauth/api_scope"
                };

                var mem = new Mock<IMemoryCache>();

                // Act
                EbayController ebay = new EbayController(mockOauth.Object,
                    null, mockClient.Object, repo, mockUserManager.Object, mem.Object);

                var result = ((await ebay.Results("dt parts", 6000, 10, 1000) as ViewResult)?
                    .ViewData.Model as IEnumerable<JToken>) ?? null;
                JToken? item = result.FirstOrDefault();

                // Assert
                Assert.True(item != null);
                Assert.InRange(result.Count(), 3, 10);

                Assert.Contains("v1", item["itemId"].ToString());
            }
        }
    }
}
