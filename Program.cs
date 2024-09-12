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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<DataContext>(opts =>
{
	opts.UseSqlServer(
		builder.Configuration["ConnectionStrings:StoreConnection"]);
});

builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<GetLocation>();
builder.Services.AddScoped<OAuth2Api>();

builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));

builder.Services.AddSingleton<IHttpContextAccessor,
    HttpContextAccessor>();

builder.Services.AddTransient<IBraintreeService,
    BraintreeService>();


// setting up Identity
builder.Services.AddDbContext<IdentityContext>(opts =>
{
    opts.UseSqlServer(
        builder.Configuration["ConnectionStrings:IdentityConnection"]);
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
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
    opts.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";
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

app.UseStaticFiles();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

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
    .ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();


CredentialUtil.Load("D:\\files\\aspnet\\Store\\ebay.yml");

await SeedData.SeedDatabase(new HttpClient(), context, userManager);

app.Run(); 
