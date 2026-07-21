using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task LogAsync(AuditLog log);

        Task<IEnumerable<AuditLog>> GetAllAsync(
            string? entity,
            int? userId,
            DateTime? from,
            DateTime? to);
    }
}