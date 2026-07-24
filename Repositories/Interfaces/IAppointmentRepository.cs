using clinic.DTOs.Appointment;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<PagedResult<Appointment>> GetAllAsync(string? status, string? search, int page, int pageSize);
        Task<Appointment?> GetByIdAsync(int id);
        Task<int> CreateAsync(AppointmentCreateDto dto);
        Task<bool> UpdateAsync(AppointmentUpdateDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);

        // Reminder support
        Task<IEnumerable<Appointment>> GetPendingRemindersAsync(DateTime date);
        Task<bool> MarkReminderSentAsync(int id);
    }
}