using ClinicHub.DTOs.Specialties;
using ClinicHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ClinicDbContext db;
        public SpecialtiesController(ClinicDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult GetAllSpecialties()
        {
            var specialties = db.Specialties.Where(s => s.IsActive)
                .Select(s => new SpecialtyListDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    IconOrImagePath = s.IconOrImagePath,
                    DoctorCount = s.Doctors.Count()
                }).ToList();

            return Ok(specialties);
        }
    }
}
