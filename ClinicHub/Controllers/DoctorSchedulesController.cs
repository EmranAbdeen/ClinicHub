using ClinicHub.DTOs.Doctor.DoctorProfile;
using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers
{
    [AuthFilter(Roles = new[] { "Doctor" })]
    public class DoctorSchedulesController : Controller
    {
        private readonly ClinicDbContext db;
        public DoctorSchedulesController(ClinicDbContext context)
        {
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            var doctor = await GetCurrentDoctor();

            if (doctor == null)
                return NotFound();

            var schedules = await db.DoctorSchedules
                .Where(s => s.DoctorId == doctor.Id).OrderBy(s => s.AvailableDate).ThenBy(s => s.StartTime)
                .Select(s => new DoctorScheduleDto
                {
                    Id = s.Id,
                    AvailableDate = s.AvailableDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SlotMinutes = s.SlotMinutes,
                    IsAvailable = s.IsAvailable
                }).ToListAsync();

            return View(schedules);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var dto = new DoctorScheduleDto
            {
                AvailableDate =
                    DateOnly.FromDateTime(DateTime.Today),

                SlotMinutes = 30,

                IsAvailable = true
            };

            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> Create(DoctorScheduleDto dto)
        {
            var doctor = await GetCurrentDoctor();

            if (doctor == null)
                return NotFound();

            // Date validation
            if (dto.AvailableDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("AvailableDate", "Available date cannot be in the past.");
            }

            // Time validation
            if (dto.EndTime <= dto.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
            }

            // Slot validation
            if (dto.SlotMinutes <= 0)
            {
                ModelState.AddModelError("SlotMinutes", "Slot minutes must be greater than zero.");
            }

            if (!ModelState.IsValid)
                return View(dto);

            // Check duplicate schedule
            var exists = await db.DoctorSchedules
                .AnyAsync(s =>
                    s.DoctorId == doctor.Id &&
                    s.AvailableDate == dto.AvailableDate &&
                    s.StartTime == dto.StartTime &&
                    s.EndTime == dto.EndTime);

            if (exists)
            {
                ModelState.AddModelError("", "This schedule already exists.");
                return View(dto);
            }

            // Create Schedule
            var schedule = new DoctorSchedule
            {
                DoctorId = doctor.Id,
                AvailableDate = dto.AvailableDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                SlotMinutes = dto.SlotMinutes,
                IsAvailable = dto.IsAvailable
            };

            db.DoctorSchedules.Add(schedule);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await GetCurrentDoctor();

            if (doctor == null)
                return NotFound();


            // IMPORTANT:
            // Only the current doctor's schedule
            var schedule = await db.DoctorSchedules
                .FirstOrDefaultAsync(s =>  s.Id == id && s.DoctorId == doctor.Id);

            if (schedule == null)
                return NotFound();

            var dto = new DoctorScheduleDto
            {
                Id = schedule.Id,
                AvailableDate = schedule.AvailableDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                SlotMinutes = schedule.SlotMinutes,
                IsAvailable = schedule.IsAvailable
            };

            return View(dto);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, DoctorScheduleDto dto)
        {
            var doctor = await GetCurrentDoctor();

            if (doctor == null)
                return NotFound();


            // Get only current doctor's schedule
            var schedule = await db.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctor.Id);

            if (schedule == null)
                return NotFound();


            // Validation

            if (dto.AvailableDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("AvailableDate", "Available date cannot be in the past.");
            }

            if (dto.EndTime <= dto.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
            }

            if (dto.SlotMinutes <= 0)
            {
                ModelState.AddModelError("SlotMinutes", "Slot minutes must be greater than zero.");
            }

            if (!ModelState.IsValid)
                return View(dto);

            // Check duplicate
            var exists = await db.DoctorSchedules
                .AnyAsync(s =>
                    s.Id != id &&
                    s.DoctorId == doctor.Id &&
                    s.AvailableDate == dto.AvailableDate &&
                    s.StartTime == dto.StartTime &&
                    s.EndTime == dto.EndTime);

            if (exists)
            {
                ModelState.AddModelError( "","This schedule already exists.");
                return View(dto);
            }


            // Update
            schedule.AvailableDate = dto.AvailableDate;
            schedule.StartTime = dto.StartTime;
            schedule.EndTime = dto.EndTime;
            schedule.SlotMinutes = dto.SlotMinutes;
            schedule.IsAvailable = dto.IsAvailable;

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
                return NotFound();

            var schedule = await db.DoctorSchedules
                .AsNoTracking().Where(s => s.Id == id && s.DoctorId == doctor.Id)
                .Select(s => new DoctorScheduleDto
                {
                    Id = s.Id,
                    AvailableDate = s.AvailableDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SlotMinutes = s.SlotMinutes,
                    IsAvailable = s.IsAvailable
                }).FirstOrDefaultAsync();

            if (schedule == null)
                return NotFound();

            return View(schedule);
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
                return NotFound();

            var schedule = await db.DoctorSchedules.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctor.Id);

            if (schedule == null)
                return NotFound();

            db.DoctorSchedules.Remove(schedule);
            await db.SaveChangesAsync();

            TempData["Success"] = "Schedule deleted successfully.";
            TempData["SuccessType"] = "success";

            return RedirectToAction("Index");
        }
        private async Task<Doctor?> GetCurrentDoctor()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return null;

            var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId.Value);
            return doctor;
        }
    }
}
