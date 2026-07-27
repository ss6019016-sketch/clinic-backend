using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllAsync(int limit = 30);
        Task<int> GetUnreadCountAsync();
        Task<int> CreateAsync(Notification notification);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync();
        Task<bool> DeleteAsync(int id);
    }
}