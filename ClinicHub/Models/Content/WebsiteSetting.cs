using System;
using System.Collections.Generic;

namespace ClinicHub.Models.Content;

public partial class WebsiteSetting
{
    public int Id { get; set; }

    public string ClinicName { get; set; } = null!;

    public string? HeroTitle { get; set; }

    public string? HeroText { get; set; }

    public string? AboutText { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? WorkingHours { get; set; }
}
