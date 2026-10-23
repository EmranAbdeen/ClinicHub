namespace ClinicHub.DTOs.Doctor
{
    public class DoctorListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public int? YearsOfExperience { get; set; }
        public string? Biography { get; set; }
        public string? ImagePath { get; set; }
        public int AppointmentCount { get; set; }
    }
}
