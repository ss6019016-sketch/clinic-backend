using clinic.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Appointment;
using clinic.Services.Interfaces;
using clinic.Repositories.Interfaces;
using clinic.Models;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;
        private readonly IAuditLogRepository _audit;
        public AppointmentsController(IAppointmentService service, IAuditLogRepository audit)
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
                Entity = "Appointment",
                EntityId = entityId,
                Details = details
            });
        }

        [RequirePermission("Appointments", "View")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return Ok(await _service.GetAllAsync(status, search, page, pageSize));
        }

        [RequirePermission("Appointments", "View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appt = await _service.GetByIdAsync(id);
            if (appt == null)
                return NotFound(new { message = "Appointment not found" });
            return Ok(appt);
        }

        [RequirePermission("Appointments", "Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var id = await _service.CreateAsync(dto);
                await LogAsync("Create", id, $"Booked appointment for PatientId {dto.PatientId} with DoctorId {dto.DoctorId} on {dto.AppointmentDate:yyyy-MM-dd} at {dto.AppointmentTime}");
                return Ok(new { message = "Appointment booked successfully", id });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [RequirePermission("Appointments", "Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Appointment not found" });
            await LogAsync("Update", id, $"Updated appointment on {dto.AppointmentDate:yyyy-MM-dd} at {dto.AppointmentTime}");
            return Ok(new { message = "Appointment updated successfully" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            if (!result) return NotFound();
            await LogAsync("StatusChange", id, $"Status changed to '{status}'");
            return Ok(new { message = "Status updated successfully" });
        }

        [RequirePermission("Appointments", "Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound();
            await LogAsync("Delete", id);
            return Ok(new { message = "Appointment deleted successfully" });
        }

        [HttpPost("{id}/send-reminder")]
        public async Task<IActionResult> SendReminder(int id)
        {
            var (success, message) = await _service.SendReminderNowAsync(id);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

    }
}