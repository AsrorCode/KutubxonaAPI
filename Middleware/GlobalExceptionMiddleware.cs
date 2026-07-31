using System.Net;
using System.Text.Json;

namespace KutubxonaAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
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
            _logger.LogError(ex, "So'rov ishlanishida xato: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Ruxsat yo'q"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Topilmadi"),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "Ichki server xatosi")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            message,
            statusCode = (int)statusCode,
            path = context.Request.Path.ToString(),
            stackTrace = _env.IsDevelopment() ? ex.StackTrace : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}