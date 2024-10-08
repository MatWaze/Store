using eBay.ApiClient.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Store.Controllers;
using Store.Infrastructure;
using Store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Yandex.Checkout.V3;

namespace StoreTests
{
    public class OrderControllerTests
    {
        public static OrderController GetController(Cart cart)
        {
            OrderController controller;

            var options = new DbContextOptionsBuilder<DataContext>()
             .UseInMemoryDatabase(databaseName: "FakeConnectionString4")
             .Options;

            using (var context = new DataContext(options))
            {
                Mock<IBraintreeService> mockBrain = new Mock<IBraintreeService>();


                var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                   Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

                ApplicationUser appUser = new ApplicationUser { Id = "test-user-id", UserName = "admin" };
                mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                    .ReturnsAsync(appUser);

                mockUserManager.Setup(um => um.AddToRoleAsync(appUser, "admin"))
                    .ReturnsAsync(IdentityResult.Success);

                IProductRepository repo = new EFProductRepository(context, mockUserManager.Object);
                IOrderRepository order = new EFOrderRepository(context);


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
                controller = new OrderController(mockBrain.Object,
                    order, repo, cart, mockUserManager.Object, null, null);
            }
            return controller;
        }

        //public OrderController ctrl = GetController();
        //[Fact]
        //public async void Cat_Create_Receipt()
        //{
        //    Cart cart = new Cart();

        //    var products = new Product[]
        //    {
        //        new() { Name = "P1", EbayProductId = "1", ProductId = 1, ItemCountry = "AM", Price = 100, Description = "new" },
        //        new() { Name = "P2", EbayProductId = "2", ProductId = 2, ItemCountry = "AM", Price = 200, Description = "used" },
        //        new() { Name = "P3", EbayProductId = "3", ProductId = 3, ItemCountry = "AM", Price = 50, Description = "new" },
        //        new() { Name = "P4", EbayProductId = "4", ProductId = 4, ItemCountry = "AM", Price = 120 },
        //    };

        //    cart.AddItem(products[0], 1);
        //    cart.AddItem(products[1], 1);

        //    List<string> Scopes = new List<string>()
        //    {
        //        "https://api.ebay.com/oauth/api_scope"
        //    };

        //    // Act
        //    OrderController controller = GetController(cart);

        //    NewReceipt result = await controller.CreateYooReceipt();
        //    var receiptItems = result.Items;

        //    // Assert
        //    Assert.Equal(result.Customer.Email, "coolmartun@gmail.com");
        //    Assert.Equal(result.Items.Count, 2);

        //    Assert.Equal(receiptItems[0].Amount.Value,
        //        cart.Lines[0].Quantity * products[0].Price);

        //    Assert.Equal(receiptItems[0].Amount.Currency, "RUB");
        //    Assert.Equal(receiptItems[0].Description, "new");
        //}
    }
}
