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

        public async Task<IEnumerable<Patient>> GetAllAsync(string? search)
        {
            using var db = _context.CreateConnection();
            var sql = "SELECT * FROM Patients WHERE Status='Active'";
            if (!string.IsNullOrEmpty(search))
                sql += " AND (FullName LIKE @Search OR Phone LIKE @Search)";
            sql += " ORDER BY CreatedAt DESC";
            return await db.QueryAsync<Patient>(sql,
                new { Search = $"%{search}%" });
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
            return await db.ExecuteAsync(
                "UPDATE Patients SET Status='Deleted' WHERE Id=@Id",
                new { Id = id }) > 0;
        }
    }
}