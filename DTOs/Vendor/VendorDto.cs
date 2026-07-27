using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Vendor
{
    public class VendorCreateDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class VendorUpdateDto : VendorCreateDto
    {
        public int Id { get; set; }
    }
}