using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using clinic.Data;
using System.Security.Claims;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly DapperContext _context;

        public UploadController(DapperContext context)
        {
            _context = context;
        }

        [HttpPost("profile-photo")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
        public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowed.Contains(ext))
                    return BadRequest(new { message = "Only jpg, png, webp allowed" });

                if (file.Length > 2 * 1024 * 1024)
                    return BadRequest(new { message = "Max 2MB allowed" });

                // Base64
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                var mimeType = file.ContentType;
                var dataUrl = $"data:{mimeType};base64,{base64}";

                // DB save
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not found" });

                using var db = _context.CreateConnection();
                var affected = await db.ExecuteAsync(
                    "UPDATE Users SET ProfilePhoto=@Photo WHERE Id=@Id",
                    new { Photo = dataUrl, Id = int.Parse(userId) });

                if (affected == 0)
                    return NotFound(new { message = "User not found in DB" });

                return Ok(new { photoUrl = dataUrl, message = "Photo uploaded!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}