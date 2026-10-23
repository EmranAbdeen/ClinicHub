using ClinicHub.DTOs.Doctor;
using ClinicHub.DTOs.Doctor.DoctorProfile;
using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using ClinicHub.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ClinicDbContext db;
        public DoctorsController(ClinicDbContext db)
        {
            this.db = db;
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Index(string? search, int? specialtyId, AppointmentStatus? status)
        {
            var doctors = db.Doctors.Include(d => d.User).Include(d => d.Specialty).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                doctors = doctors.Where(d => d.User.FullName.Contains(search) || d.User.Email.Contains(search));
            }

            if (specialtyId.HasValue)
            {
                doctors = doctors.Where(d => d.SpecialtyId == specialtyId.Value);
            }

            ViewBag.Specialties = await db.Specialties.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.Search = search;
            ViewBag.SpecialtyId = specialtyId;
            ViewBag.Status = status;

            var doctorList = await doctors.ToListAsync();
            return View(doctorList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var doctor = await db.Doctors.AsNoTracking().Where(d => d.Id == id)
                .Select(d => new DoctorDetailsDto
                {
                    Id = d.Id,
                    FullName = d.User.FullName,
                    Email = d.User.Email,
                    Phone = d.User.Phone,
                    SpecialtyName = d.Specialty != null ? d.Specialty.Name : "General Practice",
                    YearsOfExperience = d.YearsOfExperience,
                    Biography = d.Biography,
                    ImagePath = d.ImagePath,
                    CreatedAt = d.User.CreatedAt,
                    IsActive = d.IsActive,
                    AppointmentCount = d.Appointments.Count()
                }).FirstOrDefaultAsync();

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Create()
        {
            await LoadSpecialties();
            return View();
        }
        
        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        [HttpPost]
        public async Task<IActionResult> Create(DoctorCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadSpecialties(dto.SpecialtyId);
                return View(dto);
            }

            // Check if the email already exists
            var emailExists = await db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                await LoadSpecialties(dto.SpecialtyId);
                return View(dto);
            }

            var doctorRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");

            if (doctorRole == null)
                return NotFound("Doctor role not found.");

            // Create a new user for the doctor
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = doctorRole.Id,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // upload image if provided

            string? imagePath = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/doctors");
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
                imagePath = $"/images/doctors/{fileName}";
            }

            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Create a new doctor record
            var doctor = new Doctor
            {
                UserId = user.Id,
                SpecialtyId = dto.SpecialtyId,
                YearsOfExperience = dto.YearsOfExperience,
                Biography = dto.Biography,
                ImagePath = imagePath,
                IsActive = true
            };

            db.Doctors.Add(doctor);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var doctor = await db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            var dto = new DoctorEditDto
            {
                Id = doctor.Id,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email,
                Phone = doctor.User.Phone,
                SpecialtyId = doctor.SpecialtyId,
                IconOrImagePath = doctor.ImagePath,
                YearsOfExperience = doctor.YearsOfExperience,
                Biography = doctor.Biography
            };

            await LoadSpecialties(dto.SpecialtyId);

            return View(dto);
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, DoctorEditDto dto)
        {
            if (id != dto.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadSpecialties(dto.SpecialtyId);
                return View(dto);
            }

            var doctor = await db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            // Update User information
            doctor.User.FullName = dto.FullName;
            doctor.User.Email = dto.Email;
            doctor.User.Phone = dto.Phone;

            //Update Doctor information
            doctor.SpecialtyId = dto.SpecialtyId;
            doctor.YearsOfExperience = dto.YearsOfExperience;
            doctor.Biography = dto.Biography;


            // Upload new image
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/doctors");
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

                doctor.ImagePath = $"/images/doctors/{fileName}";
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            doctor.IsActive = !doctor.IsActive;

            await db.SaveChangesAsync();

            if (doctor.IsActive)
            {
                TempData["Success"] = "Doctor activated successfully.";
                TempData["SuccessType"] = "success";
            }
            else
            {
                TempData["Success"] = "Doctor deactivated successfully.";
                TempData["SuccessType"] = "danger";
            }

            return RedirectToAction("Index");
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            var doctor = await db.Doctors.Include(d => d.User).Include(d => d.Appointments).FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            // Cannot delete while appointments are linked
            if (doctor.Appointments.Any())
            {
                TempData["Error"] = $"This doctor cannot be deleted because {doctor.Appointments.Count} appointment(s) are linked to this doctor.";
                return RedirectToAction("Index");
            }

            try
            {
            var user = doctor.User;

            db.Doctors.Remove(doctor);
                if (user != null)
                    db.Users.Remove(user);

            await db.SaveChangesAsync();
            TempData["Success"] = "Doctor deleted successfully.";

            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "This doctor cannot be deleted because they have available time slots (schedule) set up. Please remove the doctor's available slots first, then try again.";
            }


            return RedirectToAction("Index");
        }

        private async Task LoadSpecialties(int? selectedId = null)
        {
            ViewBag.Specialties = new SelectList(
                await db.Specialties
                    .Where(s => s.IsActive)
                    .ToListAsync(), "Id", "Name", selectedId
            );
        }

        // ======================
        // Profile
        // ======================
        [AuthFilter(Roles = new[] { "Doctor" })]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if(userId == null)
                return RedirectToAction("Login", "Account");

            var doctor = await db.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.DoctorSchedules)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Patient).ThenInclude(p => p.User)
                                    .FirstOrDefaultAsync(d => d.UserId == userId);

            if(doctor == null)
                return NotFound();

            var dto = new DoctorProfileDto
            {
                Id = doctor.Id,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email,
                Phone = doctor.User.Phone,
                Biography = doctor.Biography,
                YearsOfExperience = doctor.YearsOfExperience,
                ImagePath = doctor.ImagePath,
                SpecialtyName = doctor.Specialty.Name,

                Schedules = doctor.DoctorSchedules.Where(s => s.IsAvailable).Select(s => new DoctorScheduleDto
                {
                    Id = s.Id,
                    AvailableDate = s.AvailableDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SlotMinutes = s.SlotMinutes,
                    IsAvailable = s.IsAvailable
                }).ToList(),

                Appointments = doctor.Appointments.Select(a => new DoctorAppointmentDto
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FullName,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Notes = a.Notes,
                    Status = (AppointmentStatus)a.Status
                }).ToList()
            };

            return View(dto);
        }
        
        [AuthFilter(Roles = new[] { "Doctor" })]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var doctor = await db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound();

            var dto = new DoctorProfileEditDto
            {
                FullName = doctor.User.FullName,
                Email = doctor.User.Email,
                Phone = doctor.User.Phone,
                Biography = doctor.Biography,
                YearsOfExperience = doctor.YearsOfExperience,
                ImagePath = doctor.ImagePath
            };

            return View(dto);
        }

        [AuthFilter(Roles = new[] { "Doctor" })]
        [HttpPost]
        public async Task<IActionResult> EditProfile(DoctorProfileEditDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var doctor = await db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                dto.ImagePath = doctor.ImagePath;
                return View(dto);
            }

            // Check email
            var emailExists = await db.Users.AnyAsync(u => u.Email == dto.Email &&  u.Id != doctor.UserId);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                dto.ImagePath = doctor.ImagePath;

                return View(dto);
            }

            // Update User information
            doctor.User.FullName = dto.FullName;
            doctor.User.Email = dto.Email;
            doctor.User.Phone = dto.Phone;

            // Update Doctor information
            doctor.Biography = dto.Biography;
            doctor.YearsOfExperience = dto.YearsOfExperience;

            // Update Profile Image
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/doctors");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Delete old image
                if (!string.IsNullOrEmpty(doctor.ImagePath))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        doctor.ImagePath
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
                doctor.ImagePath = $"/images/doctors/{fileName}";
            }

            await db.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            TempData["SuccessType"] = "success";

            return RedirectToAction("Profile");
        }
    }
}
