using eBay.ApiClient.Auth.OAuth2;
using eBay.ApiClient.Auth.OAuth2.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Store.Controllers;
using Store.Infrastructure;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Store.Models
{
	public static class SeedData
	{
        private static async Task SeedProductsAsync(HttpClient client, string token, DataContext context,
	        JToken items2, Category cat, string adminId)
        {
			var items = items2["itemSummaries"];
            foreach (var item in items)
            {
                string ebayId = item["itemId"].ToString();

                var itemAsync = await EbayService.GetItemInfoAsync(client, token, ebayId);
                var existingProduct = await context.Products
                    .FirstOrDefaultAsync(p => p.EbayProductId == ebayId);

				var culture = new CultureInfo("en-US");
                if (existingProduct == null)
                {
	                string? quan = itemAsync["estimatedAvailabilities"]?.First()?["estimatedAvailableQuantity"]?.ToString()
						?? null;
	                string? ship = item["shippingOptions"]?.FirstOrDefault()?["shippingCost"]?["value"]?.ToString() 
	                    ?? null;
					if (!(quan == null || ship == null))
					{
						var additionalImages = item["additionalImages"];

						string imageUrls = String.Empty;
						// Loop through the additional images and extract the imageUrl
						if (additionalImages != null)
						{
							foreach (var image in additionalImages)
							{
								string imageUrl = image["imageUrl"].ToString();
								imageUrls += (imageUrl + " ");
							}
						}
						
						var newProduct = new Product
                        {
                            Name = item["title"]!.ToString(),
                            Category = cat,
                            Quantity = int.Parse(quan, culture),
                            EbayProductId = ebayId,
                            ItemCountry = item["itemLocation"]["country"].ToString()!,
                            Description = item["condition"].ToString(),
                            ImageLink = item["image"]["imageUrl"].ToString().Replace("225.", "500."),
                            Price = decimal.Parse(item["price"]["value"].ToString(), culture),
                            ShippingPrice = decimal.Parse(ship, culture),
                            UserId = adminId,
							ImageUrls = imageUrls,
						};
                        context.Products.Add(newProduct);
					}
                }
            }
            await context.SaveChangesAsync();
        }

        public static async Task SeedDatabase(HttpClient httpClient,
			DataContext context, UserManager<ApplicationUser> userManager)
		{
			await context.Database.MigrateAsync();
			
			if (context.Products.Count() == 0 &&
				context.Categories.Count() == 0)
			{
                List<string> Scopes = new List<string>()
				{
					"https://api.ebay.com/oauth/api_scope"
				};

                var o = new OAuth2Api();
				string access = o.GetApplicationToken(OAuthEnvironment.PRODUCTION, Scopes)
					.AccessToken.Token;
                ApplicationUser? admin = await userManager.FindByNameAsync("admin");
                
				Category c1 = new() { Name = "All Store Parts", EbayCategoryId = 6000 };
				Category c2 = new() { Name = "Parts & Accessories", EbayCategoryId = 6028 };
				Category c3 = new() { Name = "Exterior Parts & Accessories", EbayCategoryId = 33637 };
				Category c4 = new() { Name = "Car & Truck Parts & Accessories", EbayCategoryId = 6030 };
				Category c5 = new() { Name = "Motorcycle & Scooter Parts & Accessories", EbayCategoryId = 10063 };

				JObject items1 = await EbayService.SearchItemsAsync(httpClient, access, "dt parts", 1, 1000, 4, "6000");
                JObject items2 = await EbayService.SearchItemsAsync(httpClient, access, "parts", 1, 1000, 4, "6028");
                JObject items3 = await EbayService.SearchItemsAsync(httpClient, access, "exterior parts", 1, 1000, 5, "33637");
				JObject items4 = await EbayService.SearchItemsAsync(httpClient, access, "car parts", 1, 1000, 4, "6030");
				JObject items5 = await EbayService.SearchItemsAsync(httpClient, access, "motorcycle parts", 1, 1000, 3, "10063");

				await SeedData.SeedProductsAsync(httpClient, access, context, items1, c1, admin.Id);
				await SeedData.SeedProductsAsync(httpClient, access, context, items2, c2, admin.Id);
				await SeedData.SeedProductsAsync(httpClient, access, context, items3, c3, admin.Id);
				await SeedData.SeedProductsAsync(httpClient, access, context, items4, c4, admin.Id);
				await SeedData.SeedProductsAsync(httpClient, access, context, items5, c5, admin.Id);
			}
		}
	}
}

