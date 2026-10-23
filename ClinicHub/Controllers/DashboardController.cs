namespace ClinicHub.Controllers
{
    using ClinicHub.Filters;
    using ClinicHub.Models;
    using ClinicHub.Models.Enums;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
    public class DashboardController : Controller
    {
        private readonly ClinicDbContext _db;
        public DashboardController(ClinicDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            // =========================
            // Dashboard Cards
            // =========================

            ViewBag.DoctorsCount = await _db.Doctors.CountAsync();
            ViewBag.PatientsCount = await _db.Patients.CountAsync();
            ViewBag.AppointmentsCount = await _db.Appointments.CountAsync();
            ViewBag.SpecialtiesCount = await _db.Specialties.CountAsync(s => s.IsActive);
            ViewBag.AdminsCount = await _db.Users.CountAsync(u => u.Role.Name == "Admin");


            // =========================
            // Appointments by Status
            // =========================

            var statusGroups = await _db.Appointments.GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToListAsync();

            var allStatuses = Enum.GetValues<AppointmentStatus>();

            ViewBag.StatusLabels = allStatuses.Select(s => s.ToString()).ToArray();

            ViewBag.StatusValues = allStatuses
                .Select(s => statusGroups.FirstOrDefault(g => g.Status == (byte)s)?.Count ?? 0).ToArray();


            // =========================
            // Appointments by Month
            // Last 12 Months
            // =========================

            var today = DateTime.Today;

            var firstMonth = new DateOnly(
                today.AddMonths(-11).Year,
                today.AddMonths(-11).Month,1
                );

            var lastMonth = new DateOnly(today.Year, today.Month,1);

            var monthlyGroups = await _db.Appointments
                .Where(a => a.AppointmentDate >= firstMonth 
                            && a.AppointmentDate <= 
                                new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)))
                .GroupBy(a => new
                {
                    a.AppointmentDate.Year,
                    a.AppointmentDate.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                }).ToListAsync();

            var monthLabels = new List<string>();
            var monthValues = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                var month = firstMonth.AddMonths(i);
                monthLabels.Add(month.ToString("yyyy-MM"));
                var count = monthlyGroups.FirstOrDefault(m =>
                        m.Year == month.Year && m.Month == month.Month)?.Count ?? 0;

                monthValues.Add(count);
            }

            ViewBag.MonthLabels = monthLabels;
            ViewBag.MonthValues = monthValues;


            // =========================
            // Recent Appointments
            // =========================

            var recentAppointments = await _db.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Take(10)
                .ToListAsync();

            return View(recentAppointments);
        }
    }
}
