namespace clinic.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Experience { get; set; }
        public decimal Fee { get; set; }
        public string AvailableDays { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string ProfilePhoto { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }
}