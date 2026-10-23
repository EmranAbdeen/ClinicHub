using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicHub.DTOs.Specialties
{
    public class SpecialtyEditDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Specialty name is required.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // images currently are stored in the wwwroot/images/doctors folder, and the path is stored in the database
        public string? IconOrImagePath { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public bool IsActive { get; set; }
    }
}
