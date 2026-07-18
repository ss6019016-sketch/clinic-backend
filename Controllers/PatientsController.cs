using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Patient;
using clinic.Services.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _service;
        public PatientsController(IPatientService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _service.GetAllAsync(search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _service.GetByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });
            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatientCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _service.CreateAsync(dto);
            return Ok(new { message = "Patient created successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound(new { message = "Patient not found" });
            return Ok(new { message = "Patient updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Patient not found" });
                return Ok(new { message = "Patient deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}