using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgateApp.Data;
using AgateApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Kullanıcı kimliği için gerekli

namespace AgateApp.Controllers
{
	[Authorize]
	public class DashboardController : Controller
	{
		private readonly AgateDbContext _context;

		public DashboardController(AgateDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int? clientId, int? campaignId)
		{
			var viewModel = new DashboardViewModel();
			var userEmail = User.Identity?.Name; // Giriş yapanın emaili

			// Eğer Staff ise, sadece atandığı kampanyaların müşterilerini görsün
			if (User.IsInRole("Staff"))
			{
				// Benim atandığım kampanya ID'leri
				var myCampaignIds = await _context.CampaignAssignments
					.Where(ca => ca.StaffEmail == userEmail)
					.Select(ca => ca.CampaignId)
					.ToListAsync();

				// O kampanyaların bağlı olduğu müşteriler
				viewModel.Clients = await _context.Campaigns
					.Where(c => myCampaignIds.Contains(c.Id))
					.Select(c => c.Client)
					.Distinct()
					.ToListAsync();
			}
			else
			{
				// Admin, Manager, Contact herkesi görür
				viewModel.Clients = await _context.Clients.ToListAsync();
			}

			if (clientId.HasValue)
			{
				viewModel.SelectedClientId = clientId;

				var query = _context.Campaigns.Where(c => c.ClientId == clientId.Value);

				// Staff filtresi: Sadece atandıklarını gör
				if (User.IsInRole("Staff"))
				{
					var myCampaignIds = await _context.CampaignAssignments
						.Where(ca => ca.StaffEmail == userEmail)
						.Select(ca => ca.CampaignId)
						.ToListAsync();

					query = query.Where(c => myCampaignIds.Contains(c.Id));
				}

				viewModel.Campaigns = await query.ToListAsync();
			}

			if (campaignId.HasValue)
			{
				viewModel.SelectedCampaignId = campaignId;

				// Reklamlara özel bir atama yapmadık, kampanyaya atanan reklamı da görür.
				// Eğer reklam bazlı atama isterseniz buraya da filtre eklenir.
				viewModel.Adverts = await _context.Adverts
					.Where(a => a.CampaignId == campaignId.Value)
					.ToListAsync();
			}

			return View(viewModel);
		}
	}
}