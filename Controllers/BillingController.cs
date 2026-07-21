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

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? search)
            => Ok(await _repo.GetAllAsync(status, search));

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
            return Ok(new { message = "Invoice created successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceUpdateDto dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.UpdateAsync(dto);
            if (!result) return NotFound(new { message = "Invoice not found" });
            return Ok(new { message = "Invoice updated successfully" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] InvoiceStatusDto dto)
        {
            var result = await _repo.UpdateStatusAsync(id, dto);
            if (!result) return NotFound();
            return Ok(new { message = "Payment status updated" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return NotFound();

            await _audit.LogAsync(new AuditLog
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "",
                Action = "Delete",
                Entity = "Invoice",
                EntityId = id
            });

            return Ok(new { message = "Invoice deleted successfully" });
        }
    }
}