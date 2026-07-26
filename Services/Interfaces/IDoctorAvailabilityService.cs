using clinic.DTOs.DoctorAvailability;
using clinic.Models;

namespace clinic.Services.Interfaces
{
    public interface IDoctorAvailabilityService
    {
        Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId);
        Task<int> CreateAsync(DoctorAvailabilityCreateDto dto);
        Task<bool> UpdateAsync(int id, DoctorAvailabilityCreateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date);
        Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, string time);
    }
}