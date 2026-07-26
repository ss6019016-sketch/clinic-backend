using Dapper;
using clinic.Data;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class DoctorAvailabilityRepository : IDoctorAvailabilityRepository
    {
        private readonly DapperContext _context;
        public DoctorAvailabilityRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId)
        {
            using var db = _context.CreateConnection();
            var sql = @"SELECT da.*, d.FullName AS DoctorName 
                        FROM DoctorAvailability da
                        INNER JOIN Doctors d ON d.Id = da.DoctorId
                        WHERE da.DoctorId=@DoctorId AND da.IsActive=1
                        ORDER BY FIELD(da.DayOfWeek,'Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')";
            return await db.QueryAsync<DoctorAvailability>(sql, new { DoctorId = doctorId });
        }

        public async Task<DoctorAvailability?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<DoctorAvailability>(
                "SELECT * FROM DoctorAvailability WHERE Id=@Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DoctorAvailability entity)
        {
            using var db = _context.CreateConnection();
            var sql = @"INSERT INTO DoctorAvailability 
                        (DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes, IsActive, CreatedAt)
                        VALUES (@DoctorId, @DayOfWeek, @StartTime, @EndTime, @SlotDurationMinutes, @IsActive, NOW());
                        SELECT LAST_INSERT_ID();";
            return await db.QuerySingleAsync<int>(sql, entity);
        }

        public async Task<bool> UpdateAsync(DoctorAvailability entity)
        {
            using var db = _context.CreateConnection();
            var sql = @"UPDATE DoctorAvailability SET 
                        DayOfWeek=@DayOfWeek, StartTime=@StartTime, EndTime=@EndTime,
                        SlotDurationMinutes=@SlotDurationMinutes, IsActive=@IsActive
                        WHERE Id=@Id";
            var rows = await db.ExecuteAsync(sql, entity);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync("DELETE FROM DoctorAvailability WHERE Id=@Id", new { Id = id });
            return rows > 0;
        }

        public async Task<IEnumerable<string>> GetBookedTimesAsync(int doctorId, DateTime date)
        {
            using var db = _context.CreateConnection();
            var sql = @"SELECT AppointmentTime FROM Appointments 
                        WHERE DoctorId=@DoctorId AND DATE(AppointmentDate)=DATE(@Date)
                        AND Status <> 'Cancelled'";
            return await db.QueryAsync<string>(sql, new { DoctorId = doctorId, Date = date });
        }
    }
}