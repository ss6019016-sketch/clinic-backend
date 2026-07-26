namespace clinic.DTOs.LabReport
{
    // Used for the list view — deliberately excludes FileData (the base64
    // blob) so fetching a patient's report list stays fast and lightweight.
    public class LabReportListDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string TestName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

   
}