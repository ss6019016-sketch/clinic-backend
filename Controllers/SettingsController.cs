using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using clinic.Data;
using clinic.DTOs.Settings;
using clinic.Helpers;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly DapperContext _context;
        public SettingsController(DapperContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var db = _context.CreateConnection();
            var settings = await db.QueryFirstOrDefaultAsync(
                "SELECT * FROM ClinicSettings WHERE Id=1");
            return Ok(settings);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] SettingsUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            using var db = _context.CreateConnection();
            await db.ExecuteAsync(@"
                UPDATE ClinicSettings SET
                    ClinicName=@ClinicName, Address=@Address,
                    Phone=@Phone, Email=@Email,
                    WorkingHours=@WorkingHours,
                    Website=@Website, Description=@Description
                WHERE Id=1", dto);

            return Ok(new { message = "Settings updated successfully" });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            using var db = _context.CreateConnection();
            var user = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM Users WHERE Id=@Id", new { Id = userId });

            if (user == null) return NotFound();

            if (!PasswordHelper.Verify(dto.CurrentPassword, (string)user.Password))
                return BadRequest(new { message = "Current password is incorrect" });

            await db.ExecuteAsync(
                "UPDATE Users SET Password=@Password WHERE Id=@Id",
                new
                {
                    Password = PasswordHelper.Hash(dto.NewPassword),
                    Id = userId
                });

            return Ok(new { message = "Password changed successfully" });
        }
    }
}