using clinic.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Vendor;
using clinic.Repositories.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorRepository _repo;
        public VendorsController(IVendorRepository repo) => _repo = repo;

        [RequirePermission("Pharmacy", "View")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _repo.GetAllAsync(search));

        [RequirePermission("Pharmacy", "View")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vendor = await _repo.GetByIdAsync(id);
            return vendor == null ? NotFound() : Ok(vendor);
        }

        [RequirePermission("Pharmacy", "Create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VendorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _repo.CreateAsync(dto);
            return Ok(new { message = "Vendor created successfully", id });
        }

        [RequirePermission("Pharmacy", "Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VendorUpdateDto dto)
        {
            dto.Id = id;
            if (!await _repo.UpdateAsync(dto)) return NotFound();
            return Ok(new { message = "Vendor updated successfully" });
        }

        [RequirePermission("Pharmacy", "Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repo.DeleteAsync(id)) return NotFound();
            return Ok(new { message = "Vendor deleted successfully" });
        }
    }
}