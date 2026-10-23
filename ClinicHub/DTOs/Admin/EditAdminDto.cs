using System.ComponentModel.DataAnnotations;

namespace ClinicHub.DTOs.Admin
{
    public class EditAdminDto
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        [RegularExpression("^07\\d{8}$", ErrorMessage = "Phone number must start with 07 and contain exactly 10 digits.")]
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }
}
