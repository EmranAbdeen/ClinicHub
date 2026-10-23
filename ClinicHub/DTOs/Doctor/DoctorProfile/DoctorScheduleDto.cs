namespace ClinicHub.DTOs.Doctor.DoctorProfile
{
    public class DoctorScheduleDto
    {
        public int Id { get; set; }
        public DateOnly AvailableDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SlotMinutes { get; set; } = 30;
        public bool IsAvailable { get; set; } = true;
    }
}
