using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.Authorization;
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

        [RequirePermission("Doctors", "View")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return Ok(await _service.GetAllAsync(search, page, pageSize));
        }

        [RequirePermission("Doctors", "View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _service.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });
            return Ok(doctor);
        }

        [RequirePermission("Doctors", "Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateAsync(dto);
            await LogAsync("Create", id, $"Created doctor '{dto.FullName}'");
            return Ok(new { message = "Doctor created successfully", id });
        }

        [RequirePermission("Doctors", "Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Doctor not found" });
            await LogAsync("Update", id, $"Updated doctor '{dto.FullName}'");
            return Ok(new { message = "Doctor updated successfully" });
        }

        [RequirePermission("Doctors", "Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Doctor not found" });
            await LogAsync("Delete", id);
            return Ok(new { message = "Doctor deleted successfully" });
        }

        [RequirePermission("Doctors", "Delete")]
        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash()
            => Ok(await _service.GetTrashAsync());

        [RequirePermission("Doctors", "Delete")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _service.RestoreAsync(id);
            if (!result) return NotFound(new { message = "Doctor not found in trash" });
            await LogAsync("Restore", id);
            return Ok(new { message = "Doctor restored successfully" });
        }

        [RequirePermission("Doctors", "Delete")]
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var result = await _service.HardDeleteAsync(id);
            if (!result) return NotFound(new { message = "Doctor not found in trash" });
            await LogAsync("PermanentDelete", id);
            return Ok(new { message = "Doctor permanently deleted" });
        }
    }
}