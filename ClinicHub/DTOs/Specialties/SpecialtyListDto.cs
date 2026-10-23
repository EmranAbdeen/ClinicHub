namespace ClinicHub.DTOs.Specialties
{
    public class SpecialtyListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconOrImagePath { get; set; }
        public bool IsActive { get; set; }
        public int DoctorCount { get; set; }
    }
}
