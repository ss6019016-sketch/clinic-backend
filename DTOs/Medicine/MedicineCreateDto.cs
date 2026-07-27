using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Medicine
{
    public class MedicineCreateDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = "Tablet";
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? VendorId { get; set; }
    }

    public class MedicineUpdateDto : MedicineCreateDto
    {
        public int Id { get; set; }
    }

    public class StockAdjustDto
    {
        [Required] public int Quantity { get; set; }   // +ve add, -ve remove
        public string? Notes { get; set; }
    }
}