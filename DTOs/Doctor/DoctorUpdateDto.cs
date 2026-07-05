using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Doctor
{
    public class DoctorUpdateDto : DoctorCreateDto
    {
        public int Id { get; set; }
    }
}