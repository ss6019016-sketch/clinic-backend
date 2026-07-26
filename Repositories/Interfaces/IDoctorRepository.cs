using clinic.DTOs.Doctor;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<PagedResult<Doctor>> GetAllAsync(string? search, int page, int pageSize);
        Task<Doctor?> GetByIdAsync(int id);
        Task<int> CreateAsync(DoctorCreateDto dto);
        Task<bool> UpdateAsync(DoctorUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        // Soft-delete / Trash support
        Task<IEnumerable<Doctor>> GetTrashAsync();
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}