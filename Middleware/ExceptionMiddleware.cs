using System.Net;
using System.Text.Json;

namespace clinic.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var payload = _env.IsDevelopment()
                    ? new { message = ex.Message, stackTrace = ex.StackTrace }
                    : new { message = "Something went wrong. Please try again later.", stackTrace = (string?)null };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }
}