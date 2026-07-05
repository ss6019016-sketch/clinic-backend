using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Prescription
{
    public class PrescriptionCreateDto
    {
        [Required] public int PatientId { get; set; }
        [Required] public int DoctorId { get; set; }
        public int? AppointmentId { get; set; }
        [Required] public string Diagnosis { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string? FollowUpDate { get; set; }

        [Required] public List<PrescriptionItemDto> Medicines { get; set; } = new();
    }

    public class PrescriptionItemDto
    {
        [Required] public string MedicineName { get; set; } = string.Empty;
        [Required] public string Dosage { get; set; } = string.Empty;
        [Required] public string Frequency { get; set; } = string.Empty;
        [Required] public string Duration { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
    }

    public class PrescriptionUpdateDto : PrescriptionCreateDto
    {
        public int Id { get; set; }
    }
}