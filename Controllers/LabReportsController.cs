using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.Authorization;
using clinic.DTOs.LabReport;
using clinic.Repositories.Interfaces;
using clinic.Models;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LabReportsController : ControllerBase
    {
        private readonly ILabReportRepository _repo;
        private readonly IAuditLogRepository _audit;

        // Keep in sync with UploadController's own limits/allowed types
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public LabReportsController(ILabReportRepository repo, IAuditLogRepository audit)
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
                Entity = "LabReport",
                EntityId = entityId,
                Details = details
            });
        }

        [RequirePermission("LabReports", "View")]
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var data = await _repo.GetByPatientIdAsync(patientId);
            return Ok(data);
        }

        [RequirePermission("LabReports", "View")]
        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetFile(int id)
        {
            var report = await _repo.GetByIdAsync(id);
            if (report == null) return NotFound(new { message = "Lab report not found" });

            // FileData is a data URL ("data:<mime>;base64,<bytes>") — strip the
            // prefix and return raw bytes so the browser can preview/download it.
            var parts = report.FileData.Split(",", 2);
            var bytes = Convert.FromBase64String(parts.Length > 1 ? parts[1] : parts[0]);
            return File(bytes, report.FileType, report.FileName);
        }

        [RequirePermission("LabReports", "Create")]
        [HttpPost]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> Upload([FromForm] LabReportCreateDto dto, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest(new { message = "Only JPG, PNG, WEBP, or PDF files are allowed" });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { message = "Max file size is 5MB" });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            var dataUrl = $"data:{file.ContentType};base64,{base64}";

            var id = await _repo.CreateAsync(dto, file.FileName, file.ContentType, dataUrl);
            await LogAsync("Create", id, $"Uploaded '{dto.TestName}' report for PatientId {dto.PatientId}");
            return Ok(new { id, message = "Lab report uploaded successfully" });
        }

        [RequirePermission("LabReports", "Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Lab report not found" });
            await LogAsync("Delete", id);
            return Ok(new { message = "Lab report deleted successfully" });
        }
    }
}