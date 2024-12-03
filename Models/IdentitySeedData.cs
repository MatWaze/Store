using Microsoft.AspNetCore.Identity;
using Store.Models.ViewModels;

namespace Store.Models
{
	public class IdentitySeedData
	{
		public static void CreateAdminAccount(
			IServiceProvider serviceProvider,
			IConfiguration configuration)
		{
			CreateAdminAccountAsync(serviceProvider, configuration)
				.Wait();
		}

		public static async Task CreateAdminAccountAsync(IServiceProvider
			serviceProvider, IConfiguration configuration)
		{
			serviceProvider = serviceProvider.CreateScope()
				.ServiceProvider;

			/*
			When your application starts, it doesn’t have a request yet
			—it’s just getting ready to handle them. But you still want 
			to create an admin user. The problem is that UserManager<T> 
			and RoleManager<T> are Scoped, so they expect to be created 
			during a request.

			Since you're seeding data outside of a regular request, 
			you need to "pretend" there's a request happening 
			by creating a scope. A scope allows you to create 
			these Scoped services (UserManager<T> and RoleManager<T>) 
			just like they would be during a real request.
			 */
			UserManager<ApplicationUser> userManager = serviceProvider
				.GetRequiredService<UserManager<ApplicationUser>>();
			RoleManager<IdentityRole> roleManager = serviceProvider
				.GetService<RoleManager<IdentityRole>>()!;
			IdentityContext context = serviceProvider
				.GetRequiredService<IdentityContext>();

			string username = "admin";
			string email = "admin@example.com";
			string password = "Secret12345";
			string roleAdmins = "Admins";
			string roleConfirmed = "ConfirmedUsers";

			if (await userManager.FindByNameAsync(username) == null)
			{
				if (await roleManager.FindByNameAsync(roleAdmins) == null)
				{
					await roleManager.CreateAsync(new IdentityRole(roleAdmins));
				}

				if (await roleManager.FindByNameAsync(roleConfirmed) == null)
				{
					await roleManager.CreateAsync(new IdentityRole(roleConfirmed));
				}

				var addr = new AddressViewModel
				{
					Country = "Armenia",
					Region = "Yerevan",
					City = "Yerevan",
					Street = "Karp Khachvankyan",
					PostalCode = "0010",
				};

				await context.Addresses.AddAsync(addr);
				await context.SaveChangesAsync();

                ApplicationUser user = new ApplicationUser
				{
					UserName = username,
					Email = email,
					YooKassaAccessToken = null,
					FullName = "Matevos Amazarian",
					AddressId = addr.Id
				};
				
				IdentityResult result = await userManager
					.CreateAsync(user, password);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, roleAdmins);
					await userManager.AddToRoleAsync(user, roleConfirmed);
				}
			}
		}
	}
}
