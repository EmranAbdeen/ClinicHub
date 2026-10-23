namespace ClinicHub.DTOs.Doctor
{
    //  Api --> Details 
    public class DoctorDetailsDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public int? YearsOfExperience { get; set; }
        public string? Biography { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int AppointmentCount { get; set; }
    }
}
