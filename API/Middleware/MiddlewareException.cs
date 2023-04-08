using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware
{
    public class MiddlewareException
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiddlewareException> _logger;
        private readonly IHostEnvironment _env;
        public MiddlewareException(RequestDelegate next, ILogger<MiddlewareException> logger, IHostEnvironment env)
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
            catch(Exception ex) {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                ApiException error = _env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

                string json = JsonSerializer.Serialize(error);

                await context.Response.WriteAsync(json);
            }
        }
    }
}