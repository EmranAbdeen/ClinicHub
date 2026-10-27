using ClinicHub.DTOs.Appointment;
using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers.APIs
{
    [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext db;
        public AppointmentsController(ClinicDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult GetAllAppointments()
        {
            var appointments = db.Appointments.AsNoTracking()
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
                }).ToList();

            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public IActionResult GetAppointment(int id)
        {
            var appointment = db.Appointments.AsNoTracking().Where(a => a.Id == id)
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
                }).FirstOrDefault();

            if (appointment == null)
            {
                return NotFound(new
                {
                    message = "Appointment not found."
                });
            }

            return Ok(appointment);
        }
    }
}
