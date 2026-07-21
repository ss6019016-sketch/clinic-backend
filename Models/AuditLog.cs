using System;

namespace clinic.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;   // Create, Update, Delete, StatusChange
        public string Entity { get; set; } = string.Empty;   // Doctor, Patient, Invoice, Staff, etc.
        public int EntityId { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}