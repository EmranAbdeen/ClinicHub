using ClinicHub.Models.Clinic;
using ClinicHub.Models.Content;
using ClinicHub.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ClinicHub.DTOs.Patient
{
    public class PatientProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^07\\d{8}$", ErrorMessage = "Phone number must start with 07 and contain exactly 10 digits.")]
        public string Phone { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public IFormFile? ImageFile { get; set; }

        public List<PatientAppointmentDto> Appointments { get; set; } = new();
        public List<ContactMessage> ContactMessages { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();

        public class PatientAppointmentDto
        {
            public int Id { get; set; }
            public string DoctorName { get; set; } = string.Empty;
            public string? SpecialtyName { get; set; }
            public DateOnly AppointmentDate { get; set; }
            public TimeOnly AppointmentTime { get; set; }
            public string? Notes { get; set; }
            public AppointmentStatus Status { get; set; }
        }
    }
}
