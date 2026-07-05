using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Staff;
using clinic.Repositories.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffRepository _repo;
        public StaffController(IStaffRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _repo.GetAllAsync(search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var staff = await _repo.GetByIdAsync(id);
            if (staff == null)
                return NotFound(new { message = "Staff not found" });
            return Ok(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaffCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _repo.CreateAsync(dto);
            return Ok(new { message = "Staff added successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StaffUpdateDto dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.UpdateAsync(dto);
            if (!result) return NotFound();
            return Ok(new { message = "Staff updated successfully" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StaffStatusDto dto)
        {
            var result = await _repo.UpdateStatusAsync(id, dto.Status);
            if (!result) return NotFound();
            return Ok(new { message = $"Staff {dto.Status} successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Staff deleted successfully" });
        }
    }
}