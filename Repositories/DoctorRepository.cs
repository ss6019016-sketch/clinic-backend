using Dapper;
using clinic.Data;
using clinic.DTOs.Doctor;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly DapperContext _context;
        public DoctorRepository(DapperContext context) => _context = context;

        public async Task<PagedResult<Doctor>> GetAllAsync(string? search, int page, int pageSize)
        {
            using var db = _context.CreateConnection();
            var whereClause = "WHERE Status='Active'";
            if (!string.IsNullOrEmpty(search))
                whereClause += " AND (FullName LIKE @Search OR Specialization LIKE @Search)";

            var countSql = $"SELECT COUNT(*) FROM Doctors {whereClause}";
            var dataSql = $@"SELECT * FROM Doctors {whereClause}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Search = $"%{search}%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await db.QueryAsync<Doctor>(dataSql, parameters);

            return new PagedResult<Doctor>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Doctor>(
                "SELECT * FROM Doctors WHERE Id=@Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DoctorCreateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(@"
        INSERT INTO Doctors
            (FullName, Specialization, Phone, Email, Experience,
             Fee, AvailableDays, Qualification, LicenseNumber,
             Bio, ProfilePhoto)
        VALUES
            (@FullName, @Specialization, @Phone, @Email, @Experience,
             @Fee, @AvailableDays, @Qualification, @LicenseNumber,
             @Bio, @ProfilePhoto);
        SELECT SCOPE_IDENTITY();", dto);
        }



        public async Task<bool> UpdateAsync(DoctorUpdateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(@"
        UPDATE Doctors SET
            FullName=@FullName,
            Specialization=@Specialization,
            Phone=@Phone,
            Email=@Email,
            Experience=@Experience,
            Fee=@Fee,
            AvailableDays=@AvailableDays,
            Qualification=@Qualification,
            LicenseNumber=@LicenseNumber,
            Bio=@Bio,
            ProfilePhoto=@ProfilePhoto
        WHERE Id=@Id", dto) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Doctors SET Status='Inactive' WHERE Id=@Id",
                new { Id = id }) > 0;
        }
    }
}