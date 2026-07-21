using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Doctor;
using clinic.Services.Interfaces;
using clinic.Repositories.Interfaces;
using clinic.Models;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _service;
        private readonly IAuditLogRepository _audit;
        public DoctorsController(IDoctorService service, IAuditLogRepository audit)
        {
            _service = service;
            _audit = audit;
        }

        private async Task LogAsync(string action, int entityId, string? details = null)
        {
            await _audit.LogAsync(new AuditLog
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "",
                Action = action,
                Entity = "Doctor",
                EntityId = entityId,
                Details = details
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _service.GetAllAsync(search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _service.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });
            return Ok(doctor);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateAsync(dto);
            await LogAsync("Create", id, $"Created doctor '{dto.FullName}'");
            return Ok(new { message = "Doctor created successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Doctor not found" });
            return Ok(new { message = "Doctor updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Doctor not found" });
            await LogAsync("Delete", id);
            return Ok(new { message = "Doctor deleted successfully" });
        }
    }
}