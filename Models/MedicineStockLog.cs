namespace clinic.Models
{
    public class MedicineStockLog
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }  // joined
        public string ChangeType { get; set; } = string.Empty; // Purchase, Prescription, Adjustment
        public int QuantityChange { get; set; }
        public int? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}