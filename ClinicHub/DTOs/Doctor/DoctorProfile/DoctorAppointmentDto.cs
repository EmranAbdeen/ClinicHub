using ClinicHub.Models.Enums;

namespace ClinicHub.DTOs.Doctor.DoctorProfile
{
    public class DoctorAppointmentDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly AppointmentTime { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
