using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers.Content
{
    [AuthFilter(Roles = new[] { "Admin", "Super_admin" } )]
    public class ContactMessagesController : Controller
    {
        private readonly ClinicDbContext db;
        public ContactMessagesController(ClinicDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await db.ContactMessages.OrderByDescending(m => m.CreatedAt).ToListAsync();
            return View(messages);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var message = await db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
                return NotFound();

            if (!message.IsRead)
            {
                message.IsRead = true;
                await db.SaveChangesAsync();

            }

            return View(message);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
                return NotFound();

            db.ContactMessages.Remove(message);
            await db.SaveChangesAsync();

            TempData["Success"] = "Message deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}
