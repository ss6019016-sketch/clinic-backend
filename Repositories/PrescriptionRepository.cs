using Dapper;
using clinic.Data;
using clinic.DTOs.Prescription;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly DapperContext _context;
        public PrescriptionRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Prescription>> GetAllAsync(string? search)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT pr.*, p.FullName AS PatientName, d.FullName AS DoctorName
                FROM Prescriptions pr
                JOIN Patients p ON pr.PatientId = p.Id
                JOIN Doctors d  ON pr.DoctorId  = d.Id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(search))
                sql += @" AND (p.FullName LIKE @Search
                           OR d.FullName LIKE @Search
                           OR pr.Diagnosis LIKE @Search)";

            sql += " ORDER BY pr.CreatedAt DESC";
            return await db.QueryAsync<Prescription>(sql,
                new { Search = $"%{search}%" });
        }

        public async Task<Prescription?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT pr.*, p.FullName AS PatientName, d.FullName AS DoctorName
                FROM Prescriptions pr
                JOIN Patients p ON pr.PatientId = p.Id
                JOIN Doctors d  ON pr.DoctorId  = d.Id
                WHERE pr.Id = @Id";

            var rx = await db.QueryFirstOrDefaultAsync<Prescription>(sql, new { Id = id });
            if (rx == null) return null;

            rx.Medicines = (await db.QueryAsync<PrescriptionItem>(
                "SELECT * FROM PrescriptionItems WHERE PrescriptionId=@Id",
                new { Id = id })).ToList();

            return rx;
        }

        public async Task<IEnumerable<Prescription>> GetByPatientAsync(int patientId)
        {
            using var db = _context.CreateConnection();
            var rxList = (await db.QueryAsync<Prescription>(@"
                SELECT pr.*, d.FullName AS DoctorName
                FROM Prescriptions pr
                JOIN Doctors d ON pr.DoctorId = d.Id
                WHERE pr.PatientId = @PatientId
                ORDER BY pr.CreatedAt DESC",
                new { PatientId = patientId })).ToList();

            foreach (var rx in rxList)
            {
                rx.Medicines = (await db.QueryAsync<PrescriptionItem>(
                    "SELECT * FROM PrescriptionItems WHERE PrescriptionId=@Id",
                    new { Id = rx.Id })).ToList();
            }
            return rxList;
        }

        public async Task<int> CreateAsync(PrescriptionCreateDto dto)
        {
            using var db = _context.CreateConnection();

            var rxId = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Prescriptions
                    (PatientId, DoctorId, AppointmentId,
                     Diagnosis, Notes, FollowUpDate)
                VALUES
                    (@PatientId, @DoctorId, @AppointmentId,
                     @Diagnosis, @Notes, @FollowUpDate);
                SELECT SCOPE_IDENTITY();",
                new
                {
                    dto.PatientId,
                    dto.DoctorId,
                    dto.AppointmentId,
                    dto.Diagnosis,
                    dto.Notes,
                    FollowUpDate = string.IsNullOrEmpty(dto.FollowUpDate)
                        ? (DateTime?)null
                        : DateTime.Parse(dto.FollowUpDate)
                });

            foreach (var med in dto.Medicines)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO PrescriptionItems
                        (PrescriptionId, MedicineName, Dosage,
                         Frequency, Duration, Instructions)
                    VALUES
                        (@PrescriptionId, @MedicineName, @Dosage,
                         @Frequency, @Duration, @Instructions)",
                    new
                    {
                        PrescriptionId = rxId,
                        med.MedicineName,
                        med.Dosage,
                        med.Frequency,
                        med.Duration,
                        med.Instructions
                    });
            }
            return rxId;
        }

        public async Task<bool> UpdateAsync(PrescriptionUpdateDto dto)
        {
            using var db = _context.CreateConnection();

            await db.ExecuteAsync(@"
                UPDATE Prescriptions SET
                    PatientId=@PatientId, DoctorId=@DoctorId,
                    Diagnosis=@Diagnosis, Notes=@Notes,
                    FollowUpDate=@FollowUpDate
                WHERE Id=@Id",
                new
                {
                    dto.Id,
                    dto.PatientId,
                    dto.DoctorId,
                    dto.Diagnosis,
                    dto.Notes,
                    FollowUpDate = string.IsNullOrEmpty(dto.FollowUpDate)
                        ? (DateTime?)null
                        : DateTime.Parse(dto.FollowUpDate)
                });

            // Medicines refresh
            await db.ExecuteAsync(
                "DELETE FROM PrescriptionItems WHERE PrescriptionId=@Id",
                new { dto.Id });

            foreach (var med in dto.Medicines)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO PrescriptionItems
                        (PrescriptionId, MedicineName, Dosage,
                         Frequency, Duration, Instructions)
                    VALUES
                        (@PrescriptionId, @MedicineName, @Dosage,
                         @Frequency, @Duration, @Instructions)",
                    new
                    {
                        PrescriptionId = dto.Id,
                        med.MedicineName,
                        med.Dosage,
                        med.Frequency,
                        med.Duration,
                        med.Instructions
                    });
            }
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "DELETE FROM Prescriptions WHERE Id=@Id",
                new { Id = id }) > 0;
        }
    }
}