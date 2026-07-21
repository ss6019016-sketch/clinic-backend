using Dapper;
using clinic.Data;
using clinic.DTOs.Appointment;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DapperContext _context;
        public AppointmentRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Appointment>> GetAllAsync(
            string? status, string? search)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT a.*, p.FullName AS PatientName, p.Phone AS PatientPhone, d.FullName AS DoctorName
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.Id
                JOIN Doctors  d ON a.DoctorId  = d.Id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(status) && status != "All")
                sql += " AND a.Status=@Status";
            if (!string.IsNullOrEmpty(search))
                sql += " AND (p.FullName LIKE @Search OR d.FullName LIKE @Search)";

            sql += " ORDER BY a.AppointmentDate DESC, a.AppointmentTime";
            return await db.QueryAsync<Appointment>(sql,
                new { Status = status, Search = $"%{search}%" });
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Appointment>(@"
                SELECT a.*, p.FullName AS PatientName, p.Phone AS PatientPhone, d.FullName AS DoctorName
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.Id
                JOIN Doctors  d ON a.DoctorId  = d.Id
                WHERE a.Id=@Id", new { Id = id });
        }

        public async Task<IEnumerable<Appointment>> GetPendingRemindersAsync(DateTime date)
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<Appointment>(@"
                SELECT a.*, p.FullName AS PatientName, p.Phone AS PatientPhone, d.FullName AS DoctorName
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.Id
                JOIN Doctors  d ON a.DoctorId  = d.Id
                WHERE a.AppointmentDate = @Date
                  AND a.Status IN ('Pending', 'Confirmed')
                  AND a.ReminderSent = 0",
                new { Date = date.Date });
        }

        public async Task<bool> MarkReminderSentAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Appointments SET ReminderSent = 1 WHERE Id=@Id",
                new { Id = id }) > 0;
        }

        public async Task<int> CreateAsync(AppointmentCreateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Appointments
                    (PatientId, DoctorId, AppointmentDate, AppointmentTime,
                     Status, Reason, Type, Notes)
                VALUES
                    (@PatientId, @DoctorId, @AppointmentDate, @AppointmentTime,
                     @Status, @Reason, @Type, @Notes);
                SELECT SCOPE_IDENTITY();", dto);
        }

        public async Task<bool> UpdateAsync(AppointmentUpdateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(@"
                UPDATE Appointments SET
                    PatientId=@PatientId, DoctorId=@DoctorId,
                    AppointmentDate=@AppointmentDate,
                    AppointmentTime=@AppointmentTime,
                    Status=@Status, Reason=@Reason,
                    Type=@Type, Notes=@Notes
                WHERE Id=@Id", dto) > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Appointments SET Status=@Status WHERE Id=@Id",
                new { Id = id, Status = status }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "DELETE FROM Appointments WHERE Id=@Id",
                new { Id = id }) > 0;
        }
    }
}