using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using ClinicHub.Models.Content;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers.Content
{
    [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
    public class TestimonialsController : Controller
    {
        private readonly ClinicDbContext db;

        public TestimonialsController(ClinicDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var testimonials = await db.Testimonials.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return View(testimonials);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var testimonial = await db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);
            if (testimonial == null)
                return NotFound();

            return View(testimonial);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var testimonial = await db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);

            if (testimonial == null)
                return NotFound();

            testimonial.IsVisible = !testimonial.IsVisible;

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Delete(int id)
        {
            var testimonial = await db.Testimonials.FirstOrDefaultAsync(t => t.Id == id);

            if (testimonial == null)
                return NotFound();


                db.Testimonials.Remove(testimonial);
                await db.SaveChangesAsync();

            TempData["Success"] = "Testimonial deleted successfully.";
            TempData["SuccessType"] = "danger";

            return RedirectToAction(nameof(Index));
        }
    }
}
