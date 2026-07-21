using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.Repositories.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogRepository _repo;
        public AuditLogController(IAuditLogRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? entity,
            [FromQuery] int? userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
            => Ok(await _repo.GetAllAsync(entity, userId, from, to));
    }
}