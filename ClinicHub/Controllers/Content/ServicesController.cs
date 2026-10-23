using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Content;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers.Content
{
    [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
    public class ServicesController : Controller
    {
        private readonly ClinicDbContext db;
        public ServicesController(ClinicDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var services = await db.Services.OrderBy(s => s.DisplayOrder).ToListAsync();
            return View(services);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Service service)
        {
            if (!ModelState.IsValid)
                return View(service);

            db.Services.Add(service);
            await db.SaveChangesAsync();

            TempData["Success"] = "Service created successfully.";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (!ModelState.IsValid)
                return View(service);

            if (id != service.Id)
                return NotFound();

            db.Services.Update(service);
            await db.SaveChangesAsync();

            TempData["Success"] = "Service updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            var service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                return NotFound();

            db.Services.Remove(service);
            await db.SaveChangesAsync();

            TempData["Success"] = "Service deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}
