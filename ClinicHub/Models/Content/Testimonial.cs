using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicHub.Models.Content;

public partial class Testimonial
{
    public int Id { get; set; }

    public string PatientName { get; set; } = null!;

    public string Message { get; set; } = null!;
    
    [Range(1, 5, ErrorMessage = "Please select a rating.")]
    public byte Rating { get; set; }

    public string? ImagePath { get; set; }

    public bool IsVisible { get; set; } = false;

    public DateTime CreatedAt { get; set; }
}
