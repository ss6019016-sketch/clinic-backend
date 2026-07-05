using clinic.DTOs.Doctor;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync(string? search);
        Task<Doctor?> GetByIdAsync(int id);
        Task<int> CreateAsync(DoctorCreateDto dto);
        Task<bool> UpdateAsync(DoctorUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}