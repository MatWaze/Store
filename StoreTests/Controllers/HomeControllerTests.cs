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
            // Arrange
            var mockProductRepo = EbayControllerTests.GetRepo();

            var mockUserManager = EbayControllerTests.GetUserManager();


            var controller = new HomeController(mockProductRepo.Object, null, mockUserManager.Object);
            controller.PageSize = 3;

            // Act
            var products = mockProductRepo.Object.Products.ToArray();
            var categories = mockProductRepo.Object.Categories.ToArray();

            var result = (await controller
                .Products() as ViewResult)?
                .ViewData.Model as ProductTarget<Product>;

            var result2 = (await controller
                .Products(productPage: 3) as ViewResult)?
                .ViewData.Model as ProductTarget<Product>;
        
            var result3 = (await controller
                .Products(productPage: 2, categoryId: 0) as ViewResult)?
                .ViewData.Model as ProductTarget<Product>;

            PagingInfo pageInfoPage1 = result!.PagingInfo;
            Product[] prodsPage1 = result.Products.ToArray();
           
            PagingInfo pageInfoPage3 = result2!.PagingInfo;
            Product[] prodsPage3 = result2.Products.ToArray();
            
            PagingInfo pageInfoPage2 = result3!.PagingInfo;
            Product[] prodsPage2 = result3.Products.ToArray();
           
            // Assert
            Assert.Equal(7, pageInfoPage1.ItemsCount);
            Assert.Equal(7, pageInfoPage3.ItemsCount);
            Assert.Equal(4, pageInfoPage2.ItemsCount);

            Assert.Equal(products[0], prodsPage1[0]);
            Assert.Equal(products[6], prodsPage3[0]);
            Assert.Equal(products[0].Category.EbayCategoryId, prodsPage2[0].Category.EbayCategoryId);
            Assert.Equal(products[0].Category.EbayCategoryId, 0);
        }

      

    }
}
