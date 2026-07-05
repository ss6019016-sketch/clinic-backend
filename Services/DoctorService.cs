using clinic.DTOs.Doctor;
using clinic.Models;
using clinic.Repositories.Interfaces;
using clinic.Services.Interfaces;

namespace clinic.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _repo;
        public DoctorService(IDoctorRepository repo) => _repo = repo;

        public Task<IEnumerable<Doctor>> GetAllAsync(string? search)
            => _repo.GetAllAsync(search);

        public Task<Doctor?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<int> CreateAsync(DoctorCreateDto dto)
            => _repo.CreateAsync(dto);

        public Task<bool> UpdateAsync(int id, DoctorCreateDto dto)
        {
            var updateDto = new DoctorUpdateDto
            {
                Id = id,
                FullName = dto.FullName,
                Specialization = dto.Specialization,
                Phone = dto.Phone,
                Email = dto.Email,
                Experience = dto.Experience,
                Fee = dto.Fee,
                AvailableDays = dto.AvailableDays,
                Qualification = dto.Qualification,
                LicenseNumber = dto.LicenseNumber,
                Bio = dto.Bio
            };
            return _repo.UpdateAsync(updateDto);
        }

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);
    }
}