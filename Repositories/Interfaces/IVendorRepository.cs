using clinic.DTOs.Vendor;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IVendorRepository
    {
        Task<IEnumerable<Vendor>> GetAllAsync(string? search);
        Task<Vendor?> GetByIdAsync(int id);
        Task<int> CreateAsync(VendorCreateDto dto);
        Task<bool> UpdateAsync(VendorUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}