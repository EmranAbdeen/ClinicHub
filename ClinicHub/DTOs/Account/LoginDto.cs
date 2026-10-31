using System.ComponentModel.DataAnnotations;

namespace ClinicHub.DTOs.Account
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
        "^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{6,10}$",
        ErrorMessage = "Password must be 6-10 characters and contain an uppercase letter, lowercase letter, number, and special character."
        )]
        public string Password { get; set; } = string.Empty;
    }
}
