using clinic.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.DoctorAvailability;
using clinic.Services.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorAvailabilityController : ControllerBase
    {
        private readonly IDoctorAvailabilityService _service;
        public DoctorAvailabilityController(IDoctorAvailabilityService service) => _service = service;

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
            => Ok(await _service.GetByDoctorIdAsync(doctorId));

        [RequirePermission("Doctors", "Edit")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorAvailabilityCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateAsync(dto);
            return Ok(new { message = "Availability added", id });
        }

        [RequirePermission("Doctors", "Edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorAvailabilityCreateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Not found" });
            return Ok(new { message = "Updated successfully" });
        }

        [RequirePermission("Doctors", "Edit")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Deleted successfully" });
        }

        // 🔑 Ye endpoint appointment booking form use karega
        [HttpGet("doctor/{doctorId}/slots")]
        public async Task<IActionResult> GetSlots(int doctorId, [FromQuery] DateTime date)
        {
            var slots = await _service.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }
    }
}