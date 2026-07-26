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

        public async Task<PagedResult<Appointment>> GetAllAsync(
            string? status, string? search, int page, int pageSize)
        {
            using var db = _context.CreateConnection();
            var whereClause = @"
                WHERE a.IsDeleted = 0";

            if (!string.IsNullOrEmpty(status) && status != "All")
                whereClause += " AND a.Status=@Status";
            if (!string.IsNullOrEmpty(search))
                whereClause += " AND (p.FullName LIKE @Search OR d.FullName LIKE @Search)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.Id
                JOIN Doctors  d ON a.DoctorId  = d.Id
                {whereClause}";
            var dataSql = $@"
                SELECT a.*, p.FullName AS PatientName, p.Phone AS PatientPhone, d.FullName AS DoctorName
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.Id
                JOIN Doctors  d ON a.DoctorId  = d.Id
                {whereClause}
                ORDER BY a.AppointmentDate DESC, a.AppointmentTime
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Status = status,
                Search = $"%{search}%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await db.QueryAsync<Appointment>(dataSql, parameters);

            return new PagedResult<Appointment>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
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
                  AND a.ReminderSent = 0
                  AND a.IsDeleted = 0",
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
                "UPDATE Appointments SET IsDeleted = 1, DeletedAt = GETDATE() WHERE Id=@Id AND IsDeleted = 0",
                new { Id = id }) > 0;
        }

        public async Task<IEnumerable<Appointment>> GetTrashAsync()
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT a.*, p.FullName AS PatientName, p.Phone AS PatientPhone, d.FullName AS DoctorName
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.Id
                JOIN Doctors  d ON a.DoctorId  = d.Id
                WHERE a.IsDeleted = 1
                ORDER BY a.DeletedAt DESC";
            return await db.QueryAsync<Appointment>(sql);
        }

        public async Task<bool> RestoreAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Appointments SET IsDeleted = 0, DeletedAt = NULL WHERE Id=@Id AND IsDeleted = 1",
                new { Id = id }) > 0;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "DELETE FROM Appointments WHERE Id=@Id AND IsDeleted = 1",
                new { Id = id }) > 0;
        }
    }
}