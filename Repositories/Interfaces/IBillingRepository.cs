using clinic.DTOs.Billing;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IBillingRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync(string? status, string? search);
        Task<Invoice?> GetByIdAsync(int id);
        Task<int> CreateAsync(InvoiceCreateDto dto);
        Task<bool> UpdateAsync(InvoiceUpdateDto dto);
        Task<bool> UpdateStatusAsync(int id, InvoiceStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}