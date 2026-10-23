using System.ComponentModel.DataAnnotations;

namespace ClinicHub.DTOs.Account
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        /*
          (?=...) : Checks that something exists without consuming it.
          .   : Matches any character
       [^A-Za-z0-9] : Matches any character that is NOT a letter (A-Z, a-z) or a number (0-9). Used to require a 'special character'.
       */
        //[RegularExpression(
        //"^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{6,10}$",
        //ErrorMessage = "Password must be 6-10 characters and contain an uppercase letter, lowercase letter, number, and special character."
        //)]
        public string Password { get; set; } = string.Empty;
    }
}
