namespace clinic.Models
{
    public class DoctorAvailability
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty; // joined field for display
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}