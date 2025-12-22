using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgateApp.Data;
using AgateApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace AgateApp.Controllers
{
	[Authorize] // Herkes giriþ yapmak zorunda
	public class CampaignsController : Controller
	{
		private readonly AgateDbContext _context;

		public CampaignsController(AgateDbContext context)
		{
			_context = context;
		}

		// GET: Campaigns
		public async Task<IActionResult> Index()
		{
			// EÐER STAFF ÝSE: Sadece kendisine atanan kampanyalarý görsün
			if (User.IsInRole("Staff"))
			{
				var userEmail = User.Identity?.Name;

				// Benim atandýðým kampanya ID'lerini bul
				var myCampaignIds = await _context.CampaignAssignments
					.Where(ca => ca.StaffEmail == userEmail)
					.Select(ca => ca.CampaignId)
					.ToListAsync();

				// Sadece o kampanyalarý getir
				return View(await _context.Campaigns
					.Where(c => myCampaignIds.Contains(c.Id))
					.Include(c => c.Client)
					.ToListAsync());
			}

			// ADMIN, MANAGER, CLIENT CONTACT: Hepsini görsün
			var agateDbContext = _context.Campaigns.Include(c => c.Client);
			return View(await agateDbContext.ToListAsync());
		}

		// GET: Campaigns/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var campaign = await _context.Campaigns
				.Include(c => c.Client)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (campaign == null) return NotFound();

			return View(campaign);
		}

		// ==========================================================
		// CREATE (Sadece Yöneticiler)
		// ==========================================================
		[Authorize(Roles = "Admin,CampaignManager")]
		public IActionResult Create(int? clientId)
		{
			if (clientId.HasValue)
			{
				ViewBag.PreSelectedClientId = clientId;
				var client = _context.Clients.FirstOrDefault(c => c.Id == clientId);
				if (client != null) ViewBag.ClientName = client.CompanyName;
			}

			ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", clientId);
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "Admin,CampaignManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Title,PlannedStartDate,PlannedFinishDate,ActualFinishDate,EstimatedCost,Budget,ActualCost,AmountPaid,DatePaid,Status,ClientId")] Campaign campaign)
		{
			if (ModelState.IsValid)
			{
				_context.Add(campaign);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", campaign.ClientId);
			return View(campaign);
		}

		// ==========================================================
		// ASSIGN STAFF (Personel Atama - Sadece Yöneticiler)
		// ==========================================================

		// GET: Atama Sayfasýný Göster
		[Authorize(Roles = "Admin,CampaignManager")]
		public IActionResult AssignStaff(int id)
		{
			var campaign = _context.Campaigns.Find(id);
			if (campaign == null) return NotFound();

			ViewBag.CampaignTitle = campaign.Title;
			ViewBag.CampaignId = id;

			// Bu kampanyaya daha önce atanmýþ personelleri bul
			var existingAssignments = _context.CampaignAssignments
				.Where(ca => ca.CampaignId == id)
				.Select(ca => ca.StaffEmail)
				.ToList();

			ViewBag.AssignedStaff = existingAssignments;

			return View();
		}

		// POST: Atamayý Kaydet
		[HttpPost]
		[Authorize(Roles = "Admin,CampaignManager")]
		public async Task<IActionResult> AssignStaff(int campaignId, string staffEmail)
		{
			// Basit doðrulama
			if (string.IsNullOrEmpty(staffEmail)) return RedirectToAction("AssignStaff", new { id = campaignId });

			// Zaten atanmýþ mý kontrol et
			var exists = await _context.CampaignAssignments
				.AnyAsync(ca => ca.CampaignId == campaignId && ca.StaffEmail == staffEmail);

			if (!exists)
			{
				var assignment = new CampaignAssignment
				{
					CampaignId = campaignId,
					StaffEmail = staffEmail
				};
				_context.Add(assignment);
				await _context.SaveChangesAsync();
			}

			// Sayfayý yenile (Böylece listeye eklendiðini görürüz)
			return RedirectToAction("AssignStaff", new { id = campaignId });
		}

		// ==========================================================
		// EDIT (Sadece Yöneticiler)
		// ==========================================================
		[Authorize(Roles = "Admin,CampaignManager")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var campaign = await _context.Campaigns.FindAsync(id);
			if (campaign == null) return NotFound();
			ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", campaign.ClientId);
			return View(campaign);
		}

		[HttpPost]
		[Authorize(Roles = "Admin,CampaignManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Title,PlannedStartDate,PlannedFinishDate,ActualFinishDate,EstimatedCost,Budget,ActualCost,AmountPaid,DatePaid,Status,ClientId")] Campaign campaign)
		{
			if (id != campaign.Id) return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(campaign);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!CampaignExists(campaign.Id)) return NotFound();
					else throw;
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", campaign.ClientId);
			return View(campaign);
		}

		// ==========================================================
		// DELETE (Sadece Yöneticiler)
		// ==========================================================
		[Authorize(Roles = "Admin,CampaignManager")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var campaign = await _context.Campaigns
				.Include(c => c.Client)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (campaign == null) return NotFound();

			return View(campaign);
		}

		// ==========================================================
		// UPDATE PAYMENT (Sadece Müþteri Temsilcisi ve Admin)
		// ==========================================================

		[Authorize(Roles = "Admin,ClientContact")]
		public async Task<IActionResult> UpdatePayment(int? id)
		{
			if (id == null) return NotFound();

			var campaign = await _context.Campaigns
				.Include(c => c.Client) // Müþteri adýný göstermek için
				.FirstOrDefaultAsync(m => m.Id == id);

			if (campaign == null) return NotFound();

			return View(campaign);
		}

		[HttpPost]
		[Authorize(Roles = "Admin,ClientContact")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdatePayment(int id, decimal AmountPaid, DateTime? DatePaid)
		{
			// 1. Veritabanýndaki orijinal kaydý çek
			var campaignToUpdate = await _context.Campaigns.FindAsync(id);

			if (campaignToUpdate == null) return NotFound();

			// 2. Sadece Ödeme Alanlarýný Güncelle
			campaignToUpdate.AmountPaid = AmountPaid;
			campaignToUpdate.DatePaid = DatePaid;

			// Ýsterseniz: Eðer tam ödeme yapýldýysa statüyü deðiþtirebilirsiniz
			// if (campaignToUpdate.AmountPaid >= campaignToUpdate.ActualCost) campaignToUpdate.Status = "Paid";

			try
			{
				// 3. Kaydet (Entity Framework sadece deðiþen alanlarý günceller)
				_context.Update(campaignToUpdate);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CampaignExists(campaignToUpdate.Id)) return NotFound();
				else throw;
			}

			// Dashboard'a geri dön
			return RedirectToAction("Index", "Dashboard", new { clientId = campaignToUpdate.ClientId });
		}

		[HttpPost, ActionName("Delete")]
		[Authorize(Roles = "Admin,CampaignManager")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var campaign = await _context.Campaigns.FindAsync(id);
			if (campaign != null) _context.Campaigns.Remove(campaign);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool CampaignExists(int id)
		{
			return _context.Campaigns.Any(e => e.Id == id);
		}
	}
}