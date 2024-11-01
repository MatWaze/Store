using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Models;
using System.Security.Cryptography;
using System.Text;

namespace Store.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Application, Bearer")]
[ApiController]
[Route("/api/[controller]")]
public class ProductsController : ControllerBase
{
    private IProductRepository products;

    public ProductsController(IProductRepository ctx)
    {
        products = ctx;
    }

    [HttpGet(Name = "products")]
    public IAsyncEnumerable<Product> Products()
    {
        return products.Products.AsAsyncEnumerable();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Product(long id)
    {
        Product? prod = await products.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);
        if (prod == null)
        {
            return NotFound();
        }
        return Ok(prod);
    }

}
