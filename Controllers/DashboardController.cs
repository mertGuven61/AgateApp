using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgateApp.Data;
using AgateApp.Models;

namespace AgateApp.Controllers
{
	public class DashboardController : Controller
	{
		private readonly AgateDbContext _context;

		public DashboardController(AgateDbContext context)
		{
			_context = context;
		}

		// Logic: Load Clients -> If Client selected, load Campaigns -> If Campaign selected, load Adverts
		public async Task<IActionResult> Index(int? clientId, int? campaignId)
		{
			var viewModel = new DashboardViewModel();

			// 1. Always load all clients for the first column
			viewModel.Clients = await _context.Clients.ToListAsync();

			// 2. If a client is selected, load their campaigns
			if (clientId.HasValue)
			{
				viewModel.SelectedClientId = clientId;
				viewModel.Campaigns = await _context.Campaigns
					.Where(c => c.ClientId == clientId.Value)
					.ToListAsync();
			}

			// 3. If a campaign is selected, load its adverts
			if (campaignId.HasValue)
			{
				viewModel.SelectedCampaignId = campaignId;
				viewModel.Adverts = await _context.Adverts
					.Where(a => a.CampaignId == campaignId.Value)
					.ToListAsync();
			}

			return View(viewModel);
		}
	}
}