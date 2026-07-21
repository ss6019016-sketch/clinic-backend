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
        public AppointmentService(IAppointmentRepository repo, IWhatsAppService whatsApp)
        {
            _repo = repo;
            _whatsApp = whatsApp;
        }

        public Task<IEnumerable<Appointment>> GetAllAsync(string? status, string? search)
            => _repo.GetAllAsync(status, search);

        public Task<Appointment?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<int> CreateAsync(AppointmentCreateDto dto)
            => _repo.CreateAsync(dto);

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