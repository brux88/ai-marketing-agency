using System.Net;
using System.Text.Json;

namespace AiMarketingAgency.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExType} - {ExMsg}", ex.GetType().FullName, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Unwrap inner exceptions from AI/HTTP calls
        var rootException = exception.InnerException ?? exception;

        var (statusCode, message) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Access denied"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            InvalidOperationException => (HttpStatusCode.Conflict, exception.Message),
            _ when exception.GetType().Name == "HttpOperationException"
                => (HttpStatusCode.BadGateway, $"Errore dal provider AI: {exception.Message}"),
            _ when exception.Message.Contains("API Key", StringComparison.OrdinalIgnoreCase)
                => (HttpStatusCode.BadGateway, "Chiave API del provider AI non valida. Verifica la chiave nelle impostazioni."),
            _ => (HttpStatusCode.InternalServerError, "Si e verificato un errore interno. Riprova.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var env = context.RequestServices.GetService<IHostEnvironment>();
        var detail = env?.IsDevelopment() == true ? exception.ToString() : null;
        var response = JsonSerializer.Serialize(new { error = message, statusCode = (int)statusCode, detail });
        await context.Response.WriteAsync(response);
    }
}
