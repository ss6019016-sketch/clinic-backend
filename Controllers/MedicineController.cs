using clinic.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Medicine;
using clinic.Repositories.Interfaces;
using clinic.Models;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineRepository _repo;
        private readonly IAuditLogRepository _audit;
        public MedicinesController(IMedicineRepository repo, IAuditLogRepository audit)
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
                Entity = "Medicine",
                EntityId = entityId,
                Details = details
            });
        }

        [RequirePermission("Pharmacy", "View")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            return Ok(await _repo.GetAllAsync(search, page, pageSize));
        }

        [RequirePermission("Pharmacy", "View")]
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock() => Ok(await _repo.GetLowStockAsync());

        [RequirePermission("Pharmacy", "View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var med = await _repo.GetByIdAsync(id);
            return med == null ? NotFound() : Ok(med);
        }

        [RequirePermission("Pharmacy", "View")]
        [HttpGet("{id}/stock-logs")]
        public async Task<IActionResult> GetStockLogs(int id) => Ok(await _repo.GetStockLogsAsync(id));

        [RequirePermission("Pharmacy", "Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MedicineCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _repo.CreateAsync(dto);
            await LogAsync("Create", id, $"Added medicine '{dto.Name}' with stock {dto.StockQuantity}");
            return Ok(new { message = "Medicine added successfully", id });
        }

        [RequirePermission("Pharmacy", "Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MedicineUpdateDto dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await _repo.UpdateAsync(dto)) return NotFound();
            await LogAsync("Update", id, $"Updated medicine '{dto.Name}'");
            return Ok(new { message = "Medicine updated successfully" });
        }

        [RequirePermission("Pharmacy", "Edit")]
        [HttpPost("{id}/adjust-stock")]
        public async Task<IActionResult> AdjustStock(int id, [FromBody] StockAdjustDto dto)
        {
            var result = await _repo.AdjustStockAsync(id, dto.Quantity, "Adjustment", null, dto.Notes);
            if (!result) return BadRequest(new { message = "Invalid stock adjustment (stock cannot go negative)" });
            await LogAsync("StockAdjust", id, $"Stock changed by {dto.Quantity}. {dto.Notes}");
            return Ok(new { message = "Stock adjusted successfully" });
        }

        [RequirePermission("Pharmacy", "Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id)) return NotFound();
            await LogAsync("Delete", id);
            return Ok(new { message = "Medicine deleted successfully" });
        }

        [RequirePermission("Pharmacy", "Delete")]
        [HttpGet("trash")]
        public async Task<IActionResult> GetTrash() => Ok(await _repo.GetTrashAsync());

        [RequirePermission("Pharmacy", "Delete")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            if (!await _repo.RestoreAsync(id)) return NotFound();
            await LogAsync("Restore", id);
            return Ok(new { message = "Medicine restored successfully" });
        }

        [RequirePermission("Pharmacy", "Delete")]
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/permanent")]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            if (!await _repo.HardDeleteAsync(id)) return NotFound();
            await LogAsync("PermanentDelete", id);
            return Ok(new { message = "Medicine permanently deleted" });
        }
    }
}