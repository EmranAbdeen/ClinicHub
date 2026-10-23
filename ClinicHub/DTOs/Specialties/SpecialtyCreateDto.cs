using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicHub.DTOs.Specialties
{
    public class SpecialtyCreateDto
    {
        [Required(ErrorMessage = "Specialty name is required.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
       
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
