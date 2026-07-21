using System.Text;
using System.Text.Json;
using clinic.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace clinic.Services
{
    public class WhatsAppSettings
    {
        public string AccessToken { get; set; } = string.Empty;
        public string PhoneNumberId { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = "v20.0";
        // Name of the WhatsApp message template you approved in Meta Business Manager.
        // Create a template with 4 body variables: {{1}} patient name, {{2}} doctor name,
        // {{3}} date, {{4}} time. Example template body:
        // "Hi {{1}}, this is a reminder for your appointment with Dr. {{2}} on {{3}} at {{4}}."
        public string TemplateName { get; set; } = "appointment_reminder";
        public string LanguageCode { get; set; } = "en_US";
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            HttpClient http,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppService> logger)
        {
            _http = http;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAppointmentReminderAsync(
            string phone, string patientName, string doctorName,
            DateTime date, string time)
        {
            if (string.IsNullOrWhiteSpace(_settings.AccessToken) ||
                string.IsNullOrWhiteSpace(_settings.PhoneNumberId))
            {
                _logger.LogWarning("WhatsApp settings not configured — reminder not sent.");
                return false;
            }

            var url = $"https://graph.facebook.com/{_settings.ApiVersion}/{_settings.PhoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = phone,
                type = "template",
                template = new
                {
                    name = _settings.TemplateName,
                    language = new { code = _settings.LanguageCode },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new object[]
                            {
                                new { type = "text", text = patientName },
                                new { type = "text", text = doctorName },
                                new { type = "text", text = date.ToString("dd MMM yyyy") },
                                new { type = "text", text = time }
                            }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_settings.AccessToken}");

            try
            {
                var response = await _http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("WhatsApp send failed for {Phone}: {Body}", phone, body);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp send threw an exception for {Phone}", phone);
                return false;
            }
        }
    }
}