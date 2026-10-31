using ClinicHub.DTOs.Appointment;
using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using ClinicHub.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ClinicHub.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ClinicDbContext db;
        public AppointmentsController(ClinicDbContext db)
        {
            this.db = db;
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Index(string? search, DateOnly? date, AppointmentStatus? status)
        {
            var query = db.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
                .AsQueryable();

            // Search by Patient or Doctor name
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.Patient.User.FullName.Contains(search) ||
                    a.Doctor.User.FullName.Contains(search));
            }

            // Filter by date
            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate == date.Value);
            }

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == (byte)status.Value);
            }

            var appointments = await query.OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    SpecialtyName = a.Doctor.Specialty.Name,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = (AppointmentStatus)a.Status,
                    Notes = a.Notes
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Date = date;

            return View(appointments);
        }
        
        [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await db.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            var appointmentDto = new AppointmentListDto
            {
                Id = appointment.Id,
                PatientName = appointment.Patient.User.FullName,
                DoctorName = appointment.Doctor.User.FullName,
                SpecialtyName = appointment.Doctor.Specialty.Name,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Status = (AppointmentStatus)appointment.Status,
                Notes = appointment.Notes
            };

            return View(appointmentDto);
        }

        // Patient only Cancelled
        [AuthFilter(Roles = new[] { "Admin", "Super_admin", "Patient", "Doctor" })]
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, AppointmentStatus status, string? returnUrl)
        {
            var appointment = await db.Appointments.Include(a => a.Patient).FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Patient can only cancel his own appointment
            if (userRole == "Patient")
            {
                if (appointment.Patient.UserId != userId)
                    return Forbid();

                if (status != AppointmentStatus.Cancelled)
                    return Forbid();
            }

            var currentStatus = (AppointmentStatus)appointment.Status;

            if (!IsValidStatusChange(currentStatus, status))
            {
                TempData["Success"] = "Invalid appointment status change.";
                TempData["SuccessType"] = "danger";

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction(nameof(Index));
            }

            appointment.Status = (byte)status;
            appointment.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();

            TempData["Success"] =
                status == AppointmentStatus.Cancelled
                    ? "Appointment cancelled successfully."
                    : status == AppointmentStatus.Confirmed
                        ? "Appointment accepted successfully."
                        : status == AppointmentStatus.Rejected
                            ? "Appointment rejected successfully."
                            : "Appointment status updated successfully.";

            TempData["SuccessType"] = "success";

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        [AuthFilter(Roles = new[] { "Admin", "Super_admin", "Doctor" })]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await db.Appointments.AsNoTracking().Where(a => a.Id == id)
                .Select(a => new AppointmentListDto
                {
                    Id = a.Id,
                    PatientName = a.Patient.User.FullName,
                    DoctorName = a.Doctor.User.FullName,
                    SpecialtyName = a.Doctor.Specialty.Name,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = (AppointmentStatus)a.Status,
                    Notes = a.Notes
                }).FirstOrDefaultAsync();

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnUrl)
        {
            var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            db.Appointments.Remove(appointment);
            await db.SaveChangesAsync();

            TempData["Success"] = "Appointment deleted successfully.";
            TempData["SuccessType"] = "success";

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction("Index");
        }

        // ==================================
        // Start of Patient Booking Section
        // ==================================

        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpGet]
        public async Task<IActionResult> Book()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
                return NotFound();

            ViewBag.Doctors = await db.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.IsActive)
                .Select(d => new
                {
                    d.Id,
                    Name = d.User.FullName,
                    Specialty = d.Specialty.Name
                })
                .ToListAsync();

            var dto = new AppointmentCreateDto
            {
                PatientId = patient.Id,
                AppointmentDate = DateOnly.FromDateTime(DateTime.Today)
            };

            return View(dto);
        }

        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpPost]
        public async Task<IActionResult> Book(AppointmentCreateDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = await db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
                return NotFound();

            dto.PatientId = patient.Id;

            // Validate Doctor
            var doctorExists = await db.Doctors.AnyAsync(d => d.Id == dto.DoctorId && d.IsActive);
            if (!doctorExists)
            {
                ModelState.AddModelError("DoctorId", "Please select a valid doctor.");
            }


            // Check Schedule
            var schedule = await db.DoctorSchedules
                .FirstOrDefaultAsync(s =>
                    s.DoctorId == dto.DoctorId &&
                    s.AvailableDate == dto.AppointmentDate &&
                    s.IsAvailable &&
                    s.StartTime <= dto.AppointmentTime &&
                    s.EndTime > dto.AppointmentTime);

            if (schedule == null)
            {
                ModelState.AddModelError("AppointmentTime", "The selected time is not available for this doctor.");
            }
            else
            {
                var minutesFromStart = (dto.AppointmentTime - schedule.StartTime).TotalMinutes;
                if (minutesFromStart % schedule.SlotMinutes != 0)
                {
                    ModelState.AddModelError("AppointmentTime", "Please select one of the available appointment slots.");
                }
            }

            // Check Existing Appointment
            var appointmentExists = await db.Appointments
                .AnyAsync(a =>
                    a.DoctorId == dto.DoctorId &&
                    a.AppointmentDate == dto.AppointmentDate &&
                    a.AppointmentTime == dto.AppointmentTime &&
                    (
                        a.Status == (byte)AppointmentStatus.Pending ||
                        a.Status == (byte)AppointmentStatus.Confirmed
                    ));

            if (appointmentExists)
            {
                ModelState.AddModelError("AppointmentTime", "This appointment time is already booked.");
            }


            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = await db.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Specialty)
                    .Where(d => d.IsActive)
                    .Select(d => new
                    {
                        d.Id,
                        Name = d.User.FullName,
                        Specialty = d.Specialty.Name
                    })
                    .ToListAsync();

                return View(dto);
            }

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                AppointmentTime = dto.AppointmentTime,
                Notes = dto.Notes,
                Status = (byte)AppointmentStatus.Pending,
                CreatedAt = DateTime.Now
            };

            db.Appointments.Add(appointment);
            await db.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully.";
            TempData["SuccessType"] = "success";

            return RedirectToAction("Profile", "Patients");
        }

        // ==================================
        // Start Edit Booking Section
        // ==================================
        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var appointment = await db.Appointments.Where(a => a.Id == id && a.Patient.UserId == userId)
                .Select(a => new AppointmentEditDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Notes = a.Notes
                }).FirstOrDefaultAsync();

            if (appointment == null)
                return NotFound();

            ViewBag.Doctors = await db.Doctors.Where(d => d.IsActive)
                .Select(d => new
                {
                    Id = d.Id,
                    Name = d.User.FullName
                }).ToListAsync();

            ViewBag.Patients = await db.Patients.Where(p => p.Id == appointment.PatientId)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.User.FullName
                }).ToListAsync();

            return View(appointment);
        }

        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, AppointmentEditDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var appointment = await db.Appointments.Include(a => a.Patient)
                .FirstOrDefaultAsync(a =>  a.Id == id && a.Patient.UserId == userId);

            if (appointment == null)
                return NotFound();

            var doctorExists = await db.Doctors.AnyAsync(d => d.Id == dto.DoctorId && d.IsActive);

            if (!doctorExists)
                ModelState.AddModelError("DoctorId", "Please select a valid doctor.");

            var schedule = await db.DoctorSchedules
                .FirstOrDefaultAsync(s =>
                    s.DoctorId == dto.DoctorId &&
                    s.AvailableDate == dto.AppointmentDate &&
                    s.IsAvailable &&
                    s.StartTime <= dto.AppointmentTime &&
                    s.EndTime > dto.AppointmentTime);

            if (schedule == null)
            {
                ModelState.AddModelError("AppointmentTime","The selected time is not available.");
            }
            else if (schedule.SlotMinutes <= 0 ||
                     (dto.AppointmentTime - schedule.StartTime).TotalMinutes % schedule.SlotMinutes != 0)
            {
                ModelState.AddModelError("AppointmentTime","Please select a valid appointment slot.");
            }

            var appointmentExists = await db.Appointments
                .AnyAsync(a =>
                    a.Id != id &&
                    a.DoctorId == dto.DoctorId &&
                    a.AppointmentDate == dto.AppointmentDate &&
                    a.AppointmentTime == dto.AppointmentTime &&
                    (a.Status == (byte)AppointmentStatus.Pending || a.Status == (byte)AppointmentStatus.Confirmed));

            if (appointmentExists)
            {
                ModelState.AddModelError("AppointmentTime","This appointment time is already booked.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Doctors = await db.Doctors.Where(d => d.IsActive)
                    .Select(d => new
                    {
                        Id = d.Id,
                        Name = d.User.FullName
                    }).ToListAsync();

                return View(dto);
            }

            // Keep the appointment linked to its original patient.
            appointment.DoctorId = dto.DoctorId;
            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.AppointmentTime = dto.AppointmentTime;
            appointment.Notes = dto.Notes;
            appointment.UpdatedAt = DateTime.Now;

            await db.SaveChangesAsync();

            TempData["Success"] = "Appointment updated successfully.";
            TempData["SuccessType"] = "success";

            return RedirectToAction("Profile", "Patients");
        }
        // ==================================
        // End Edit Booking Section
        // ==================================

        // Get Available Times for Doctor
        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateOnly date)
        {
            var schedule = await db.DoctorSchedules
                .FirstOrDefaultAsync(s =>
                    s.DoctorId == doctorId &&
                    s.AvailableDate == date &&
                    s.IsAvailable);

            if (schedule == null)
            {
                return Json(new List<string>());
            }

            var bookedTimes = await db.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate == date &&
                    (a.Status == (byte)AppointmentStatus.Pending || a.Status == (byte)AppointmentStatus.Confirmed))
                .Select(a => a.AppointmentTime).ToListAsync();

            var slots = new List<string>();
            var currentTime = schedule.StartTime;

            while (currentTime < schedule.EndTime)
            {
                if (!bookedTimes.Contains(currentTime))
                {
                    slots.Add(currentTime.ToString("HH:mm"));
                }
                currentTime = currentTime.AddMinutes(schedule.SlotMinutes);
            }

            return Json(slots);
        }

        // Get Available Dates for Doctor
        [AuthFilter(Roles = new[] { "Patient" })]
        [HttpGet]
        public async Task<IActionResult> GetAvailableDates(int doctorId)
        {
            var dates = await db.DoctorSchedules
                .Where(s =>
                    s.DoctorId == doctorId &&
                    s.IsAvailable &&
                    s.AvailableDate >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(s => s.AvailableDate)
                .Select(s => s.AvailableDate)
                .Distinct()
                .ToListAsync();

            var result = dates.Select(d => new
            {
                value = d.ToString("yyyy-MM-dd"),
                text = d.ToString("dd MMM yyyy")
            });

            return Json(result);
        }

        // =======================================================
        // End of Patient Booking Section
        // =======================================================

        // Validate appointment status transition
        private bool IsValidStatusChange(AppointmentStatus currentStatus, AppointmentStatus newStatus)
        {
            if (currentStatus == newStatus)
                return false;

            return currentStatus switch
            {
                AppointmentStatus.Pending =>
                    newStatus == AppointmentStatus.Confirmed ||
                    newStatus == AppointmentStatus.Rejected ||
                    newStatus == AppointmentStatus.Cancelled,

                AppointmentStatus.Confirmed =>
                    newStatus == AppointmentStatus.Completed ||
                    newStatus == AppointmentStatus.Cancelled,

                AppointmentStatus.Completed =>
                    false,

                AppointmentStatus.Rejected =>
                    false,

                AppointmentStatus.Cancelled =>
                    false,

                _ => false
            };
        }
    }
}
