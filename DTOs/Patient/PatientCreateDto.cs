using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Patient
{
    public class PatientCreateDto
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Gender { get; set; } = string.Empty;
        [Required] public int Age { get; set; }
        [Required] public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string Disease { get; set; } = string.Empty;
        public string MedicalHistory { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
    }
}