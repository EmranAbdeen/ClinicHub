using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicHub.DTOs.Doctor.DoctorProfile
{
    public class DoctorProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? ImagePath { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public List<DoctorScheduleDto> Schedules { get; set; } = new();
        public List<DoctorAppointmentDto> Appointments { get; set; } = new();
    }
}
