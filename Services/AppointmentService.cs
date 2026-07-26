using clinic.DTOs.Appointment;
using clinic.Models;
using clinic.Repositories.Interfaces;
using clinic.Services.Interfaces;

namespace clinic.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repo;
        private readonly IWhatsAppService _whatsApp;
        private readonly IDoctorAvailabilityService _availabilityService;

        public AppointmentService(
            IAppointmentRepository repo,
            IWhatsAppService whatsApp,
            IDoctorAvailabilityService availabilityService)
        {
            _repo = repo;
            _whatsApp = whatsApp;
            _availabilityService = availabilityService;
        }

        public Task<PagedResult<Appointment>> GetAllAsync(string? status, string? search, int page, int pageSize)
            => _repo.GetAllAsync(status, search, page, pageSize);

        public Task<Appointment?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public async Task<int> CreateAsync(AppointmentCreateDto dto)
        {
            var isAvailable = await _availabilityService.IsSlotAvailableAsync(
                dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime);

            if (!isAvailable)
                throw new InvalidOperationException("This slot is already booked. Please choose another time.");

            return await _repo.CreateAsync(dto);
        }

        public Task<bool> UpdateAsync(int id, AppointmentCreateDto dto)
        {
            var updateDto = new AppointmentUpdateDto
            {
                Id = id,
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                AppointmentTime = dto.AppointmentTime,
                Status = dto.Status,
                Reason = dto.Reason,
                Type = dto.Type,
                Notes = dto.Notes
            };
            return _repo.UpdateAsync(updateDto);
        }

        public Task<bool> UpdateStatusAsync(int id, string status)
            => _repo.UpdateStatusAsync(id, status);

        public Task<bool> DeleteAsync(int id)
            => _repo.DeleteAsync(id);

        public Task<IEnumerable<Appointment>> GetTrashAsync()
            => _repo.GetTrashAsync();

        public Task<bool> RestoreAsync(int id)
            => _repo.RestoreAsync(id);

        public Task<bool> HardDeleteAsync(int id)
            => _repo.HardDeleteAsync(id);

        public async Task<(bool success, string message)> SendReminderNowAsync(int id)
        {
            var appt = await _repo.GetByIdAsync(id);
            if (appt == null) return (false, "Appointment not found");

            if (string.IsNullOrWhiteSpace(appt.PatientPhone))
                return (false, "Patient has no phone number on file");

            var sent = await _whatsApp.SendAppointmentReminderAsync(
                appt.PatientPhone, appt.PatientName, appt.DoctorName,
                appt.AppointmentDate, appt.AppointmentTime);

            if (sent) await _repo.MarkReminderSentAsync(id);

            return sent
                ? (true, "Reminder sent successfully")
                : (false, "Failed to send reminder — check WhatsApp API configuration");
        }
    }
}