using clinic.DTOs.Staff;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<Staff>> GetAllAsync(string? search);
        Task<Staff?> GetByIdAsync(int id);
        Task<int> CreateAsync(StaffCreateDto dto);
        Task<bool> UpdateAsync(StaffUpdateDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
    }
}