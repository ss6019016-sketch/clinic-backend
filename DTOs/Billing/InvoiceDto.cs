using System.ComponentModel.DataAnnotations;

namespace clinic.DTOs.Billing
{
    public class InvoiceCreateDto
    {
        [Required] public int PatientId { get; set; }
        [Required] public string InvoiceDate { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Unpaid";
        public decimal Discount { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public string Notes { get; set; } = string.Empty;
        public int? AppointmentId { get; set; }

        [Required] public List<InvoiceItemDto> Items { get; set; } = new();
    }

    public class InvoiceItemDto
    {
        [Required] public string ItemName { get; set; } = string.Empty;
        [Required] public int Quantity { get; set; } = 1;
        [Required] public decimal Price { get; set; }
    }

    public class InvoiceUpdateDto : InvoiceCreateDto
    {
        public int Id { get; set; }
    }

    public class InvoiceStatusDto
    {
        [Required] public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }
}