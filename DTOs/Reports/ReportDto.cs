namespace clinic.DTOs.Reports
{
    public class RevenueReportDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Count { get; set; }
    }

    public class DoctorReportDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int TotalPatients { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingBills { get; set; }
        public decimal TodayRevenue { get; set; }
        public IEnumerable<object> RecentAppointments { get; set; } = new List<object>();
    }
}