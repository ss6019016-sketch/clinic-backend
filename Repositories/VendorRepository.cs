using Dapper;
using clinic.Data;
using clinic.DTOs.Vendor;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class VendorRepository : IVendorRepository
    {
        private readonly DapperContext _context;
        public VendorRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Vendor>> GetAllAsync(string? search)
        {
            using var db = _context.CreateConnection();
            var where = "WHERE Status='Active'";
            if (!string.IsNullOrEmpty(search))
                where += " AND (Name LIKE @Search OR ContactPerson LIKE @Search)";

            return await db.QueryAsync<Vendor>(
                $"SELECT * FROM Vendors {where} ORDER BY CreatedAt DESC",
                new { Search = $"%{search}%" });
        }

        public async Task<Vendor?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Vendor>(
                "SELECT * FROM Vendors WHERE Id=@Id", new { Id = id });
        }

        public async Task<int> CreateAsync(VendorCreateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Vendors (Name, ContactPerson, Phone, Email, Address)
                VALUES (@Name, @ContactPerson, @Phone, @Email, @Address);
                SELECT SCOPE_IDENTITY();", dto);
        }

        public async Task<bool> UpdateAsync(VendorUpdateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(@"
                UPDATE Vendors SET
                    Name=@Name, ContactPerson=@ContactPerson,
                    Phone=@Phone, Email=@Email, Address=@Address
                WHERE Id=@Id", dto) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Vendors SET Status='Inactive' WHERE Id=@Id AND Status='Active'",
                new { Id = id }) > 0;
        }
    }
}