using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Appointment;
using clinic.Services.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;
        public AppointmentsController(IAppointmentService service)
            => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? search)
            => Ok(await _service.GetAllAsync(status, search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appt = await _service.GetByIdAsync(id);
            if (appt == null)
                return NotFound(new { message = "Appointment not found" });
            return Ok(appt);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateAsync(dto);
            return Ok(new { message = "Appointment booked successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Appointment not found" });
            return Ok(new { message = "Appointment updated successfully" });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            if (!result) return NotFound();
            return Ok(new { message = "Status updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Appointment deleted successfully" });
        }
    }
}