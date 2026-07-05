namespace clinic.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public int? AppointmentId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PrescriptionItem> Medicines { get; set; } = new();
    }

   
}