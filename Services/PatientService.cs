using clinic.DTOs.Patient;
using clinic.Models;
using clinic.Repositories.Interfaces;
using clinic.Services.Interfaces;

namespace clinic.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repo;
        public PatientService(IPatientRepository repo) => _repo = repo;

        public Task<IEnumerable<Patient>> GetAllAsync(string? search)
            => _repo.GetAllAsync(search);

        public Task<Patient?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<int> CreateAsync(PatientCreateDto dto)
            => _repo.CreateAsync(dto);

        public Task<bool> UpdateAsync(int id, PatientCreateDto dto)
        {
            var updateDto = new PatientUpdateDto
            {
                Id = id,
                FullName = dto.FullName,
                Gender = dto.Gender,
                Age = dto.Age,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                BloodGroup = dto.BloodGroup,
                Disease = dto.Disease,
                MedicalHistory = dto.MedicalHistory,
                EmergencyContact = dto.EmergencyContact
            };
            return _repo.UpdateAsync(updateDto);
        }

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}