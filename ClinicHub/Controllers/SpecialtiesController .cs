using ClinicHub.DTOs.Specialties;
using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ClinicHub.Controllers
{
    [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
    public class SpecialtiesController : Controller
    {
        public readonly ClinicDbContext db;
        public SpecialtiesController(ClinicDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index(string? search)
        {
            // AsNoTracking:
            //أنا فقط أريد قراءة البيانات، ولن أعدل الـ Entity
            var query = db.Specialties.AsNoTracking() 
                .Select(s => new SpecialtyListDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IconOrImagePath = s.IconOrImagePath,
                    IsActive = s.IsActive,
                    DoctorCount = s.Doctors.Count()
                });

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }

            var specialties = await query.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Search = search;

            return View(specialties);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var specialty = await db.Specialties.AsNoTracking().Where(s => s.Id == id)
                .Select(s => new SpecialtyListDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IconOrImagePath = s.IconOrImagePath,
                    IsActive = s.IsActive,
                    DoctorCount = s.Doctors.Count()
                })
                .FirstOrDefaultAsync(); // s => s.Id == id

            if (specialty == null)
                return NotFound();

            return View(specialty);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SpecialtyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Check for duplicate specialty name
            var existingSpecialty = await db.Specialties.AnyAsync(s => s.Name.ToLower() == dto.Name.ToLower());
            if (existingSpecialty)
            {
                ModelState.AddModelError("Name", "A specialty with this name already exists.");
                return View(dto);
            }

            // upload image if provided
            string? imagePath = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/specialties");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }

                imagePath = $"/images/specialties/{fileName}";
            }

            var specialty = new Specialty
            {
                Name = dto.Name,
                Description = dto.Description,
                IconOrImagePath = imagePath,
                IsActive = dto.IsActive
            };
            db.Specialties.Add(specialty);
            await db.SaveChangesAsync();
            TempData["Success"] = "Specialty created successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var specialty = await db.Specialties.FindAsync(id);

            if (specialty == null)
                return NotFound();

            var dto = new SpecialtyEditDto
            {
                Id = specialty.Id,
                Name = specialty.Name,
                Description = specialty.Description,
                IconOrImagePath = specialty.IconOrImagePath,
                IsActive = specialty.IsActive
            };

            return View(dto);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, SpecialtyEditDto dto)
        {
            if (id != dto.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(dto);

            var specialty = await db.Specialties.FindAsync(id);
            if (specialty == null)
                return NotFound();

            // Check for duplicate specialty name
            var existingSpecialty = await db.Specialties.AnyAsync(s => s.Id != id && s.Name.ToLower() == dto.Name.ToLower());
            if (existingSpecialty)
            {
                ModelState.AddModelError("Name", "A specialty with this name already exists.");
                return View(dto);
            }

            // upload new image if provided
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/specialties");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }

                specialty.IconOrImagePath = $"/images/specialties/{fileName}";
            }


            specialty.Name = dto.Name.Trim();
            specialty.Description = dto.Description;
            specialty.IsActive = dto.IsActive;

            await db.SaveChangesAsync();
            TempData["Success"] = "Specialty updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var specialty = await db.Specialties.Include(s => s.Doctors).FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
                return NotFound();

            // Cannot delete while doctors are linked
            if (specialty.Doctors.Any())
            {
                TempData["Error"] = $"This specialty cannot be deleted because {specialty.Doctors.Count} doctor(s) are linked to it.";
                return RedirectToAction("Index");
            }

            db.Specialties.Remove(specialty);
            await db.SaveChangesAsync();
            TempData["Success"] = "Specialty deleted successfully.";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ToggleStatus(int id)
        {
            var specialty = await db.Specialties.FindAsync(id);

            if (specialty == null)
                return NotFound();
            
            specialty.IsActive = !specialty.IsActive;
            
            await db.SaveChangesAsync();
            if (specialty.IsActive)
            {
                TempData["Success"] = "Specialty activated successfully.";
                TempData["SuccessType"] = "success";
            }
            else
            {
                TempData["Success"] = "Specialty deactivated successfully.";
                TempData["SuccessType"] = "danger";
            }

            return RedirectToAction("Index");
        }
    }
}
