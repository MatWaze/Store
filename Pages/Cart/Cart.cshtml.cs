using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Store.Infrastructure;
using Store.Models;
using Microsoft.AspNetCore.Http;
using NuGet.Protocol.Core.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Globalization;

namespace Store.Pages.Cart
{
    public class CartModel : PageModel
    {
        private readonly IProductRepository repo;

        public CartModel(IProductRepository rep, Models.Cart cartService)
        {
            repo = rep;
            Cart = cartService;
           
        }

        public Models.Cart? Cart { get; set; }
        public string ReturnUrl { get; set; } = "/";


        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";
        }

        public IActionResult OnPostAddToCart(string ebayProductId, string returnUrl)
        {
            Product? product = repo.Products
                .FirstOrDefault(p => p.EbayProductId == ebayProductId.ToString());
            if (product != null)
            {
                if (product.Quantity > 0)
                {
                    CartLine? cl = Cart.Lines.FirstOrDefault(c => c.Product.ProductId == product.ProductId);
                    if (cl == null || cl.Quantity < product.Quantity)
                    {
                        Cart.AddItem(product, 1); // Add to cart
                        var totalItems = Cart.Lines.Sum(cl => cl.Quantity); // Total items in the cart
                        var totalValue = Cart.Lines.Sum(cl => cl.Product.Price * cl.Quantity); // Total cart value

                        return new JsonResult(new
                        {
                            success = true,
                            totalItems = totalItems,
                            totalValue = totalValue.ToString("c", CultureInfo.GetCultureInfo("en-US")) // Format total value
                        });
                    }
                }
            }
            return new JsonResult(new { success = false });
        }


        //public IActionResult OnPost(string ebayProductId, string returnUrl)
        //{
        //    Product? product = repo.Products
        //        .FirstOrDefault(p => p.EbayProductId == ebayProductId.ToString());
        //    if (product != null)
        //    {
        //        if (product.Quantity > 0)
        //        {
        //            //product.Quantity--;
        //            //repo.SaveProduct(product);
        //            //product.Deleted = true;
        //            CartLine? cl = Cart.Lines.FirstOrDefault(c => c.Product.ProductId == product.ProductId);
        //            if (cl == null || cl.Quantity < product.Quantity)
        //            {
        //                Cart.AddItem(product, 1); // Add to cart 
        //            }

        //        }
        //    }
        //    return RedirectToPage(new { returnUrl = returnUrl });
        //}

        public IActionResult OnPostRemove(long productId, string returnUrl)
        {
            Product? product = repo.Products
                .FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                //CartLine? cart = Cart.Lines
                //    .FirstOrDefault(cl => cl.Product.ProductId == productId);
                //product.Quantity += cart.Quantity;
                //repo.SaveProduct(product);
                //product.Deleted = false;
                Cart.RemoveLine(product);
            }
            return RedirectToPage(new { returnUrl = returnUrl });
        }
    }
}