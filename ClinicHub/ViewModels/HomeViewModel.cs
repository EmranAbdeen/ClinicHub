using System.Collections.Generic;
using ClinicHub.Models.Clinic;
using ClinicHub.Models.Content;

namespace ClinicHub.ViewModels
{
    public class HomeViewModel
    {
        public WebsiteSetting? Settings { get; set; }
        public List<Service> Services { get; set; } = new();
        public List<Specialty> Specialties { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();

        public int DoctorsCount { get; set; }
        public int SpecialtiesCount { get; set; }
        public int ServicesCount { get; set; }
        //public int AppointmentsCount { get; set; }
    }
}
