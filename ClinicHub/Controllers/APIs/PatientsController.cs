using ClinicHub.DTOs.Patient;
using ClinicHub.Filters;
using ClinicHub.Models;
using ClinicHub.Models.Clinic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Controllers.APIs
{
    [AuthFilter(Roles = new[] { "Admin", "Super_admin" })]
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ClinicDbContext db;
        public PatientsController(ClinicDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult GetAllPatients()
        {
            var Patients = db.Patients.AsNoTracking()
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
                }).ToList();

            return Ok(Patients);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetPatient(int id)
        {
            var Patient = db.Patients.AsNoTracking().Where(p => p.Id == id)
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
                }).FirstOrDefault();
            
            if (Patient == null)
            {
                return NotFound(new
                {
                    message = "Patient not found."
                });
            }
            return Ok(Patient);
        }
    }
}
