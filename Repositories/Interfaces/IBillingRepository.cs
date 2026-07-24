using clinic.DTOs.Billing;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IBillingRepository
    {
        Task<PagedResult<Invoice>> GetAllAsync(string? status, string? search, int page, int pageSize);
        Task<Invoice?> GetByIdAsync(int id);
        Task<int> CreateAsync(InvoiceCreateDto dto);
        Task<bool> UpdateAsync(InvoiceUpdateDto dto);
        Task<bool> UpdateStatusAsync(int id, InvoiceStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}