using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgateApp.Data;
using AgateApp.Models;

namespace AgateApp.Controllers
{
    public class AdvertsController : Controller
    {
        private readonly AgateDbContext _context;

        public AdvertsController(AgateDbContext context)
        {
            _context = context;
        }


        // GET: Adverts
        public async Task<IActionResult> Index()
        {
            var agateDbContext = _context.Adverts.Include(a => a.Campaign);
            return View(await agateDbContext.ToListAsync());
        }

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var advert = await _context.Adverts
				.Include(a => a.Campaign) // Varsa
				.Include(a => a.Notes)    // <-- BUNU MUTLAKA EKLEYÝN
				.FirstOrDefaultAsync(m => m.Id == id);

			if (advert == null) return NotFound();

			// Notlarý tarihe göre tersten sýralayalým (en yeni en üstte)
			advert.Notes = advert.Notes.OrderByDescending(n => n.CreatedAt).ToList();

			return View(advert);
		}

		// Yeni Metot: Not Ekleme
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddNote(int advertId, string noteText)
		{
			if (!string.IsNullOrWhiteSpace(noteText))
			{
				var note = new AdvertNote
				{
					AdvertId = advertId,
					NoteText = noteText,
					AuthorName = User.Identity.Name ?? "Unknown Staff", // Giriþ yapan kullanýcý
					CreatedAt = DateTime.Now
				};

				_context.Add(note);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Details), new { id = advertId });
		}

		// GET: Adverts/Create
		public IActionResult Create(int? campaignId)
        {
            if (campaignId.HasValue)
            {
                ViewBag.PreSelectedCampaignId = campaignId;

                // CampaignId for return to dashboard
                var campaign = _context.Campaigns.FirstOrDefault(c => c.Id == campaignId);
                if (campaign != null)
                {
                    ViewBag.CampaignTitle = campaign.Title;
                    ViewBag.RelatedClientId = campaign.ClientId;
                }
            }

            ViewData["CampaignId"] = new SelectList(_context.Campaigns, "Id", "Title", campaignId);
            return View();
        }

        // POST: Adverts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,MediaChannel,ProductionStatus,ScheduledRunDateStart,ScheduledRunDateEnd,Cost,CampaignId")] Advert advert)
        {
            if (ModelState.IsValid)
            {
                _context.Add(advert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CampaignId"] = new SelectList(_context.Campaigns, "Id", "Title", advert.CampaignId);
            return View(advert);
        }

        // GET: Adverts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advert = await _context.Adverts.FindAsync(id);
            if (advert == null)
            {
                return NotFound();
            }
            ViewData["CampaignId"] = new SelectList(_context.Campaigns, "Id", "Title", advert.CampaignId);
            return View(advert);
        }

        // POST: Adverts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,MediaChannel,ProductionStatus,ScheduledRunDateStart,ScheduledRunDateEnd,Cost,CampaignId")] Advert advert)
        {
            if (id != advert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(advert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertExists(advert.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CampaignId"] = new SelectList(_context.Campaigns, "Id", "Title", advert.CampaignId);
            return View(advert);
        }

        // GET: Adverts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advert = await _context.Adverts
                .Include(a => a.Campaign)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (advert == null)
            {
                return NotFound();
            }

            return View(advert);
        }

        // POST: Adverts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advert = await _context.Adverts.FindAsync(id);
            if (advert != null)
            {
                _context.Adverts.Remove(advert);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool AdvertExists(int id)
        {
            return _context.Adverts.Any(e => e.Id == id);
        }
    }
}
