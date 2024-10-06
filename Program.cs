using Microsoft.EntityFrameworkCore;
using Store.Models;
using Store.Controllers;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Identity;
using Store.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Azure.Extensions.AspNetCore.DataProtection.Keys;
using Microsoft.AspNetCore.DataProtection.AzureKeyVault;
using eBay.ApiClient.Auth.OAuth2;
using Microsoft.AspNetCore.Antiforgery;
using Yandex.Checkout.V3;
using Vite.AspNetCore;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddViteServices(opts =>
{
    opts.Manifest = "dir/.vite/manifest.json";
});

builder.Services.AddDbContext<DataContext>(opts =>
{
	opts.UseSqlServer(
		builder.Configuration["ConnectionStrings:StoreConnection"]);
});

builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<GetLocation>();
builder.Services.AddScoped<OAuth2Api>();

builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IHttpContextAccessor,
    HttpContextAccessor>();

builder.Services.AddTransient<IBraintreeService,
    BraintreeService>();


builder.Services.AddOutputCache(opts =>
{
    opts.AddPolicy("default", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(4));
    });
});

// configure anti-forgery
builder.Services.Configure<AntiforgeryOptions>(opts => {
    opts.HeaderName = "X-XSRF-TOKEN";
});

// setting up Identity
builder.Services.AddDbContext<IdentityContext>(opts =>
{
    opts.UseSqlServer(
        builder.Configuration["ConnectionStrings:IdentityConnection"]);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityContext>();

builder.Services.AddControllersWithViews();

builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireDigit = false;
    opts.User.RequireUniqueEmail = true;
});

builder.Services.AddAzureClients(opts =>
{
    opts.AddBlobServiceClient(builder.Configuration["ConnectionStrings:BlobConnection"]);
});

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

app.UseOutputCache();

app.UseStaticFiles();
app.UseSession();

//IAntiforgery antiforgery = app.Services.GetRequiredService<IAntiforgery>();

//app.Use(async (context, next) => {
//    if (!context.Request.Path.StartsWithSegments("/api"))
//    {
//        string? token =
//        antiforgery.GetAndStoreTokens(context).RequestToken;
//        if (token != null)
//        {
//            context.Response.Cookies.Append("XSRF-TOKEN",
//            token,
//            new CookieOptions { HttpOnly = false });
//        }
//    }
//    await next();
//});

app.UseAuthentication();
app.UseAuthorization();

// WebSockets support is required for HMR (hot module reload).
// Uncomment the following line if your pipeline doesn't contain it.
// app.UseWebSockets();
// Enable all required features to use the Vite Development Server.
// Pass true if you want to use the integrated middleware.

app.MapControllerRoute("catpage",
    "{categoryId}/Page{productPage:int}",
    new { Controller = "Home", action = "Products" });

app.MapControllerRoute("page",
    "Page{productPage:int}",
    new { Controller = "Home", action = "Products", productPage = 1 });

app.MapDefaultControllerRoute();
app.MapRazorPages();

IdentitySeedData.CreateAdminAccount(app.Services, app.Configuration);

var context = app.Services.CreateScope()
	.ServiceProvider.GetRequiredService<DataContext>();

var userManager = app.Services.CreateScope()
    .ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

var blob = app.Services.CreateScope()
    .ServiceProvider.GetRequiredService<BlobStorageService>();

using (Stream fileStream = await blob.GetFileStreamAsync("ebay.yml"))
{
    using (StreamReader streamReader = new StreamReader(fileStream))
    {
        CredentialUtil.Load(streamReader);
    }
}

await SeedData.SeedDatabase(new HttpClient(), context, userManager);


app.Run(); 
