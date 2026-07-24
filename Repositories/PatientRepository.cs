using Dapper;
using clinic.Data;
using clinic.DTOs.Patient;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly DapperContext _context;
        public PatientRepository(DapperContext context) => _context = context;

        public async Task<PagedResult<Patient>> GetAllAsync(string? search, int page, int pageSize)
        {
            using var db = _context.CreateConnection();

            var whereClause = "WHERE Status='Active'";
            if (!string.IsNullOrEmpty(search))
                whereClause += " AND (FullName LIKE @Search OR Phone LIKE @Search)";

            var countSql = $"SELECT COUNT(*) FROM Patients {whereClause}";
            var dataSql = $@"SELECT * FROM Patients {whereClause}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Search = $"%{search}%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await db.QueryAsync<Patient>(dataSql, parameters);

            return new PagedResult<Patient>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Patient>(
                "SELECT * FROM Patients WHERE Id=@Id", new { Id = id });
        }

        public async Task<int> CreateAsync(PatientCreateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Patients
                    (FullName, Gender, Age, Phone, Email, Address,
                     BloodGroup, Disease, MedicalHistory, EmergencyContact)
                VALUES
                    (@FullName, @Gender, @Age, @Phone, @Email, @Address,
                     @BloodGroup, @Disease, @MedicalHistory, @EmergencyContact);
                SELECT SCOPE_IDENTITY();", dto);
        }

        public async Task<bool> UpdateAsync(PatientUpdateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(@"
                UPDATE Patients SET
                    FullName=@FullName, Gender=@Gender, Age=@Age,
                    Phone=@Phone, Email=@Email, Address=@Address,
                    BloodGroup=@BloodGroup, Disease=@Disease,
                    MedicalHistory=@MedicalHistory,
                    EmergencyContact=@EmergencyContact
                WHERE Id=@Id", dto) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            // Soft delete
            var rows = await db.ExecuteAsync(
                "UPDATE Patients SET Status='Deleted' WHERE Id=@Id AND Status='Active'",
                new { Id = id });
            return rows > 0;
        }
    }
}