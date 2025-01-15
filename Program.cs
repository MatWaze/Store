using Microsoft.EntityFrameworkCore;
using Store.Models;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Identity;
using Store.Infrastructure;
using eBay.ApiClient.Auth.OAuth2;
using Microsoft.AspNetCore.Antiforgery;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Azure.AI.Translation.Text;
using Azure;
using ServiceStack.Redis;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Serilog;
using FluentValidation.AspNetCore;
using Store.Pages.Account;
using Store.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom
    .Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddViteServices(opts =>
{
    opts.Manifest = "dir/.vite/manifest.json";
});

builder.Services.AddDbContext<DataContext>(opts =>
{
    //opts.UseSqlServer(
    //    builder.Configuration["ConnectionStrings:AzureStoreConnection"]);

    opts.UseNpgsql(Environment.GetEnvironmentVariable("HerokuConnection"));
});

builder.Services.AddScoped<IRedisClientAsync,
    RedisClient>(c =>
{
    var client = new RedisClient(
        builder.Configuration["RedisConnection:Host"],
        int.Parse(builder.Configuration["RedisConnection:Port"]!),
        Environment.GetEnvironmentVariable("RedisConnection")
    );
    return client;
});

builder.Services.AddScoped<IMyCacheStore,
    CacheService>();

builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<GetLocation>();
builder.Services.AddScoped<OAuth2Api>();

builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IHttpContextAccessor,
    HttpContextAccessor>();

builder.Services.AddSingleton<IEbayService,
    EbayService>();

builder.Services.AddScoped<IBraintreeService,
    BraintreeService>();

builder.Services.AddScoped<ISendEmail,
    SendEmailGoogle>();

builder.Services.AddTransient<IRazorViewToStringRenderer, 
    RazorViewToStringRenderer>();

builder.Services.AddSingleton(opts =>
{
    return new TextTranslationClient(
        new AzureKeyCredential(Environment.GetEnvironmentVariable("AzureTranslation")!)
    );
});

builder.Services.AddSingleton(opts =>
{
    return new TextAnalyticsClient(
		new Uri(builder.Configuration["AzureTranslation:LanguageEndpoint"]!),
		new AzureKeyCredential(Environment.GetEnvironmentVariable("AzureLanguage")!)
    );
});

builder.Services.AddScoped<IAzureTranslation,
    AzureTranslation>();

builder.Services.AddHostedService<CsvReaderService>();
builder.Services.AddHostedService<CsvReaderHelperUU>();
builder.Services.AddHostedService<CsvReaderHelperUU2>();

// make SeedData scoped instead of singleton
// since it depends on scoped services like IAzureTranslation
builder.Services.AddScoped<SeedData>();

builder.Services.AddScoped(provider => provider.GetRequiredService<IBraintreeService>().CreateGateway());

// Configure Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddRazorPages()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddFluentValidationAutoValidation();


builder.Services.Configure<MvcOptions>(options =>
{
	options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
		_ => "");

	options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
		_ => "");
    
	// You can add more customizations here for other DataAnnotations or validation errors
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"), // English
        new CultureInfo("ru-RU"), // Russian
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

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
    //opts.UseSqlServer(
    //builder.Configuration["ConnectionStrings:IdentityConnection"]);
    opts.UseNpgsql(Environment.GetEnvironmentVariable("HerokuIdentityConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityContext>();
builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequiredLength = 10;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequiredUniqueChars = 0;
    opts.User.RequireUniqueEmail = true;
});

builder.Services.AddAzureClients(opts =>
{
    opts.AddBlobServiceClient(Environment.GetEnvironmentVariable("BlobConnection"));
});

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(opts =>
{
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.Cookie.Name = Guid.NewGuid().ToString();
});

builder.Services.AddAuthentication(opts =>
{
    opts.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    opts.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(opts =>
{
    opts.Events.DisableAuthForPath(e => e.OnRedirectToLogin, 
        "/api", StatusCodes.Status401Unauthorized);

    opts.Events.DisableAuthForPath(e => e.OnRedirectToAccessDenied, 
        "/api", StatusCodes.Status403Forbidden);
}).AddJwtBearer(opts =>
{
    opts.RequireHttpsMetadata = false;
    opts.SaveToken = true;
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("jwtSecret")!)
        ),
        ValidateAudience = false,
        ValidateIssuer = false
    };
    opts.Events = new JwtBearerEvents
    {
        OnTokenValidated = async ctx =>
        {
            UserManager<ApplicationUser> userManager = ctx
                .HttpContext
                .RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();

            SignInManager<ApplicationUser> signInManager = ctx
                .HttpContext
                .RequestServices
                .GetRequiredService<SignInManager<ApplicationUser>>();

            string? userName = ctx.Principal?
                .FindFirst(ClaimTypes.Name)?
                .Value;

            if (userName != null)
            {
                ApplicationUser? user = await userManager.FindByNameAsync(userName);
                if (user != null)
                {
                    ctx.Principal = await signInManager
                        .CreateUserPrincipalAsync(user);
                }
            }
        }
    };
});

var app = builder.Build();

app.UseOutputCache();

app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseSession();

app.UseRequestLocalization();

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

var seedData = app.Services
    .CreateScope()
    .ServiceProvider
    .GetRequiredService<SeedData>();

await seedData.SeedDatabase();

app.Run(); 
