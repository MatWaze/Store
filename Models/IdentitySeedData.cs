using Microsoft.AspNetCore.Identity;

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
			UserManager<IdentityUser> userManager = serviceProvider
				.GetRequiredService<UserManager<IdentityUser>>();
			RoleManager<IdentityRole> roleManager = serviceProvider
				.GetService<RoleManager<IdentityRole>>()!;

			string username = configuration["Data:AdminUser:Name"]
				?? "admin";
			string email = configuration["Data:AdminUser:Email"]
				?? "admin@example.com";
			string password = configuration["Data:AdminUser:Password"]
				?? "secret";
			string role = configuration["Data:AdminUser:Role"]
				?? "Admins";
			
			if (await userManager.FindByNameAsync(username) == null)
			{
				if (await roleManager.FindByNameAsync(email) == null)
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}

				IdentityUser user = new IdentityUser
				{
					UserName = username,
					Email = email
				};

				IdentityResult result = await userManager
					.CreateAsync(user, password);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, role);
				}
			}
		}
	}
}
