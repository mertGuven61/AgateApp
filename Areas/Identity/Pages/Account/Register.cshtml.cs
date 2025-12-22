using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için gerekli
using Microsoft.Extensions.Logging;

namespace AgateApp.Areas.Identity.Pages.Account
{
	[Authorize(Roles = "Admin")] // Sadece Adminler erişebilir
	public class RegisterModel : PageModel
	{
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IUserStore<IdentityUser> _userStore;
		private readonly RoleManager<IdentityRole> _roleManager; // Rolleri yönetmek için

		public RegisterModel(
			UserManager<IdentityUser> userManager,
			IUserStore<IdentityUser> userStore,
			SignInManager<IdentityUser> signInManager,
			RoleManager<IdentityRole> roleManager) // Constructor'a ekledik
		{
			_userManager = userManager;
			_userStore = userStore;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		[BindProperty]
		public InputModel Input { get; set; }

		public string ReturnUrl { get; set; }

		// Rol listesini View'a göndermek için
		public SelectList RolesList { get; set; }

		public class InputModel
		{
			[Required]
			[Display(Name = "Username")]
			public string Username { get; set; }

			[Required]
			[EmailAddress]
			[Display(Name = "Email")]
			public string Email { get; set; }

			[Required]
			[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
			[DataType(DataType.Password)]
			[Display(Name = "Password")]
			public string Password { get; set; }

			[DataType(DataType.Password)]
			[Display(Name = "Confirm password")]
			[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
			public string ConfirmPassword { get; set; }

			// YENİ EKLENEN: Seçilen Rol
			[Required]
			[Display(Name = "Assign Role")]
			public string Role { get; set; }
		}

		public void OnGet(string returnUrl = null)
		{
			ReturnUrl = returnUrl;

			// Veritabanındaki tüm rolleri çekip listeye atıyoruz
			// Admin, Staff, CampaignManager, ClientContact vs.
			RolesList = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
		}

		public async Task<IActionResult> OnPostAsync(string returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");

			// Post işlemi başarısız olursa liste kaybolmasın diye tekrar yüklüyoruz
			RolesList = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");

			if (ModelState.IsValid)
			{
				var user = CreateUser();

				await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
				await ((IUserEmailStore<IdentityUser>)_userStore).SetEmailAsync(user, Input.Email, CancellationToken.None);

				// Email onaylanmış varsayalım (Şirket içi olduğu için)
				user.EmailConfirmed = true;

				var result = await _userManager.CreateAsync(user, Input.Password);

				if (result.Succeeded)
				{
					// 1. KULLANICIYA SEÇİLEN ROLÜ ATA
					await _userManager.AddToRoleAsync(user, Input.Role);

					// 2. OTOMATİK GİRİŞİ KALDIRDIK (ÖNEMLİ!)
					// await _signInManager.SignInAsync(user, isPersistent: false); <- Bu satır silindi.
					// Çünkü Admin yeni kullanıcı oluştururken kendi oturumu kapanmamalı.

					// Başarılı mesajı verip sayfayı yenileyebiliriz veya listeye dönebiliriz.
					return RedirectToPage("Register"); // Sayfayı yenile (Yeni bir tane daha eklemek için)
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return Page();
		}

		private IdentityUser CreateUser()
		{
			try
			{
				return Activator.CreateInstance<IdentityUser>();
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'.");
			}
		}
	}
}