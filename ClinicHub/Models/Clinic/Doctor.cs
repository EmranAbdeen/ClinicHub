using System;
using System.Collections.Generic;

namespace ClinicHub.Models.Clinic;

public partial class Doctor
{
    public int Id { get; set; }

    public string? Biography { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? ConsultationInfo { get; set; }

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; }

    public int UserId { get; set; }

    public int SpecialtyId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();

    public virtual Specialty Specialty { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
