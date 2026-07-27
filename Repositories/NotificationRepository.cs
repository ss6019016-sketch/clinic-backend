using Dapper;
using clinic.Data;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DapperContext _context;
        public NotificationRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Notification>> GetAllAsync(int limit = 30)
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<Notification>(
                "SELECT TOP (@Limit) * FROM Notifications ORDER BY CreatedAt DESC",
                new { Limit = limit });
        }

        public async Task<int> GetUnreadCountAsync()
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Notifications WHERE IsRead = 0");
        }

        public async Task<int> CreateAsync(Notification n)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Notifications (Title, Message, Type, Link)
                VALUES (@Title, @Message, @Type, @Link);
                SELECT SCOPE_IDENTITY();", n);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Notifications SET IsRead = 1 WHERE Id=@Id",
                new { Id = id }) > 0;
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Notifications SET IsRead = 1 WHERE IsRead = 0") >= 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "DELETE FROM Notifications WHERE Id=@Id", new { Id = id }) > 0;
        }
    }
}