using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Store.Models;
using Store.Models.ViewModels;
using Store.Controllers;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Store.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace StoreTests
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Can_Filter_And_Paginate()
        {
            var categories = new Category[]
            {
                new Category { EbayCategoryId = 6000, Name = "c1"},
                new Category { EbayCategoryId = 6001, Name = "c2"},
            };

            var products = new Product[]
            {
                new Product { ProductId = 1, Name = "p1", Category = categories[0] },
                new Product { ProductId = 2, Name = "p2", Category = categories[0] },
                new Product { ProductId = 3, Name = "p3", Category = categories[0] },
                new Product { ProductId = 4, Name = "p4", Category = categories[0] },
                new Product { ProductId = 5, Name = "p5", Category = categories[0] },
                new Product { ProductId = 6, Name = "p6", Category = categories[1] },
                new Product { ProductId = 7, Name = "p7", Category = categories[1] },
            };

            Mock<IProductRepository> mock = new Mock<IProductRepository>();

            mock.Setup(m => m.Products)
                .Returns(products.AsQueryable<Product>());

            mock.Setup(m => m.Categories)
                .Returns(categories.AsQueryable<Category>());

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            ApplicationUser appUser = new ApplicationUser { Id = "test-user-id", UserName = "Mat" };

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(appUser);

            var controller = new HomeController(mock.Object, null, mockUserManager.Object);
            controller.PageSize = 7;

            // Act
            ProductTarget<Product>? result = (await controller.Products() as ViewResult)?
                .ViewData.Model as ProductTarget<Product>;

            // Assert
            PagingInfo? pageInfo = result?.PagingInfo;
            Product[]? prods = result?.Products?.ToArray();

            Assert.True(prods.Length == 5);
            Assert.Equal(pageInfo.ItemsCount, prods.Length + 1);
            Assert.True(pageInfo.CurrentCategory == categories[0].EbayCategoryId);
        }

        [Fact]
        public void Can_Add_Delete_Product()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "FakeConnectionString1")
                .Options;

            using (var context = new DataContext(options))
            {
                context.Products.AddRange(
                    new Product { ProductId = 1, Name = "Product 1", EbayProductId = "6000", ItemCountry = "AM" },
                    new Product { ProductId = 2, Name = "Product 2", EbayProductId = "6000", ItemCountry = "AM" },
                    new Product { ProductId = 3, Name = "Product 3", EbayProductId = "6000", ItemCountry = "AM" }
                );
                context.SaveChanges();

                // Mock UserManager
                var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                    Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

                mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(new ApplicationUser { Id = "test-user-id" });

                IProductRepository repo = new EFProductRepository(context, mockUserManager.Object);

                // Act
                var newProduct = new Product { ProductId = 4, Name = "Product 4", EbayProductId = "6000", ItemCountry = "AM" };
                var newProduct1 = new Product { ProductId = 5, Name = "Product 5", EbayProductId = "6000", ItemCountry = "AM" };
                
                repo.AddProduct(newProduct);
                repo.AddProduct(newProduct1);
                repo.DeleteProduct(newProduct1);

                // Assert
                var products = repo.Products.ToList();
                Assert.Equal(4, products.Count);
                Assert.Contains(products, p => p.ProductId == 4 && p.Name == "Product 4");
            }
        }

    }
}
