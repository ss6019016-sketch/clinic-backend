using clinic.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Billing;
using clinic.Repositories.Interfaces;
using clinic.Models;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingRepository _repo;
        private readonly IAuditLogRepository _audit;
        public BillingController(IBillingRepository repo, IAuditLogRepository audit)
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
                Entity = "Invoice",
                EntityId = entityId,
                Details = details
            });
        }

        [RequirePermission("Billing", "View")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return Ok(await _repo.GetAllAsync(status, search, page, pageSize));
        }

        [RequirePermission("Billing", "View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _repo.GetByIdAsync(id);
            if (invoice == null)
                return NotFound(new { message = "Invoice not found" });
            return Ok(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _repo.CreateAsync(dto);
            await LogAsync("Create", id, $"Invoice created for patient #{dto.PatientId}");
            return Ok(new { message = "Invoice created successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceUpdateDto dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.UpdateAsync(dto);
            if (!result) return NotFound(new { message = "Invoice not found" });
            await LogAsync("Update", id, $"Invoice updated for patient #{dto.PatientId}");
            return Ok(new { message = "Invoice updated successfully" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] InvoiceStatusDto dto)
        {
            var result = await _repo.UpdateStatusAsync(id, dto);
            if (!result) return NotFound();
            await LogAsync("StatusChange", id, $"Payment status changed to '{dto.Status}'");
            return Ok(new { message = "Payment status updated" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return NotFound();

            await LogAsync("Delete", id);

            return Ok(new { message = "Invoice deleted successfully" });
        }
    }
}