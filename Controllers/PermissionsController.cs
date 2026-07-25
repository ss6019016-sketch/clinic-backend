using clinic.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.Repositories.Interfaces;
using clinic.Services.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _repo;
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionRepository repo, IPermissionService permissionService)
        {
            _repo = repo;
            _permissionService = permissionService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
            => Ok(await _repo.GetAllAsync());

        [HttpGet("my")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(role))
                return Unauthorized();

            var permissions = await _permissionService.GetForRoleAsync(role);
            return Ok(permissions);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PermissionUpdateRequest request)
        {
            var updated = await _repo.UpdateAsync(id, request.CanView, request.CanCreate, request.CanEdit, request.CanDelete);
            return updated ? Ok(new { message = "Permission updated successfully" }) : NotFound(new { message = "Permission not found" });
        }
    }

    public class PermissionUpdateRequest
    {
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
