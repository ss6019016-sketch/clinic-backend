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
        public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Only jpg, png, webp allowed" });

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "Max 2MB allowed" });

            // Base64 mein convert karo
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            var base64 = Convert.ToBase64String(bytes);
            var mimeType = file.ContentType;
            var dataUrl = $"data:{mimeType};base64,{base64}";

            // DB mein save karo
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            using var db = _context.CreateConnection();
            await db.ExecuteAsync(
                "UPDATE Users SET ProfilePhoto=@Photo WHERE Id=@Id",
                new { Photo = dataUrl, Id = userId });

            return Ok(new
            {
                photoUrl = dataUrl,
                message = "Photo uploaded successfully!"
            });
        }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            using var db = _context.CreateConnection();
            var user = await db.QueryFirstOrDefaultAsync(
                "SELECT Id, FullName, Email, Phone, Role, ProfilePhoto FROM Users WHERE Id=@Id",
                new { Id = userId });
            return Ok(user);
        }
    }
}