using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AgateApp.Data
{
	public static class RoleInitializer
	{
		public static async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

			string[] roleNames = { "Admin", "CampaignManager", "ClientContact", "Staff" };
			foreach (var roleName in roleNames)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}


			// Admin
			await CreateUser(userManager, "admin@agate.com", "Admin123!", "Admin");

			// Campaign Manager (Kampanya Yöneticisi)
			await CreateUser(userManager, "manager@agate.com", "Manager123!", "CampaignManager");

			// Client Contact (Müşteri Temsilcisi)
			await CreateUser(userManager, "contact@agate.com", "Contact123!", "ClientContact");

			// Staff (Personel)
			await CreateUser(userManager, "staff@agate.com", "Staff123!", "Staff");
		}

		private static async Task CreateUser(UserManager<IdentityUser> userManager, string email, string password, string role)
		{
			var user = await userManager.FindByEmailAsync(email);
			if (user == null)
			{
				var newUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
				var result = await userManager.CreateAsync(newUser, password);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(newUser, role);
				}
			}
		}
	}
}