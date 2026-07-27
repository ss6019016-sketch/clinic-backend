using Dapper;
using clinic.Data;
using clinic.DTOs.Prescription;
using clinic.Models;
using clinic.Repositories.Interfaces;

namespace clinic.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly DapperContext _context;
        private readonly IMedicineRepository _medicineRepo;
        private readonly INotificationRepository _notificationRepo;

        public PrescriptionRepository(
            DapperContext context,
            IMedicineRepository medicineRepo,
            INotificationRepository notificationRepo)
        {
            _context = context;
            _medicineRepo = medicineRepo;
            _notificationRepo = notificationRepo;
        }

        public async Task<PagedResult<Prescription>> GetAllAsync(string? search, int page, int pageSize)
        {
            using var db = _context.CreateConnection();
            var whereClause = "WHERE pr.IsDeleted = 0";

            if (!string.IsNullOrEmpty(search))
                whereClause += @" AND (p.FullName LIKE @Search
                           OR d.FullName LIKE @Search
                           OR pr.Diagnosis LIKE @Search)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Prescriptions pr
                JOIN Patients p ON pr.PatientId = p.Id
                JOIN Doctors d  ON pr.DoctorId  = d.Id
                {whereClause}";
            var dataSql = $@"
                SELECT pr.*, p.FullName AS PatientName, d.FullName AS DoctorName
                FROM Prescriptions pr
                JOIN Patients p ON pr.PatientId = p.Id
                JOIN Doctors d  ON pr.DoctorId  = d.Id
                {whereClause}
                ORDER BY pr.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Search = $"%{search}%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await db.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await db.QueryAsync<Prescription>(dataSql, parameters);

            return new PagedResult<Prescription>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Prescription?> GetByIdAsync(int id)
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT pr.*, p.FullName AS PatientName, d.FullName AS DoctorName
                FROM Prescriptions pr
                JOIN Patients p ON pr.PatientId = p.Id
                JOIN Doctors d  ON pr.DoctorId  = d.Id
                WHERE pr.Id = @Id";

            var rx = await db.QueryFirstOrDefaultAsync<Prescription>(sql, new { Id = id });
            if (rx == null) return null;

            rx.Medicines = (await db.QueryAsync<PrescriptionItem>(
                "SELECT * FROM PrescriptionItems WHERE PrescriptionId=@Id",
                new { Id = id })).ToList();

            return rx;
        }

        public async Task<IEnumerable<Prescription>> GetByPatientAsync(int patientId)
        {
            using var db = _context.CreateConnection();
            var rxList = (await db.QueryAsync<Prescription>(@"
                SELECT pr.*, d.FullName AS DoctorName
                FROM Prescriptions pr
                JOIN Doctors d ON pr.DoctorId = d.Id
                WHERE pr.PatientId = @PatientId AND pr.IsDeleted = 0
                ORDER BY pr.CreatedAt DESC",
                new { PatientId = patientId })).ToList();

            foreach (var rx in rxList)
            {
                rx.Medicines = (await db.QueryAsync<PrescriptionItem>(
                    "SELECT * FROM PrescriptionItems WHERE PrescriptionId=@Id",
                    new { Id = rx.Id })).ToList();
            }
            return rxList;
        }

        public async Task<int> CreateAsync(PrescriptionCreateDto dto)
        {
            using var db = _context.CreateConnection();

            var rxId = await db.ExecuteScalarAsync<int>(@"
                INSERT INTO Prescriptions
                    (PatientId, DoctorId, AppointmentId,
                     Diagnosis, Notes, FollowUpDate)
                VALUES
                    (@PatientId, @DoctorId, @AppointmentId,
                     @Diagnosis, @Notes, @FollowUpDate);
                SELECT SCOPE_IDENTITY();",
                new
                {
                    dto.PatientId,
                    dto.DoctorId,
                    dto.AppointmentId,
                    dto.Diagnosis,
                    dto.Notes,
                    FollowUpDate = string.IsNullOrEmpty(dto.FollowUpDate)
                        ? (DateTime?)null
                        : DateTime.Parse(dto.FollowUpDate)
                });

            foreach (var med in dto.Medicines)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO PrescriptionItems
                        (PrescriptionId, MedicineId, Quantity, MedicineName, Dosage,
                         Frequency, Duration, Instructions)
                    VALUES
                        (@PrescriptionId, @MedicineId, @Quantity, @MedicineName, @Dosage,
                         @Frequency, @Duration, @Instructions)",
                    new
                    {
                        PrescriptionId = rxId,
                        med.MedicineId,
                        med.Quantity,
                        med.MedicineName,
                        med.Dosage,
                        med.Frequency,
                        med.Duration,
                        med.Instructions
                    });

                if (med.MedicineId.HasValue && med.MedicineId.Value > 0)
                {
                    await _medicineRepo.AdjustStockAsync(
                        med.MedicineId.Value, -med.Quantity, "Prescription", rxId,
                        $"Deducted for Prescription #{rxId}");

                    var medicine = await _medicineRepo.GetByIdAsync(med.MedicineId.Value);
                    if (medicine != null && medicine.IsLowStock)
                    {
                        await _notificationRepo.CreateAsync(new Notification
                        {
                            Title = "Low Stock Alert",
                            Message = $"{medicine.Name} stock is low ({medicine.StockQuantity} left, reorder level {medicine.ReorderLevel}).",
                            Type = "LowStock",
                            Link = "/pharmacy/medicines"
                        });
                    }
                }
            }
            return rxId;
        }

        public async Task<bool> UpdateAsync(PrescriptionUpdateDto dto)
        {
            using var db = _context.CreateConnection();

            await db.ExecuteAsync(@"
                UPDATE Prescriptions SET
                    PatientId=@PatientId, DoctorId=@DoctorId,
                    Diagnosis=@Diagnosis, Notes=@Notes,
                    FollowUpDate=@FollowUpDate
                WHERE Id=@Id",
                new
                {
                    dto.Id,
                    dto.PatientId,
                    dto.DoctorId,
                    dto.Diagnosis,
                    dto.Notes,
                    FollowUpDate = string.IsNullOrEmpty(dto.FollowUpDate)
                        ? (DateTime?)null
                        : DateTime.Parse(dto.FollowUpDate)
                });

            // Medicines refresh
            await db.ExecuteAsync(
                "DELETE FROM PrescriptionItems WHERE PrescriptionId=@Id",
                new { dto.Id });

            foreach (var med in dto.Medicines)
            {
                await db.ExecuteAsync(@"
                    INSERT INTO PrescriptionItems
                        (PrescriptionId, MedicineId, Quantity, MedicineName, Dosage,
                         Frequency, Duration, Instructions)
                    VALUES
                        (@PrescriptionId, @MedicineId, @Quantity, @MedicineName, @Dosage,
                         @Frequency, @Duration, @Instructions)",
                    new
                    {
                        PrescriptionId = dto.Id,
                        med.MedicineId,
                        med.Quantity,
                        med.MedicineName,
                        med.Dosage,
                        med.Frequency,
                        med.Duration,
                        med.Instructions
                    });
            }
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            var result = await db.ExecuteAsync(
                "UPDATE Prescriptions SET IsDeleted = 1, DeletedAt = GETDATE() WHERE Id=@Id AND IsDeleted = 0",
                new { Id = id }) > 0;

            if (result)
            {
                var items = await db.QueryAsync<PrescriptionItem>(
                    "SELECT * FROM PrescriptionItems WHERE PrescriptionId=@Id", new { Id = id });
                foreach (var item in items.Where(i => i.MedicineId.HasValue))
                {
                    await _medicineRepo.AdjustStockAsync(
                        item.MedicineId!.Value, item.Quantity, "Adjustment", id,
                        $"Restocked - Prescription #{id} deleted");
                }
            }
            return result;
        }

        public async Task<IEnumerable<Prescription>> GetTrashAsync()
        {
            using var db = _context.CreateConnection();
            var sql = @"
                SELECT pr.*, p.FullName AS PatientName, d.FullName AS DoctorName
                FROM Prescriptions pr
                JOIN Patients p ON pr.PatientId = p.Id
                JOIN Doctors d  ON pr.DoctorId  = d.Id
                WHERE pr.IsDeleted = 1
                ORDER BY pr.DeletedAt DESC";
            return await db.QueryAsync<Prescription>(sql);
        }

        public async Task<bool> RestoreAsync(int id)
        {
            using var db = _context.CreateConnection();
            var result = await db.ExecuteAsync(
                "UPDATE Prescriptions SET IsDeleted = 0, DeletedAt = NULL WHERE Id=@Id AND IsDeleted = 1",
                new { Id = id }) > 0;

            if (result)
            {
                var items = await db.QueryAsync<PrescriptionItem>(
                    "SELECT * FROM PrescriptionItems WHERE PrescriptionId=@Id", new { Id = id });
                foreach (var item in items.Where(i => i.MedicineId.HasValue))
                {
                    await _medicineRepo.AdjustStockAsync(
                        item.MedicineId!.Value, -item.Quantity, "Adjustment", id,
                        $"Re-deducted - Prescription #{id} restored");
                }
            }
            return result;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            using var db = _context.CreateConnection();
            await db.ExecuteAsync(
                "DELETE FROM PrescriptionItems WHERE PrescriptionId=@Id", new { Id = id });
            return await db.ExecuteAsync(
                "DELETE FROM Prescriptions WHERE Id=@Id AND IsDeleted = 1",
                new { Id = id }) > 0;
        }
    }
}