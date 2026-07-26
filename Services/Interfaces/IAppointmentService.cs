using clinic.DTOs.Appointment;
using clinic.Models;

namespace clinic.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<PagedResult<Appointment>> GetAllAsync(string? status, string? search, int page, int pageSize);
        Task<Appointment?> GetByIdAsync(int id);
        Task<int> CreateAsync(AppointmentCreateDto dto);
        Task<bool> UpdateAsync(int id, AppointmentCreateDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);
        Task<(bool success, string message)> SendReminderNowAsync(int id);

        // Soft-delete / Trash support
        Task<IEnumerable<Appointment>> GetTrashAsync();
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}