//string link = "https://dontknowhowtonameit1.blob.core.windows.net/web/mamazari.JPG";
//           await context.Products.AddRangeAsync(
//new Product
//{
//	Name = "Engine Oil Filter",
//	Price = 15m,
//	Quantity = 1,
//	Category = c1,
//	Description = "Essential for keeping your engine clean and running smoothly.  Traps dirt and debris from your engine oil, extending its life and preventing wear.",
//	ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/Kfz-oelfilter-muenze_95bfd727-e9cb-4089-a2be-bbbcf41ac9ae.jpg",
//	UserId = admin.Id,
//},
//new Product
//{
//	Name = "Brake Pads",
//	Price = 50m,
//	Quantity = 2,
//	Category = c2,
//	Description = "Critical for safe braking.  Provides friction to slow down your vehicle, ensuring reliable stopping power.",
//                   ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/10032598a_9d1f8cc3-df9d-43a5-94c1-791b863713cb.jpg",
//                   UserId = admin.Id,
//               },
//new Product
//{
//	Name = "Air Filter",
//	Price = 20m,
//	Quantity = 1,
//	Category = c1,
//	Description = "Keeps your engine breathing clean air. Traps dust, pollen, and other contaminants before they reach the engine, improving performance and fuel efficiency.",
//                   ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/WS-002-1-simota-air-filter-%281%29_042b9acc-a9b6-4032-960b-4e1dafbcc834.webp",
//                   UserId = admin.Id,
//               },
//new Product
//{
//	Name = "Spark Plugs",
//	Price = 10m,
//	Quantity = 3,
//	Category = c1,
//	Description = "Ignites the fuel-air mixture in your engine, ensuring smooth and efficient combustion.  Replace worn plugs for optimal performance and fuel economy.",
//                   ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/images%20%281%29_e2f54bcd-cfab-4a5d-9050-6a5696abb71f.jfif",
//                   UserId = admin.Id,
//               },
//new Product
//{
//	Name = "Wipes Bladers",
//	Price = 15m,
//	Quantity = 1,
//	Category = c3,
//	Description = "Provides clear visibility during rain or snow.  Ensure safe driving by replacing worn blades for streak-free wiping action.",
//                   ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/71cTysOgcaL._AC_UF894%2C1000_QL80__1b513fbb-6188-4a59-9795-c074a45e5236.jpg",
//                   UserId = admin.Id,
//               },
//new Product
//{
//	Name = "Brake Rotors",
//	Price = 80m,
//	Quantity = 1,
//	Category = c2,
//	Description = "The metal discs that your brake pads press against.  Ensure smooth and even braking by replacing worn or damaged rotors.",
//	ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/Brake%2BRotors_53d727cc-36f3-4c0f-8f6f-41c5304b0775.png",
//	UserId = admin.Id,
//               },
//new Product
//{
//	Name = "Brake Fluid",
//	Price = 25m,
//	Quantity = 1,
//	Category = c2,
//	Description = "Hydraulic fluid that transmits pressure from the brake pedal to the calipers.  Regularly check and replace fluid for optimal braking performance.",
//                   ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/brake-fluid-2_heroef65daa9-9d4f-499c-840c-3501ca57f0b7_3fb405e1-946a-4e7e-b52f-5c65cb3b7015.avif",
//                   UserId = admin.Id,
//               },
//new Product
//{
//	Name = "Windshield Washer Fluid",
//	Price = 10m,
//	Quantity = 2,
//	Category = c3,
//	Description = "Essential for cleaning your windshield.  Provides a clear view during rain or snow, improving visibility and safety.",
//                   ImageLink = "https://dontknowhowtonameit1.blob.core.windows.net/web/11806D-Washer-Fluid-16.9oz-1_d52a6a19-ca95-4d0e-96cd-a1dece1a7bc3.png",
//                   UserId = admin.Id,
//               }
//           );