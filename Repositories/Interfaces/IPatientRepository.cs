using clinic.DTOs.Patient;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
 
       
    public interface IPatientRepository
    {
        Task<PagedResult<Patient>> GetAllAsync(string? search, int page, int pageSize);
        Task<Patient?> GetByIdAsync(int id);
        Task<int> CreateAsync(PatientCreateDto dto);
        Task<bool> UpdateAsync(PatientUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}