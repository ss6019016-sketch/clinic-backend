using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http;
using System.Text;
using System.Text.Json;
using clinic.Services;

namespace clinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class WhatsAppController : ControllerBase
    {
        private readonly WhatsAppSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public WhatsAppController(IOptions<WhatsAppSettings> settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// TEMPORARY DEBUG ENDPOINT - sends Meta's default pre-approved "hello_world"
        /// template (no parameters) directly, bypassing the appointment reminder logic,
        /// just to verify the AccessToken / PhoneNumberId connection works end-to-end.
        /// Phone must be international format without '+' or leading zero (e.g. 923222351675).
        /// Safe to delete once real templates are approved.
        /// </summary>
        [HttpPost("test-whatsapp")]
        public async Task<IActionResult> TestWhatsApp([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(new { message = "Query param 'phone' is required, e.g. ?phone=923222351675" });

            if (string.IsNullOrWhiteSpace(_settings.AccessToken) || string.IsNullOrWhiteSpace(_settings.PhoneNumberId))
                return BadRequest(new { message = "WhatsAppSettings.AccessToken or PhoneNumberId is empty in appsettings.json" });

            var url = $"https://graph.facebook.com/{_settings.ApiVersion}/{_settings.PhoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = phone,
                type = "template",
                template = new
                {
                    name = "hello_world",
                    language = new { code = "en_US" }
                }
            };

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_settings.AccessToken}");

            try
            {
                var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, new
                {
                    success = response.IsSuccessStatusCode,
                    statusCode = (int)response.StatusCode,
                    metaResponse = body
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Request to Meta threw an exception", error = ex.Message });
            }
        }
    }
}