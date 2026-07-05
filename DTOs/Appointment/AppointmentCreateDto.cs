using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Appointment
{
    public class AppointmentCreateDto
    {
        [Required] public int PatientId { get; set; }
        [Required] public int DoctorId { get; set; }
        [Required] public DateTime AppointmentDate { get; set; }
        [Required] public string AppointmentTime { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string Reason { get; set; } = string.Empty;
        public string Type { get; set; } = "New";
        public string Notes { get; set; } = string.Empty;
    }

    public class AppointmentUpdateDto : AppointmentCreateDto
    {
        public int Id { get; set; }
    }
}