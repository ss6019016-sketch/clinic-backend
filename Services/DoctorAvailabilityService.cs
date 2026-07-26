using clinic.DTOs.DoctorAvailability;
using clinic.Models;
using clinic.Repositories.Interfaces;
using clinic.Services.Interfaces;
using System.Globalization;

namespace clinic.Services
{
    public class DoctorAvailabilityService : IDoctorAvailabilityService
    {
        private readonly IDoctorAvailabilityRepository _repo;
        public DoctorAvailabilityService(IDoctorAvailabilityRepository repo) => _repo = repo;

        public Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId)
            => _repo.GetByDoctorIdAsync(doctorId);

        public Task<int> CreateAsync(DoctorAvailabilityCreateDto dto)
        {
            var entity = new DoctorAvailability
            {
                DoctorId = dto.DoctorId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                SlotDurationMinutes = dto.SlotDurationMinutes,
                IsActive = dto.IsActive
            };
            return _repo.CreateAsync(entity);
        }

        public async Task<bool> UpdateAsync(int id, DoctorAvailabilityCreateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.DayOfWeek = dto.DayOfWeek;
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.SlotDurationMinutes = dto.SlotDurationMinutes;
            existing.IsActive = dto.IsActive;

            return await _repo.UpdateAsync(existing);
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var dayName = date.DayOfWeek.ToString(); // "Monday"
            var schedules = await _repo.GetByDoctorIdAsync(doctorId);
            var todaySchedule = schedules.Where(s => s.DayOfWeek == dayName).ToList();

            var bookedTimes = (await _repo.GetBookedTimesAsync(doctorId, date))
                                .Select(t => t.Trim())
                                .ToHashSet();

            var slots = new List<AvailableSlotDto>();

            foreach (var sch in todaySchedule)
            {
                var current = sch.StartTime;
                while (current.Add(TimeSpan.FromMinutes(sch.SlotDurationMinutes)) <= sch.EndTime)
                {
                    var timeStr = DateTime.Today.Add(current).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                    slots.Add(new AvailableSlotDto
                    {
                        Time = timeStr,
                        IsBooked = bookedTimes.Contains(timeStr)
                    });
                    current = current.Add(TimeSpan.FromMinutes(sch.SlotDurationMinutes));
                }
            }

            return slots;
        }

        public async Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, string time)
        {
            var bookedTimes = await _repo.GetBookedTimesAsync(doctorId, date);
            return !bookedTimes.Contains(time.Trim());
        }
    }
}