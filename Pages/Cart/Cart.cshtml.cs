using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Store.Infrastructure;
using Store.Models;
using Microsoft.AspNetCore.Http;

namespace Store.Pages.Cart
{
    public class CartModel : PageModel
    {
        private readonly IProductRepository repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartModel(IProductRepository rep, Models.Cart cartService,
            IHttpContextAccessor httpContextAccessor)
        {
            repo = rep;
            Cart = cartService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Models.Cart? Cart { get; set; }
        public string ReturnUrl { get; set; } = "/";

        public HttpRequest Request => _httpContextAccessor.HttpContext.Request;

        public void OnGet(string returnUrl)
        {
            ReturnUrl = returnUrl ?? "/";
        }

        public IActionResult OnPost(string ebayProductId, string returnUrl)
        {
            Product? product = repo.Products
                .FirstOrDefault(p => p.EbayProductId == ebayProductId.ToString());
            if (product != null)
            {
                if (product.Quantity > 0)
                {
                    product.Quantity--;
                    repo.SaveProduct(product);
                    //product.Deleted = true;
                    Cart.AddItem(product, 1, Request);
                }
            }
            return RedirectToPage(new { returnUrl = returnUrl });
        }

        public IActionResult OnPostRemove(long productId, string returnUrl)
        {
            Product? product = repo.Products
                .FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                CartLine? cart = Cart.Lines
                    .FirstOrDefault(cl => cl.Product.ProductId == productId);
                Console.WriteLine("cart q: " + cart.Quantity);
                product.Quantity += cart.Quantity;
                Console.WriteLine(product.Quantity);
                repo.SaveProduct(product);
                //product.Deleted = false;
                Cart.RemoveLine(product, Request);
            }
            return RedirectToPage(new { returnUrl = returnUrl });
        }
    }
}