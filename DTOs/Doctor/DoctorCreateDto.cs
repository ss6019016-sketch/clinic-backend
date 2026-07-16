using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Doctor
{
    public class DoctorCreateDto
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Specialization { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
        public int Experience { get; set; }
        public decimal Fee { get; set; }
        public string AvailableDays { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ProfilePhoto { get; set; } = string.Empty;
    }


}