using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Doctor;
using clinic.Services.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _service;
        public DoctorsController(IDoctorService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _service.GetAllAsync(search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _service.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });
            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateAsync(dto);
            return Ok(new { message = "Doctor created successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Doctor not found" });
            return Ok(new { message = "Doctor updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Doctor not found" });
            return Ok(new { message = "Doctor deleted successfully" });
        }
    }
}