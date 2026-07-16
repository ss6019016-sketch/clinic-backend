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
        private readonly IWebHostEnvironment _env;

        public UploadController(DapperContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

            // ✅ Railway pe contentRoot use karo, wwwroot nahi
            var uploadsFolder = Path.Combine(
                _env.ContentRootPath, "uploads"
            );
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{fileName}";

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            using var db = _context.CreateConnection();
            await db.ExecuteAsync(
                "UPDATE Users SET ProfilePhoto=@Photo WHERE Id=@Id",
                new { Photo = fileUrl, Id = userId });

            return Ok(new { photoUrl = fileUrl, message = "Photo uploaded!" });
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