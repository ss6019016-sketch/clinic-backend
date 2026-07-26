using clinic.DTOs.Staff;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<PagedResult<Staff>> GetAllAsync(string? search, int page, int pageSize);
        Task<Staff?> GetByIdAsync(int id);
        Task<int> CreateAsync(StaffCreateDto dto);
        Task<bool> UpdateAsync(StaffUpdateDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);

        // Soft-delete / Trash support
        Task<IEnumerable<Staff>> GetTrashAsync();
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}