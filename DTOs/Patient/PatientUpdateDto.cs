using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Patient
{
    public class PatientUpdateDto : PatientCreateDto
    {
        public int Id { get; set; }
    }
}