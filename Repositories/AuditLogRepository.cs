using Dapper;
using clinic.Data;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DapperContext _context;
        public AuditLogRepository(DapperContext context) => _context = context;

        public async Task LogAsync(AuditLog log)
        {
            using var db = _context.CreateConnection();
            await db.ExecuteAsync(@"
                INSERT INTO AuditLogs
                    (UserId, UserName, Role, Action, Entity, EntityId, Details, Timestamp)
                VALUES
                    (@UserId, @UserName, @Role, @Action, @Entity, @EntityId, @Details, @Timestamp)",
                log);
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(
            string? entity, int? userId, DateTime? from, DateTime? to)
        {
            using var db = _context.CreateConnection();

            var sql = "SELECT * FROM AuditLogs WHERE 1=1";

            if (!string.IsNullOrEmpty(entity))
                sql += " AND Entity = @Entity";
            if (userId.HasValue)
                sql += " AND UserId = @UserId";
            if (from.HasValue)
                sql += " AND Timestamp >= @From";
            if (to.HasValue)
                sql += " AND Timestamp <= @To";

            sql += " ORDER BY Timestamp DESC";

            return await db.QueryAsync<AuditLog>(sql,
                new { Entity = entity, UserId = userId, From = from, To = to });
        }
    }
}