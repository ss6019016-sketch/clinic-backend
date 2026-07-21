namespace clinic.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string Reason { get; set; } = string.Empty;
        public string Type { get; set; } = "New";
        public string Notes { get; set; } = string.Empty;
        public bool ReminderSent { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}