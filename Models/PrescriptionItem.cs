namespace clinic.Models
{
    public class PrescriptionItem
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public int? MedicineId { get; set; }       // NEW - links to Medicines table (nullable = old records safe)
        public int Quantity { get; set; } = 1;      // NEW - qty deducted from stock
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
    }
}