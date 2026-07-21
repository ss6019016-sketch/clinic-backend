using clinic.Repositories.Interfaces;
using clinic.Services.Interfaces;

namespace clinic.Services
{
    /// <summary>
    /// Runs continuously in the background. Once a day (around the configured hour),
    /// it looks at tomorrow's confirmed/pending appointments and sends a WhatsApp
    /// reminder for each one that hasn't already received one.
    /// </summary>
    public class AppointmentReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<AppointmentReminderBackgroundService> _logger;
        private const int RunHourUtc = 15; // ~ evening in PKT; adjust as needed

        public AppointmentReminderBackgroundService(
            IServiceProvider services,
            ILogger<AppointmentReminderBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.UtcNow.Hour == RunHourUtc)
                    {
                        await SendTomorrowsRemindersAsync();
                        // Sleep for an hour so we don't fire multiple times within the same hour
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending appointment reminders");
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task SendTomorrowsRemindersAsync()
        {
            using var scope = _services.CreateScope();
            var apptRepo = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
            var whatsApp = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();

            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            var appointments = await apptRepo.GetPendingRemindersAsync(tomorrow);

            foreach (var appt in appointments)
            {
                if (string.IsNullOrWhiteSpace(appt.PatientPhone)) continue;

                var sent = await whatsApp.SendAppointmentReminderAsync(
                    appt.PatientPhone, appt.PatientName, appt.DoctorName,
                    appt.AppointmentDate, appt.AppointmentTime);

                if (sent)
                {
                    await apptRepo.MarkReminderSentAsync(appt.Id);
                    _logger.LogInformation("Reminder sent for appointment {Id}", appt.Id);
                }
            }
        }
    }
}