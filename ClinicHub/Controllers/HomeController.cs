using System.Diagnostics;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using ClinicHub.Models.Content;
using ClinicHub.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClinicDbContext _db;
        public HomeController(ClinicDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _db.WebsiteSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new WebsiteSetting
                {
                    ClinicName = "ClinicHub",
                    HeroTitle = "WELCOME TO CLINICHUB",
                    HeroText = "We are dedicated to providing high-quality, compassionate healthcare for you and your family.",
                    AboutText = "ClinicHub is a modern healthcare center providing comprehensive medical care with dedicated professionals and advanced technologies.",
                    Address = "A108 Adam Street, New York, NY 535022",
                    Phone = "+1 5589 55488 55",
                    Email = "info@clinichub.com",
                    WorkingHours = "Mon - Fri: 8:00 AM - 8:00 PM"
                };
                _db.WebsiteSettings.Add(settings);
                await _db.SaveChangesAsync();
            }

            var services = await _db.Services.OrderBy(s => s.DisplayOrder).ToListAsync();
            var specialties = await _db.Specialties.Where(s => s.IsActive).Include(s => s.Doctors).ToListAsync();
            var doctors = await _db.Doctors.Where(d => d.IsActive).Include(d => d.User).Include(d => d.Specialty).ToListAsync();
            var testimonials = await _db.Testimonials.Where(t => t.IsVisible).OrderByDescending(t => t.CreatedAt).ToListAsync();

            var doctorsCount = await _db.Doctors.CountAsync(d => d.IsActive);
            var specialtiesCount = await _db.Specialties.CountAsync(s => s.IsActive);
            var servicesCount = await _db.Services.CountAsync();
            //var appointmentsCount = await _db.Appointments.CountAsync();

            var viewModel = new HomeViewModel
            {
                Settings = settings,
                Services = services,
                Specialties = specialties,
                Doctors = doctors,
                Testimonials = testimonials,
                DoctorsCount = doctorsCount > 0 ? doctorsCount : doctors.Count,
                SpecialtiesCount = specialtiesCount > 0 ? specialtiesCount : specialties.Count,
                ServicesCount = servicesCount > 0 ? servicesCount : services.Count
                // AppointmentsCount = appointmentsCount
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendContactMessage(ContactMessage model)
        {
            // A method for sending and receiving data from the server without needing to reload the entire page.
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var currentUser = await _db.Users.FindAsync(userId.Value);
                if (currentUser != null)
                {
                    if (string.IsNullOrWhiteSpace(model.FullName))
                    {
                        model.FullName = currentUser.FullName;
                    }
                    if (string.IsNullOrWhiteSpace(model.Email))
                    {
                        model.Email = currentUser.Email;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Message))
            {
                if (isAjax)
                {
                    return BadRequest("Please fill in all required fields.");
                }
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Index", new { fragment = "contact" });
            }

            model.FullName = model.FullName.Trim();
            model.Email = model.Email.Trim();
            model.Subject = model.Subject?.Trim();
            model.Message = model.Message.Trim();
            model.CreatedAt = DateTime.UtcNow;
            model.IsRead = false;
            _db.ContactMessages.Add(model);
            await _db.SaveChangesAsync();

            if (isAjax)
            {
                return Content("OK");
            }

            TempData["Success"] = "Your message has been sent successfully. Thank you!";
            return RedirectToAction("Index", new { fragment = "contact" });
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = "An error occurred while processing your request."
            });
        }

        public IActionResult AccessDenied()
        {
            return View("Error", new ErrorViewModel
            {
                Message = "You do not have permission to access this page."
            });
        }

        public IActionResult NotFoundPage()
        {
            return View("NotFound");
        }
    }
}
