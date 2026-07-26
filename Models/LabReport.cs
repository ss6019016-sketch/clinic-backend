namespace clinic.Models
{
    public class LabReport
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty; // joined
        public int? DoctorId { get; set; }
        public string? DoctorName { get; set; } // joined
        public string TestName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileData { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}