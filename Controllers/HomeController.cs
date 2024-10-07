﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Models;
using Store.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Store.Infrastructure;

namespace Store.Controllers
{
	[Authorize]
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private IProductRepository context;
		private BlobStorageService _blobStorageService;
        private UserManager<ApplicationUser> userManager;

        public int PageSize = 6;

        public HomeController(IProductRepository dataContext,
            BlobStorageService blobStorageService,
			UserManager<ApplicationUser> userMgr)
		{
			context = dataContext;
			_blobStorageService = blobStorageService;
			userManager = userMgr;
        }

        public async Task<IActionResult> Index()
        {
            List<string> imgList = new List<string>
            {
                "all_store_parts.jpg", "exterior_parts.jpg", "parts_accessories.webp",
                "car_track_parts.jpg", "motorcycle_parts.webp"
            };
            ViewBag.CategoryList = imgList;
            foreach (var cat in context.Categories.ToArray())
            {
                Console.WriteLine("cat: " + cat.Name);
            }
            return View(context.Categories.ToArray());
        }

        public IActionResult Form(long? id)
        {
            ViewBag.Categories = context.Categories.ToArray();
            if (id.HasValue && id.Value > 0)
            {
                Product? product = context.Products
                    .Include(c => c.Category)
                    .FirstOrDefault(p => p.ProductId == id.Value);
                if (product != null)
                {
                    //TempData["id"] = product.ProductId.ToString();
                    return View("Form", product);
                }
            }
            return RedirectToAction(nameof(Products));
        }

        public IActionResult Create()
        {
            ViewBag.Categories = context.Categories.ToArray();

            return View("Form", new Product() { Name = "" });
        }

        public IActionResult Delete(long id)
        {
            var prodToDel = context.Products
                .FirstOrDefault(p => p.ProductId == id);
            if (prodToDel != null)
            {
                context.DeleteProduct(prodToDel);
            }
            return RedirectToAction(nameof(Products));
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> NewProduct(long id, Product product)
        //{
        //    ViewBag.Categories = context.Categories.ToArray();

        //    if (ModelState.IsValid)
        //    {
        //        if (product.ImageFile != null)
        //        {
        //            string newImageLink = await _blobStorageService.UploadFileAsync(product.ImageFile);
        //            product.ImageLink = newImageLink;
        //        }
        //        else
        //        {
        //            product.ImageLink = "";
        //        }
        //        product.UserId = (await userManager.GetUserAsync(User)).Id;
        //        context.AddProduct(product);
        //        return RedirectToAction(nameof(Products));
        //    }
        //    else
        //    {
        //        return View("Form", new Product { Name = "" });
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> ChangeProduct(long id,[FromForm] Product product)
        //{

        //    Product? prodToChange = context.Products
        //        .Include(p => p.Category)
        //        .Where(p => p.ProductId == id)
        //        .FirstOrDefault();
        //    if (ModelState.IsValid)
        //    {
        //        prodToChange.Name = product.Name;
        //        prodToChange.Price = product.Price;
        //        prodToChange.CategoryId = product.CategoryId;
        //        prodToChange.Description = product.Description;

        //        if (product.ImageFile != null)
        //        {
        //            string newImageLink = await _blobStorageService.UploadFileAsync(product.ImageFile);
        //            prodToChange.ImageLink = newImageLink;
        //        }
        //        context.SaveProduct(prodToChange);
        //        return RedirectToAction(nameof(Index), new { id = prodToChange.ProductId });
        //    }
        //    else
        //    {
        //        ViewBag.Categories = context.Categories.ToArray();
        //        return View("Form", prodToChange);
        //    }
        //}

        public async Task<IActionResult> Products(long categoryId = 6000, int productPage = 1)
        {
            ViewBag.CurrentUser = await userManager.GetUserAsync(User);
            Product[] pr = context.Products
                .Include(p => p.Category).ToArray();
            
            var prods = pr.OrderBy(p => p.ProductId)
                .Where(p => (p.Category.EbayCategoryId == categoryId)
                    && p.Deleted == false)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize)
                .ToArray();

            return View(new ProductTarget<Product>
            {
                Products = prods,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = productPage,
                    ItemsPerPage = PageSize,
                    ItemsCount = pr.Where(p => 
                        (p.Category.EbayCategoryId == categoryId)
                        && p.Deleted == false).Count(),
                    CurrentCategory = categoryId
                },
            });
        }
    }
}