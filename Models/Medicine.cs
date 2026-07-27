namespace clinic.Models
{
    public class Medicine
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = "Tablet";
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? VendorId { get; set; }
        public string? VendorName { get; set; }   // joined, read-only
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }

        public bool IsLowStock => StockQuantity <= ReorderLevel;
    }
}