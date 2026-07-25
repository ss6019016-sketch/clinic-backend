using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.Authorization;
using clinic.DTOs.Patient;
using clinic.Services.Interfaces;
using clinic.Repositories.Interfaces;
using clinic.Models;
using System.Security.Claims;

namespace clinic.Controllers
{
     [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _service;
        private readonly IAuditLogRepository _audit;
        public PatientsController(IPatientService service, IAuditLogRepository audit)
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
                Entity = "Patient",
                EntityId = entityId,
                Details = details
            });
        }
 
        [RequirePermission("Patients", "View")]
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
 
        [RequirePermission("Patients", "View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _service.GetByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });
            return Ok(patient);
        }
 
        [RequirePermission("Patients", "Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatientCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateAsync(dto);
            await LogAsync("Create", id);
            return Ok(new { message = "Patient created successfully", id });
        }
 
        [RequirePermission("Patients", "Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Patient not found" });
            await LogAsync("Update", id);
            return Ok(new { message = "Patient updated successfully" });
        }
 
        [RequirePermission("Patients", "Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Patient not found" });
 
                await _audit.LogAsync(new AuditLog
                {
                    UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                    UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "",
                    Action = "Delete",
                    Entity = "Patient",
                    EntityId = id
                });
 
                return Ok(new { message = "Patient deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}