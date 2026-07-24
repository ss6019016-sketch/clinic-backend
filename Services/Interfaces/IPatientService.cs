using clinic.DTOs.Patient;
using clinic.Models;

namespace clinic.Services.Interfaces
{
   public interface IPatientService
    {
        Task<PagedResult<Patient>> GetAllAsync(string? search, int page, int pageSize);
        Task<Patient?> GetByIdAsync(int id);
        Task<int> CreateAsync(PatientCreateDto dto);
        Task<bool> UpdateAsync(int id, PatientCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}