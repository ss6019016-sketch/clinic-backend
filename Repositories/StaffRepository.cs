using Dapper;
using clinic.Data;
using clinic.DTOs.Staff;
using clinic.Helpers;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly DapperContext _context;
        public StaffRepository(DapperContext context) => _context = context;

        public async Task<PagedResult<Staff>> GetAllAsync(string? search, int page, int pageSize)
        {
            using var db = _context.CreateConnection();
            var whereClause = "WHERE IsDeleted = 0";
            if (!string.IsNullOrEmpty(search))
                whereClause += @" AND (FullName LIKE @Search
                           OR Email LIKE @Search
                           OR Role LIKE @Search)";

            var countSql = $"SELECT COUNT(*) FROM Users {whereClause}";
            var dataSql = $@"SELECT * FROM Users {whereClause}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Search = $"%{search}%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await db.QueryAsync<Staff>(dataSql, parameters);

            return new PagedResult<Staff>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Staff?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Staff>(
                "SELECT * FROM Users WHERE Id=@Id", new { Id = id });
        }

        public async Task<int> CreateAsync(StaffCreateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Users
                    (FullName, Email, Phone, Password, Role, Status)
                VALUES
                    (@FullName, @Email, @Phone, @Password, @Role, @Status);
                SELECT SCOPE_IDENTITY();",
                new
                {
                    dto.FullName,
                    dto.Email,
                    dto.Phone,
                    Password = PasswordHelper.Hash(dto.Password),
                    dto.Role,
                    dto.Status
                });
        }

        public async Task<bool> UpdateAsync(StaffUpdateDto dto)
        {
            using var db = _context.CreateConnection();

            if (!string.IsNullOrEmpty(dto.Password))
            {
                return await db.ExecuteAsync(@"
                    UPDATE Users SET
                        FullName=@FullName, Email=@Email, Phone=@Phone,
                        Role=@Role, Status=@Status,
                        Password=@Password
                    WHERE Id=@Id",
                    new
                    {
                        dto.Id,
                        dto.FullName,
                        dto.Email,
                        dto.Phone,
                        dto.Role,
                        dto.Status,
                        Password = PasswordHelper.Hash(dto.Password)
                    }) > 0;
            }

            return await db.ExecuteAsync(@"
                UPDATE Users SET
                    FullName=@FullName, Email=@Email,
                    Phone=@Phone, Role=@Role, Status=@Status
                WHERE Id=@Id",
                new
                {
                    dto.Id,
                    dto.FullName,
                    dto.Email,
                    dto.Phone,
                    dto.Role,
                    dto.Status
                }) > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Users SET Status=@Status WHERE Id=@Id",
                new { Id = id, Status = status }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            // Soft delete — data preserved, just hidden + login blocked
            return await db.ExecuteAsync(
                "UPDATE Users SET IsDeleted = 1, DeletedAt = GETDATE() WHERE Id=@Id AND IsDeleted = 0",
                new { Id = id }) > 0;
        }

        public async Task<IEnumerable<Staff>> GetTrashAsync()
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<Staff>(
                "SELECT * FROM Users WHERE IsDeleted = 1 ORDER BY DeletedAt DESC");
        }

        public async Task<bool> RestoreAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Users SET IsDeleted = 0, DeletedAt = NULL WHERE Id=@Id AND IsDeleted = 1",
                new { Id = id }) > 0;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "DELETE FROM Users WHERE Id=@Id AND IsDeleted = 1",
                new { Id = id }) > 0;
        }
    }
}