using clinic.DTOs.Prescription;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<PagedResult<Prescription>> GetAllAsync(string? search, int page, int pageSize);
        Task<Prescription?> GetByIdAsync(int id);
        Task<IEnumerable<Prescription>> GetByPatientAsync(int patientId);
        Task<int> CreateAsync(PrescriptionCreateDto dto);
        Task<bool> UpdateAsync(PrescriptionUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}