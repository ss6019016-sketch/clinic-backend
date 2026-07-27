using Dapper;
using clinic.Data;
using clinic.DTOs.Medicine;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly DapperContext _context;
        public MedicineRepository(DapperContext context) => _context = context;

        public async Task<PagedResult<Medicine>> GetAllAsync(string? search, int page, int pageSize)
        {
            using var db = _context.CreateConnection();
            var whereClause = "WHERE m.Status='Active'";
            if (!string.IsNullOrEmpty(search))
                whereClause += " AND (m.Name LIKE @Search OR m.GenericName LIKE @Search OR m.Category LIKE @Search)";

            var countSql = $"SELECT COUNT(*) FROM Medicines m {whereClause}";
            var dataSql = $@"
                SELECT m.*, v.Name AS VendorName
                FROM Medicines m
                LEFT JOIN Vendors v ON m.VendorId = v.Id
                {whereClause}
                ORDER BY m.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Search = $"%{search}%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await db.QueryAsync<Medicine>(dataSql, parameters);

            return new PagedResult<Medicine>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Medicine?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Medicine>(@"
                SELECT m.*, v.Name AS VendorName
                FROM Medicines m
                LEFT JOIN Vendors v ON m.VendorId = v.Id
                WHERE m.Id=@Id", new { Id = id });
        }

        public async Task<int> CreateAsync(MedicineCreateDto dto)
        {
            using var db = _context.CreateConnection();
            var id = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Medicines
                    (Name, GenericName, Category, Unit, StockQuantity, ReorderLevel,
                     PurchasePrice, SellingPrice, ExpiryDate, VendorId)
                VALUES
                    (@Name, @GenericName, @Category, @Unit, @StockQuantity, @ReorderLevel,
                     @PurchasePrice, @SellingPrice, @ExpiryDate, @VendorId);
                SELECT SCOPE_IDENTITY();", dto);

            if (dto.StockQuantity > 0)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO MedicineStockLogs (MedicineId, ChangeType, QuantityChange, Notes)
                    VALUES (@MedicineId, 'Purchase', @Qty, 'Initial stock')",
                    new { MedicineId = id, Qty = dto.StockQuantity });
            }
            return id;
        }

        public async Task<bool> UpdateAsync(MedicineUpdateDto dto)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(@"
                UPDATE Medicines SET
                    Name=@Name, GenericName=@GenericName, Category=@Category, Unit=@Unit,
                    ReorderLevel=@ReorderLevel, PurchasePrice=@PurchasePrice,
                    SellingPrice=@SellingPrice, ExpiryDate=@ExpiryDate, VendorId=@VendorId
                WHERE Id=@Id", dto) > 0;
            // Note: StockQuantity is NOT updated here directly — use AdjustStockAsync for stock changes
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Medicines SET Status='Inactive' WHERE Id=@Id AND Status='Active'",
                new { Id = id }) > 0;
        }

        public async Task<IEnumerable<Medicine>> GetLowStockAsync()
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<Medicine>(@"
                SELECT m.*, v.Name AS VendorName
                FROM Medicines m
                LEFT JOIN Vendors v ON m.VendorId = v.Id
                WHERE m.Status='Active' AND m.StockQuantity <= m.ReorderLevel
                ORDER BY m.StockQuantity ASC");
        }

        public async Task<int> GetLowStockCountAsync()
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Medicines WHERE Status='Active' AND StockQuantity <= ReorderLevel");
        }

        public async Task<bool> AdjustStockAsync(int medicineId, int quantityChange, string changeType, int? referenceId, string? notes)
        {
            using var db = _context.CreateConnection();

            var updated = await db.ExecuteAsync(@"
                UPDATE Medicines SET StockQuantity = StockQuantity + @Qty
                WHERE Id=@Id AND (StockQuantity + @Qty) >= 0",
                new { Id = medicineId, Qty = quantityChange }) > 0;

            if (updated)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO MedicineStockLogs (MedicineId, ChangeType, QuantityChange, ReferenceId, Notes)
                    VALUES (@MedicineId, @ChangeType, @Qty, @ReferenceId, @Notes)",
                    new { MedicineId = medicineId, ChangeType = changeType, Qty = quantityChange, ReferenceId = referenceId, Notes = notes });
            }
            return updated;
        }

        public async Task<IEnumerable<MedicineStockLog>> GetStockLogsAsync(int medicineId)
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<MedicineStockLog>(@"
                SELECT * FROM MedicineStockLogs WHERE MedicineId=@Id ORDER BY CreatedAt DESC",
                new { Id = medicineId });
        }

        public async Task<IEnumerable<Medicine>> GetTrashAsync()
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<Medicine>(
                "SELECT * FROM Medicines WHERE Status='Inactive' ORDER BY CreatedAt DESC");
        }

        public async Task<bool> RestoreAsync(int id)
        {
            using var db = _context.CreateConnection();
            return await db.ExecuteAsync(
                "UPDATE Medicines SET Status='Active' WHERE Id=@Id AND Status='Inactive'",
                new { Id = id }) > 0;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            await db.ExecuteAsync("DELETE FROM MedicineStockLogs WHERE MedicineId=@Id", new { Id = id });
            return await db.ExecuteAsync(
                "DELETE FROM Medicines WHERE Id=@Id AND Status='Inactive'",
                new { Id = id }) > 0;
        }
    }
}