using ClinicHub.DTOs.Appointment;
using ClinicHub.DTOs.Patient;
using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using ClinicHub.Models.Content;
using ClinicHub.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers
{
    public class PatientsController : Controller
    {
        private readonly ClinicDbContext db;
        public PatientsController(ClinicDbContext db)
        {
            this.db = db;
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Index(string? search)
        {
            var query = db.Patients
                .Include(p => p.User)
                .AsQueryable();

            // Search by patient name, email, or phone
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.User.FullName.Contains(search) ||
                    p.User.Email.Contains(search) ||
                    p.User.Phone.Contains(search));
            }

            var patients = await query.OrderBy(p => p.User.FullName)
                .Select(p => new PatientListDto
                {
                    Id = p.Id,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    IsActive = p.User.IsActive,
                    CreatedAt = p.User.CreatedAt,
                    ImagePath = p.ImagePath
                })
                .ToListAsync();

            ViewBag.Search = search;

            return View(patients);
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Details(int id)
        {
            var patient = await db.Patients
                .Include(p => p.User).Where(p => p.Id == id)
                .Select(p => new PatientListDto
                {
                    Id = p.Id,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    ImagePath = p.ImagePath,
                    IsActive = p.User.IsActive,
                    CreatedAt = p.User.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var patient = await db.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            // Activate or deactivate the patient account
            patient.User.IsActive = !patient.User.IsActive;

            await db.SaveChangesAsync();

            TempData["Success"] = patient.User.IsActive
                ? "Patient activated successfully."
                : "Patient deactivated successfully.";

            return RedirectToAction("Index");
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            var patient = await db.Patients.Include(p => p.User).Include(p => p.Appointments).FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();

            // Cannot delete while appointments are linked
            if (patient.Appointments.Any())
            {
                TempData["Error"] =
                    $"This patient cannot be deleted because {patient.Appointments.Count} appointment(s) are linked to this patient.";
                return RedirectToAction(nameof(Index));
            }

            var user = patient.User;

            db.Patients.Remove(patient);
            if (user != null)
                db.Users.Remove(user);

            await db.SaveChangesAsync();

            TempData["Success"] = "Patient deleted successfully.";
            TempData["SuccessType"] = "danger";


            return RedirectToAction("Index");
        }

        // ==================================
        // Start of Patient Section
        // ==================================

        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await db.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.Specialty)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound();

            var contactMessages = await db.ContactMessages
                .Where(m =>
                    m.Email.ToLower() == patient.User.Email.ToLower() ||
                    m.FullName.ToLower() == patient.User.FullName.ToLower())
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var testimonials = await db.Testimonials
                .Where(t =>
                    t.PatientName.ToLower() == patient.User.FullName.ToLower())
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var model = new PatientProfileDto
            {
                Id = patient.Id,
                FullName = patient.User.FullName,
                Email = patient.User.Email,
                Phone = patient.User.Phone,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                IsActive = patient.User.IsActive,
                ImagePath = patient.ImagePath,

                ContactMessages = contactMessages,
                Testimonials = testimonials,

                Appointments = patient.Appointments
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => new PatientProfileDto.PatientAppointmentDto
                    {
                        Id = a.Id,
                        DoctorName = a.Doctor.User.FullName,
                        SpecialtyName = a.Doctor.Specialty?.Name,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Notes = a.Notes,
                        Status = (AppointmentStatus)a.Status
                    })
                    .ToList()
            };

            return View(model);
        }

        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpPost]
        public async Task<IActionResult> EditProfile(PatientProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await db.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound();

            patient.User.FullName = dto.FullName;
            patient.User.Email = dto.Email;
            patient.User.Phone = dto.Phone;

            patient.DateOfBirth = dto.DateOfBirth;
            patient.Gender = dto.Gender;


            // Upload Image if provided

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/patients");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Delete old image
                if (!string.IsNullOrEmpty(patient.ImagePath))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        patient.ImagePath
                            .TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                    );

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // New image name
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                // Save image
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(fileStream);
                }

                // Save image path in database
                patient.ImagePath = $"/images/patients/{fileName}";
            }

            await db.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction("Profile", new { activeTab = "info" });
        }

        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpPost]
        public async Task<IActionResult> AddTestimonial(string message, byte rating)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await db.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Please enter your review / testimonial message.";

                return RedirectToAction("Profile");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please select a rating between 1 and 5.";

                return RedirectToAction("Profile");
            }

            var testimonial = new Testimonial
            {
                PatientName = patient.User.FullName,
                Message = message.Trim(),
                Rating = rating,

                // Use patient's existing profile image
                ImagePath = patient.ImagePath,

                // Admin must approve it
                IsVisible = false,

                CreatedAt = DateTime.Now
            };

            db.Testimonials.Add(testimonial);
            await db.SaveChangesAsync();

            TempData["Success"] =
                "Your testimonial has been submitted successfully and is awaiting approval.";

            return RedirectToAction(nameof(Profile));
        }

        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string subject, string message)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await db.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Message text is required.";
                return RedirectToAction(nameof(Profile));
            }

            var contactMessage = new ContactMessage
            {
                FullName = patient.User.FullName,
                Email = patient.User.Email,
                Subject = string.IsNullOrWhiteSpace(subject)
                    ? "Message from Patient Profile"
                    : subject.Trim(),
                Message = message.Trim(),
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            db.ContactMessages.Add(contactMessage);
            await db.SaveChangesAsync();

            TempData["Success"] =
                "Your message has been sent to the clinic successfully.";

            return RedirectToAction(nameof(Profile));
        }
    }
}
