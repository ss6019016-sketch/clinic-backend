using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IDoctorAvailabilityRepository
    {
        Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId);
        Task<DoctorAvailability?> GetByIdAsync(int id);
        Task<int> CreateAsync(DoctorAvailability entity);
        Task<bool> UpdateAsync(DoctorAvailability entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<string>> GetBookedTimesAsync(int doctorId, DateTime date);
    }
}