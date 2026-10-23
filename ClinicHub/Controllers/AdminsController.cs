using ClinicHub.DTOs.Admin;
using ClinicHub.Models;
using ClinicHub.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers
{
    [AuthFilter(Roles = new[] { "Super_admin" })]
    public class AdminsController : Controller
    {
        private readonly ClinicDbContext db;

        public AdminsController(ClinicDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = db.Users.Include(u => u.Role).Where(u => u.Role.Name == "Admin").AsQueryable();

            // Search by name, email, or phone
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Phone.Contains(search));
            }

            var admins = await query.OrderBy(u => u.FullName).ToListAsync();
            ViewBag.Search = search;

            return View(admins);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var admin = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role.Name == "Admin");

            if (admin == null)
                return NotFound();

            return View(admin);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var admin = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role.Name == "Admin");

            if (admin == null)
                return NotFound();

            var dto = new EditAdminDto
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                Phone = admin.Phone,
                IsActive = admin.IsActive
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditAdminDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var admin = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == dto.Id && u.Role.Name == "Admin");

            if (admin == null)
                return NotFound();

            // Check if email belongs to another user
            var emailExists = await db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != dto.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View(dto);
            }

            admin.FullName = dto.FullName;
            admin.Email = dto.Email;
            admin.Phone = dto.Phone;
            admin.IsActive = dto.IsActive;

            await db.SaveChangesAsync();
            TempData["Success"] = "Admin updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var admin = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role.Name == "Admin");

            if (admin == null)
                return NotFound();

            db.Users.Remove(admin);
            await db.SaveChangesAsync();

            TempData["Success"] = "Admin deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}
