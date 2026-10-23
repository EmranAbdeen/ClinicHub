using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Content;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers.Content
{
    [AuthFilter(Roles = new[] { "Super_admin" })]
    public class WebsiteSettingsController : Controller
    {
        private readonly ClinicDbContext db;
        public WebsiteSettingsController(ClinicDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Edit()
        {
            var settings = await db.WebsiteSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new WebsiteSetting
                {
                    // Initialize default values for the website settings
                    ClinicName = "ClinicHub",
                    HeroTitle = "Welcome to ClinicHub",
                    HeroText = "We provide comprehensive and compassionate healthcare services for you and your family.",
                    AboutText = "ClinicHub is committed to delivering exceptional patient care with state-of-the-art medical equipment, highly qualified doctors, and a dedicated healthcare team.",
                    Address = "123 Main Street, Amman, Jordan",
                    Phone = "+962 7 8149 8888",
                    Email = "info@clinichub.com",
                    WorkingHours = "Sat-Thu, 9:00 AM - 11:00 PM"

                };

                db.WebsiteSettings.Add(settings);
                await db.SaveChangesAsync();
            }

            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WebsiteSetting settings)
        {
            if (!ModelState.IsValid)
                return View(settings);

            var existing = await db.WebsiteSettings.FirstOrDefaultAsync(s => s.Id == settings.Id);

            if (existing == null)
                return NotFound();

            existing.ClinicName = settings.ClinicName;
            existing.HeroTitle = settings.HeroTitle;
            existing.HeroText = settings.HeroText;
            existing.AboutText = settings.AboutText;
            existing.Address = settings.Address;
            existing.Phone = settings.Phone;
            existing.Email = settings.Email;
            existing.WorkingHours = settings.WorkingHours;

            await db.SaveChangesAsync();

            TempData["Success"] = "Website settings updated successfully.";

            return RedirectToAction("Edit");
        }
    }
}
