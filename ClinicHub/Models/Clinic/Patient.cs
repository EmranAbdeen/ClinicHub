using System;
using System.Collections.Generic;

namespace ClinicHub.Models.Clinic;

public partial class Patient
{
    public int Id { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? ImagePath { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User User { get; set; } = null!;
}
