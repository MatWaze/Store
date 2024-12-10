using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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
    private readonly HttpClient httpClient;

    public ProductsController(
        IProductRepository ctx,
        HttpClient http
        )
    {
        products = ctx;
        httpClient = http;
    }

    public async Task<double> GetRate()
    {
        string res = httpClient.GetStringAsync("https://cdn.jsdelivr.net/gh/fawazahmed0/currency-api@1/latest/currencies/cny.json").Result;
        JObject json = JObject.Parse(res);
        double rate = json["cny"]["usd"].ToObject<double>();
        return rate;
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
