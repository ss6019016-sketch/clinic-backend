using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Auth;
using clinic.Services.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
            => _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _authService.RegisterAsync(dto);
            if (!success)
                return BadRequest(new { message = "Email already registered" });
            return Ok(new { message = "Registered successfully" });
        }
    }
}