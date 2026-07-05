namespace clinic.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string Disease { get; set; } = string.Empty;
        public string MedicalHistory { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }
}