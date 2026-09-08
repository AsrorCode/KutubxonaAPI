using System.Net;
using System.Text.Json;
using FluentValidation;
using KutubxonaAPI.Exceptions;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        string errorCode;
        object? errors = null;

        switch (ex)
        {
            case AppException appEx:
                statusCode = appEx.StatusCode;
                message = appEx.Message;
                errorCode = appEx.ErrorCode;
                _logger.LogWarning("App exception: {Code} {Message}", errorCode, message);
                break;

            case ValidationException valEx:
                statusCode = 400;
                message = "Validatsiya xatosi";
                errorCode = "VALIDATION_ERROR";
                errors = valEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                _logger.LogWarning("Validation: {Path}", context.Request.Path);
                break;

            case UnauthorizedAccessException:
                statusCode = 401;
                message = "Autentifikatsiya kerak";
                errorCode = "UNAUTHORIZED";
                break;

            case KeyNotFoundException:
                statusCode = 404;
                message = "Topilmadi";
                errorCode = "NOT_FOUND";
                break;

            case ArgumentException argEx:
                statusCode = 400;
                message = argEx.Message;
                errorCode = "BAD_ARGUMENT";
                break;

            case InvalidOperationException invEx:
                statusCode = 400;
                message = invEx.Message;
                errorCode = "INVALID_OPERATION";
                break;

            case OperationCanceledException:
                // Foydalanuvchi so'rovni tashladi — bu xato emas
                statusCode = 499; // "Client Closed Request"
                message = "So'rov bekor qilindi";
                errorCode = "CANCELLED";
                _logger.LogInformation("Request cancelled: {Path}", context.Request.Path);
                break;

            default:
                statusCode = 500;
                message = "Ichki server xatosi";
                errorCode = "INTERNAL_ERROR";
                _logger.LogError(ex, "Unhandled exception: {Path}", context.Request.Path);
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            message,
            errorCode,
            statusCode,
            path = context.Request.Path.ToString(),
            timestamp = DateTime.UtcNow,
            errors,
            stackTrace = _env.IsDevelopment() && statusCode == 500 ? ex.StackTrace : null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(json);
    }
}
