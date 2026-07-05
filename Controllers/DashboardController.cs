using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using clinic.Data;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DapperContext _context;
        public DashboardController(DapperContext context) => _context = context;

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            using var db = _context.CreateConnection();

            var totalPatients = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Patients WHERE Status='Active'");

            var totalDoctors = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Doctors WHERE Status='Active'");

            var todayAppointments = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Appointments WHERE CAST(AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)");

            var pendingBills = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Invoices WHERE Status='Unpaid'");

            var recentAppointments = await db.QueryAsync(@"
                SELECT TOP 5
                    a.Id, p.FullName AS PatientName,
                    d.FullName AS DoctorName,
                    a.AppointmentTime AS Time,
                    a.Status
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.Id
                JOIN Doctors d  ON a.DoctorId  = d.Id
                ORDER BY a.CreatedAt DESC");

            return Ok(new
            {
                totalPatients,
                totalDoctors,
                todayAppointments,
                pendingBills,
                recentAppointments
            });
        }
    }
}