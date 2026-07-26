namespace clinic.DTOs.DoctorAvailability
{
    public class DoctorAvailabilityCreateDto
    {
        public int DoctorId { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public bool IsActive { get; set; } = true;
    }

    public class AvailableSlotDto
    {
        public string Time { get; set; } = string.Empty; // "09:00 AM"
        public bool IsBooked { get; set; }
    }
}