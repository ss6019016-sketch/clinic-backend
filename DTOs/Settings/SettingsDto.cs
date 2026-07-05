using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Settings
{
    public class SettingsUpdateDto
    {
        [Required] public string ClinicName { get; set; } = string.Empty;
        [Required] public string Address { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string WorkingHours { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required] public string CurrentPassword { get; set; } = string.Empty;
        [Required, MinLength(6)] public string NewPassword { get; set; } = string.Empty;
    }
}