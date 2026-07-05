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

        public async Task<IEnumerable<Staff>> GetAllAsync(string? search)
        {
            using var db = _context.CreateConnection();
            var sql = "SELECT * FROM Users WHERE 1=1";
            if (!string.IsNullOrEmpty(search))
                sql += @" AND (FullName LIKE @Search
                           OR Email LIKE @Search
                           OR Role LIKE @Search)";
            sql += " ORDER BY CreatedAt DESC";
            return await db.QueryAsync<Staff>(sql,
                new { Search = $"%{search}%" });
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
            return await db.ExecuteAsync(
                "DELETE FROM Users WHERE Id=@Id",
                new { Id = id }) > 0;
        }
    }
}