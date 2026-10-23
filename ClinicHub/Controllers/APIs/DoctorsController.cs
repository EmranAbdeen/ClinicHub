using ClinicHub.DTOs.Doctor;
using ClinicHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ClinicDbContext db;
        public DoctorsController(ClinicDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult GetAllDoctors()
        {
            var doctors = db.Doctors.AsNoTracking().Where(d => d.IsActive)
                .Select(d => new DoctorListDto
                {
                    Id = d.Id,
                    FullName = d.User.FullName,
                    SpecialtyName = d.Specialty.Name,
                    YearsOfExperience = d.YearsOfExperience,
                    Biography = d.Biography,
                    ImagePath = d.ImagePath,
                    AppointmentCount = d.Appointments.Count()
                }).ToList();
            return Ok(doctors);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetDoctor(int id)
        {
            var doctors = db.Doctors.AsNoTracking().Where(d => d.Id == id && d.User.IsActive)
                .Select(d => new DoctorListDto
                {
                    Id = d.Id,
                    FullName = d.User.FullName,
                    SpecialtyName = d.Specialty.Name,
                    YearsOfExperience = d.YearsOfExperience,
                    Biography = d.Biography,
                    ImagePath = d.ImagePath,
                    AppointmentCount = d.Appointments.Count()
                }).FirstOrDefault();
            
            if(doctors == null)
            {
                return NotFound(new
                {
                    message = "Doctor not found."
                });
            }

            return Ok(doctors);
        }
    }
}
