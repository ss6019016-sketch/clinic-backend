using clinic.DTOs.Auth;

namespace clinic.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<bool> RegisterAsync(RegisterDto dto);
    }
}