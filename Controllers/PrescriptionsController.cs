using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using clinic.DTOs.Prescription;
using clinic.Repositories.Interfaces;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionRepository _repo;
        public PrescriptionsController(IPrescriptionRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _repo.GetAllAsync(search));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rx = await _repo.GetByIdAsync(id);
            if (rx == null)
                return NotFound(new { message = "Prescription not found" });
            return Ok(rx);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
            => Ok(await _repo.GetByPatientAsync(patientId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrescriptionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = await _repo.CreateAsync(dto);
            return Ok(new { message = "Prescription created successfully", id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] PrescriptionUpdateDto dto)
        {
            dto.Id = id;
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _repo.UpdateAsync(dto);
            if (!result) return NotFound();
            return Ok(new { message = "Prescription updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Prescription deleted successfully" });
        }
    }
}