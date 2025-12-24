using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgateApp.Data;
using AgateApp.Models;
using AgateApp.Services;

namespace AgateApp.Controllers
{
    public class AdvertsController : Controller
    {
        private readonly AgateDbContext _context;
        private readonly GroqService _groqService;

        public AdvertsController(AgateDbContext context, GroqService groqService)
        {
            _context = context;
            _groqService = groqService;
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
                .Include(a => a.Notes)    // <-- BUNU MUTLAKA EKLEY�N
                .FirstOrDefaultAsync(m => m.Id == id);

            if (advert == null) return NotFound();

            // Notlar� tarihe g�re tersten s�ralayal�m (en yeni en �stte)
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
                    AuthorName = User.Identity.Name ?? "Unknown Staff", // Giri� yapan kullan�c�
                    CreatedAt = DateTime.Now
                };

                _context.Add(note);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = advertId });
        }

        // AI ile Açıklama Oluşturma
        [HttpPost]
        public async Task<IActionResult> GenerateAIDescription([FromBody] GenerateDescriptionRequest request)
        {
            if (request == null || request.AdvertId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz reklam ID." });
            }

            var advert = await _context.Adverts
                .Include(a => a.Campaign)
                .FirstOrDefaultAsync(a => a.Id == request.AdvertId);

            if (advert == null)
            {
                return Json(new { success = false, message = $"Reklam bulunamadı. ID: {request.AdvertId}" });
            }

            try
            {
                // Groq kullan (Çok hızlı, güvenilir ve ücretsiz!)
                var description = await _groqService.GenerateAdvertDescriptionAsync(advert);
                return Json(new { success = true, description = description });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
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

    // Request model for AI description generation
    public class GenerateDescriptionRequest
    {
        public int AdvertId { get; set; }
    }
}
