using System;
using System.Collections.Generic;

namespace ClinicHub.Models.Clinic;

public partial class DoctorSchedule
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public DateOnly AvailableDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SlotMinutes { get; set; }

    public bool IsAvailable { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;
}
