namespace ClinicHub.Models.Enums
{
    public enum AppointmentStatus : byte
    {
        Pending = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3,
        Rejected = 4
    }
}
