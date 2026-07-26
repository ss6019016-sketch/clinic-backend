using clinic.DTOs.LabReport;
using clinic.Models;

namespace clinic.Repositories.Interfaces
{
    public interface ILabReportRepository
    {
        Task<IEnumerable<LabReportListDto>> GetByPatientIdAsync(int patientId);
        Task<LabReport?> GetByIdAsync(int id);   // full record, including FileData — for download/view
        Task<int> CreateAsync(LabReportCreateDto dto, string fileName, string fileType, string fileData);
        Task<bool> DeleteAsync(int id);
    }
}