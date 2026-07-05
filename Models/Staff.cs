namespace clinic.Models
{
    public class Staff
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Receptionist";
        public string Password { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; }
    }
}