using Dapper;
using clinic.Data;
using clinic.DTOs.LabReport;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class LabReportRepository : ILabReportRepository
    {
        private readonly DapperContext _context;
        public LabReportRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<LabReportListDto>> GetByPatientIdAsync(int patientId)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT lr.Id, lr.PatientId, p.FullName AS PatientName,
                       lr.DoctorId, d.FullName AS DoctorName,
                       lr.TestName, lr.ReportDate, lr.FileName, lr.FileType,
                       lr.Notes, lr.CreatedAt
                FROM LabReports lr
                JOIN Patients p ON lr.PatientId = p.Id
                LEFT JOIN Doctors d ON lr.DoctorId = d.Id
                WHERE lr.PatientId = @PatientId
                ORDER BY lr.ReportDate DESC, lr.CreatedAt DESC";
            return await db.QueryAsync<LabReportListDto>(sql, new { PatientId = patientId });
        }

        public async Task<LabReport?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT lr.*, p.FullName AS PatientName, d.FullName AS DoctorName
                FROM LabReports lr
                JOIN Patients p ON lr.PatientId = p.Id
                LEFT JOIN Doctors d ON lr.DoctorId = d.Id
                WHERE lr.Id = @Id";
            return await db.QueryFirstOrDefaultAsync<LabReport>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(LabReportCreateDto dto, string fileName, string fileType, string fileData)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                INSERT INTO LabReports
                    (PatientId, DoctorId, TestName, ReportDate, FileName, FileType, FileData, Notes, CreatedAt)
                VALUES
                    (@PatientId, @DoctorId, @TestName, @ReportDate, @FileName, @FileType, @FileData, @Notes, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await db.QuerySingleAsync<int>(sql, new
            {
                dto.PatientId,
                dto.DoctorId,
                dto.TestName,
                dto.ReportDate,
                FileName = fileName,
                FileType = fileType,
                FileData = fileData,
                dto.Notes
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync("DELETE FROM LabReports WHERE Id=@Id", new { Id = id });
            return rows > 0;
        }
    }
}