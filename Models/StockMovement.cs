namespace clinic.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty; // joined from Medicines
        public string ChangeType { get; set; } = string.Empty;   // Purchase / Prescription / Adjustment / Return
        public int QuantityChange { get; set; }
        public int? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}