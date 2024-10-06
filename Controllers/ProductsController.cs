using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Models;
using System.Security.Cryptography;
using System.Text;

namespace Store.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ProductsController : ControllerBase
{
    private DataContext context;

    public ProductsController(DataContext ctx)
    {
        context = ctx;
    }

    //[HttpGet]
    //public IAsyncEnumerable<Product> GetProducts()
    //{
    //    return context.Products.AsAsyncEnumerable();
    //}

    //[HttpGet("{id}")]
    //public async Task<IActionResult> GetProduct(long id)
    //{
    //    Product? prod = await context.Products.FindAsync(id);
    //    if (prod == null)
    //    {
    //        return NotFound();
    //    }
    //    return Ok(prod);
    //}

    private string VerificationToken = "a94cbd68e463cb9780e2008b1f61986110a5fd0ff8b99c9cba15f1f802ad65";
    private string EndpointUrl = "https://eafd-83-139-27-248.ngrok-free.app/api/products";
    private string NotificationsPath = "./notifications/";

    [HttpGet]
    public IActionResult VerifyEndpoint([FromQuery] string challenge_code)
    {
        // Compute the SHA-256 hash
        IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha256.AppendData(Encoding.UTF8.GetBytes(challenge_code));
        sha256.AppendData(Encoding.UTF8.GetBytes(VerificationToken));
        sha256.AppendData(Encoding.UTF8.GetBytes(EndpointUrl));
        byte[] bytes = sha256.GetHashAndReset();
        var hashHex = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        Console.WriteLine(hashHex);
        // Create the response
        var response = new
        {
            challengeResponse = hashHex
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> HandleNotification()
    {
        // Read the request body
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();

        // Save the notification to a file
        var currentDtNow = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var filePath = Path.Combine(NotificationsPath, $"{currentDtNow}_user_notification.txt");
        await System.IO.File.WriteAllTextAsync(filePath, body);

        // Return a JSON response
        var response = new
        {
            status = "Notification received"
        };
        return new ContentResult
        {
            Content = System.Text.Json.JsonSerializer.Serialize(response),
            ContentType = "application/json",
            StatusCode = 200
        };
    }

}