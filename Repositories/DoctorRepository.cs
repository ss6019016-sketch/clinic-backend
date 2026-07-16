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

        public async Task<IEnumerable<Doctor>> GetAllAsync(string? search)
        {
            using var db = _context.CreateConnection();
            var sql = "SELECT * FROM Doctors WHERE Status='Active'";
            if (!string.IsNullOrEmpty(search))
                sql += " AND (FullName LIKE @Search OR Specialization LIKE @Search)";
            sql += " ORDER BY CreatedAt DESC";
            return await db.QueryAsync<Doctor>(sql,
                new { Search = $"%{search}%" });
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
             Fee, AvailableDays, Qualification, LicenseNumber, Bio, ProfilePhoto)
        VALUES
            (@FullName, @Specialization, @Phone, @Email, @Experience,
             @Fee, @AvailableDays, @Qualification, @LicenseNumber, @Bio, @ProfilePhoto);
        SELECT SCOPE_IDENTITY();", dto);
        }




        public async Task<bool> UpdateAsync(DoctorUpdateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(@"
        UPDATE Doctors SET
            FullName=@FullName, Specialization=@Specialization,
            Phone=@Phone, Email=@Email, Experience=@Experience,
            Fee=@Fee, AvailableDays=@AvailableDays,
            Qualification=@Qualification,
            LicenseNumber=@LicenseNumber, Bio=@Bio,
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