using System.ComponentModel.DataAnnotations;
namespace clinic.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Role is intentionally NOT settable from the client anymore.
        // Public self-registration always creates a "Receptionist" account.
        // Admin/Doctor accounts must be created by an existing Admin via
        // the protected /api/staff endpoint.
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}