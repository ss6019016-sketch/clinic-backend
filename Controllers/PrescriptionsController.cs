using clinic.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Prescription;
using clinic.Repositories.Interfaces;
using clinic.Models;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionRepository _repo;
        private readonly IAuditLogRepository _audit;
        public PrescriptionsController(IPrescriptionRepository repo, IAuditLogRepository audit)
        {
            _repo = repo;
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
                Entity = "Prescription",
                EntityId = entityId,
                Details = details
            });
        }

        [RequirePermission("Prescriptions", "View")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return Ok(await _repo.GetAllAsync(search, page, pageSize));
        }

        [RequirePermission("Prescriptions", "View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rx = await _repo.GetByIdAsync(id);
            if (rx == null)
                return NotFound(new { message = "Prescription not found" });
            return Ok(rx);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
            => Ok(await _repo.GetByPatientAsync(patientId));

        [RequirePermission("Prescriptions", "Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrescriptionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _repo.CreateAsync(dto);
            await LogAsync("Create", id, $"Created prescription for PatientId {dto.PatientId} with diagnosis '{dto.Diagnosis}'");
            return Ok(new { message = "Prescription created successfully", id });
        }

        [RequirePermission("Prescriptions", "Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] PrescriptionUpdateDto dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.UpdateAsync(dto);
            if (!result) return NotFound();
            await LogAsync("Update", id, $"Updated prescription, diagnosis '{dto.Diagnosis}'");
            return Ok(new { message = "Prescription updated successfully" });
        }

        [RequirePermission("Prescriptions", "Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return NotFound();
            await LogAsync("Delete", id);
            return Ok(new { message = "Prescription deleted successfully" });
        }
    }
}