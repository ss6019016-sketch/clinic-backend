namespace clinic.DTOs.Reports
{
    public class RevenueReportDto
    {
        public DateTime Date { get; set; }
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

    public class AppointmentStatDto
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
        public int Confirmed { get; set; }
        public int Pending { get; set; }
        public int Cancelled { get; set; }
        public int Completed { get; set; }
    }

    public class InvoiceStatusReportDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaymentMethodReportDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class RecentAppointmentDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
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