using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using clinic.Data;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly DapperContext _context;
        public ReportsController(DapperContext context) => _context = context;

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] string? from,
            [FromQuery] string? to)
        {
            using var db = _context.CreateConnection();

            var fromDate = string.IsNullOrEmpty(from)
                ? DateTime.Now.AddDays(-30)
                : DateTime.Parse(from);
            var toDate = string.IsNullOrEmpty(to)
                ? DateTime.Now
                : DateTime.Parse(to);

            var data = await db.QueryAsync(@"
                SELECT
                    CAST(CreatedAt AS DATE) AS Date,
                    SUM(GrandTotal)         AS Total,
                    COUNT(*)                AS Count
                FROM Invoices
                WHERE Status='Paid'
                  AND CreatedAt BETWEEN @From AND @To
                GROUP BY CAST(CreatedAt AS DATE)
                ORDER BY CAST(CreatedAt AS DATE)",
                new { From = fromDate, To = toDate });

            return Ok(data);
        }

        [HttpGet("top-doctors")]
        public async Task<IActionResult> GetTopDoctors()
        {
            using var db = _context.CreateConnection();
            var data = await db.QueryAsync(@"
                SELECT TOP 5
                    d.FullName         AS DoctorName,
                    d.Specialization,
                    COUNT(DISTINCT a.PatientId) AS TotalPatients,
                    ISNULL(SUM(i.GrandTotal), 0) AS TotalRevenue
                FROM Doctors d
                LEFT JOIN Appointments a ON d.Id = a.DoctorId
                LEFT JOIN Invoices i ON a.Id = i.AppointmentId AND i.Status='Paid'
                GROUP BY d.Id, d.FullName, d.Specialization
                ORDER BY TotalRevenue DESC");

            return Ok(data);
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointmentStats(
            [FromQuery] string? from,
            [FromQuery] string? to)
        {
            using var db = _context.CreateConnection();

            var fromDate = string.IsNullOrEmpty(from)
                ? DateTime.Now.AddDays(-7)
                : DateTime.Parse(from);
            var toDate = string.IsNullOrEmpty(to)
                ? DateTime.Now
                : DateTime.Parse(to);

            var data = await db.QueryAsync(@"
                SELECT
                    CAST(AppointmentDate AS DATE) AS Date,
                    COUNT(*) AS Total,
                    SUM(CASE WHEN Status='Confirmed'  THEN 1 ELSE 0 END) AS Confirmed,
                    SUM(CASE WHEN Status='Pending'    THEN 1 ELSE 0 END) AS Pending,
                    SUM(CASE WHEN Status='Cancelled'  THEN 1 ELSE 0 END) AS Cancelled,
                    SUM(CASE WHEN Status='Completed'  THEN 1 ELSE 0 END) AS Completed
                FROM Appointments
                WHERE AppointmentDate BETWEEN @From AND @To
                GROUP BY CAST(AppointmentDate AS DATE)
                ORDER BY CAST(AppointmentDate AS DATE)",
                new { From = fromDate, To = toDate });

            return Ok(data);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            using var db = _context.CreateConnection();

            var totalRevenue = await db.ExecuteScalarAsync<decimal>(
                "SELECT ISNULL(SUM(GrandTotal),0) FROM Invoices WHERE Status='Paid'");

            var totalPatients = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Patients WHERE Status='Active'");

            var totalAppointments = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Appointments");

            return Ok(new
            {
                totalRevenue,
                totalPatients,
                totalAppointments
            });
        }
    }
}