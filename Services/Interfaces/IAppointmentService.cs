using clinic.DTOs.Appointment;
using clinic.Models;

namespace clinic.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAllAsync(string? status, string? search);
        Task<Appointment?> GetByIdAsync(int id);
        Task<int> CreateAsync(AppointmentCreateDto dto);
        Task<bool> UpdateAsync(int id, AppointmentCreateDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
    }
}