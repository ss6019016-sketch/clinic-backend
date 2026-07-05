using Dapper;
using clinic.Data;
using clinic.DTOs.Billing;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class BillingRepository : IBillingRepository
    {
        private readonly DapperContext _context;
        public BillingRepository(DapperContext context) => _context = context;

        public async Task<IEnumerable<Invoice>> GetAllAsync(
            string? status, string? search)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT i.*, p.FullName AS PatientName
                FROM Invoices i
                JOIN Patients p ON i.PatientId = p.Id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(status) && status != "All")
                sql += " AND i.Status = @Status";
            if (!string.IsNullOrEmpty(search))
                sql += " AND (p.FullName LIKE @Search OR i.InvoiceNumber LIKE @Search)";

            sql += " ORDER BY i.CreatedAt DESC";
            return await db.QueryAsync<Invoice>(sql,
                new { Status = status, Search = $"%{search}%" });
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();

            var sql = @"
                SELECT i.*, p.FullName AS PatientName
                FROM Invoices i
                JOIN Patients p ON i.PatientId = p.Id
                WHERE i.Id = @Id";

            var invoice = await db.QueryFirstOrDefaultAsync<Invoice>(sql, new { Id = id });
            if (invoice == null) return null;

            invoice.Items = (await db.QueryAsync<InvoiceItem>(
                "SELECT * FROM InvoiceItems WHERE InvoiceId = @Id",
                new { Id = id })).ToList();

            return invoice;
        }

        public async Task<int> CreateAsync(InvoiceCreateDto dto)
        {
            using var db = _context.CreateConnection();

            // Auto invoice number
            var count = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Invoices");
            var invoiceNumber = $"INV-{(count + 1):D3}";

            // Calculate totals
            var subtotal = dto.Items.Sum(i => i.Quantity * i.Price);
            var grandTotal = subtotal - dto.Discount + (subtotal * dto.Tax / 100);

            var invoiceId = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Invoices
                    (InvoiceNumber, PatientId, AppointmentId,
                     TotalAmount, Discount, Tax, GrandTotal,
                     Status, PaymentMethod, Notes)
                VALUES
                    (@InvoiceNumber, @PatientId, @AppointmentId,
                     @TotalAmount, @Discount, @Tax, @GrandTotal,
                     @Status, @PaymentMethod, @Notes);
                SELECT SCOPE_IDENTITY();",
                new
                {
                    InvoiceNumber = invoiceNumber,
                    dto.PatientId,
                    dto.AppointmentId,
                    TotalAmount = subtotal,
                    dto.Discount,
                    dto.Tax,
                    GrandTotal = grandTotal,
                    dto.Status,
                    dto.PaymentMethod,
                    dto.Notes
                });

            // Insert items
            foreach (var item in dto.Items)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO InvoiceItems (InvoiceId, ItemName, Quantity, Price, Total)
                    VALUES (@InvoiceId, @ItemName, @Quantity, @Price, @Total)",
                    new
                    {
                        InvoiceId = invoiceId,
                        item.ItemName,
                        item.Quantity,
                        item.Price,
                        Total = item.Quantity * item.Price
                    });
            }

            return invoiceId;
        }

        public async Task<bool> UpdateAsync(InvoiceUpdateDto dto)
        {
            using var db = _context.CreateConnection();

            var subtotal = dto.Items.Sum(i => i.Quantity * i.Price);
            var grandTotal = subtotal - dto.Discount + (subtotal * dto.Tax / 100);

            await db.ExecuteAsync(@"
                UPDATE Invoices SET
                    PatientId=@PatientId, Status=@Status,
                    PaymentMethod=@PaymentMethod, Discount=@Discount,
                    Tax=@Tax, TotalAmount=@TotalAmount,
                    GrandTotal=@GrandTotal, Notes=@Notes
                WHERE Id=@Id",
                new
                {
                    dto.Id,
                    dto.PatientId,
                    dto.Status,
                    dto.PaymentMethod,
                    dto.Discount,
                    dto.Tax,
                    TotalAmount = subtotal,
                    GrandTotal = grandTotal,
                    dto.Notes
                });

            // Items refresh
            await db.ExecuteAsync(
                "DELETE FROM InvoiceItems WHERE InvoiceId=@Id", new { dto.Id });

            foreach (var item in dto.Items)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO InvoiceItems (InvoiceId, ItemName, Quantity, Price, Total)
                    VALUES (@InvoiceId, @ItemName, @Quantity, @Price, @Total)",
                    new
                    {
                        InvoiceId = dto.Id,
                        item.ItemName,
                        item.Quantity,
                        item.Price,
                        Total = item.Quantity * item.Price
                    });
            }
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, InvoiceStatusDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(@"
                UPDATE Invoices SET Status=@Status, PaymentMethod=@PaymentMethod
                WHERE Id=@Id",
                new { Id = id, dto.Status, dto.PaymentMethod }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "DELETE FROM Invoices WHERE Id=@Id", new { Id = id }) > 0;
        }
    }
}