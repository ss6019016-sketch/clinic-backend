using clinic.DTOs.Medicine;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IMedicineRepository
    {
        Task<PagedResult<Medicine>> GetAllAsync(string? search, int page, int pageSize);
        Task<Medicine?> GetByIdAsync(int id);
        Task<int> CreateAsync(MedicineCreateDto dto);
        Task<bool> UpdateAsync(MedicineUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Medicine>> GetLowStockAsync();
        Task<int> GetLowStockCountAsync();
        Task<bool> AdjustStockAsync(int medicineId, int quantityChange, string changeType, int? referenceId, string? notes);
        Task<IEnumerable<MedicineStockLog>> GetStockLogsAsync(int medicineId);

        // Trash
        Task<IEnumerable<Medicine>> GetTrashAsync();
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}