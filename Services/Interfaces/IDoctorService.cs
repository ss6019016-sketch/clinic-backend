using clinic.DTOs.Doctor;
using clinic.Models;

namespace clinic.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<Doctor>> GetAllAsync(string? search);
        Task<Doctor?> GetByIdAsync(int id);
        Task<int> CreateAsync(DoctorCreateDto dto);
        Task<bool> UpdateAsync(int id, DoctorCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}