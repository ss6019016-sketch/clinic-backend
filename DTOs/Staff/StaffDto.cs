using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Staff
{
    public class StaffCreateDto
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class StaffUpdateDto
    {
        public int Id { get; set; }
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class StaffStatusDto
    {
        [Required] public string Status { get; set; } = string.Empty;
    }
}