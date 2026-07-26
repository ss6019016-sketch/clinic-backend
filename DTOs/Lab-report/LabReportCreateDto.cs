namespace clinic.DTOs.LabReport
{
   

    public class LabReportCreateDto
    {
        public int PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public string? Notes { get; set; }
    }
}