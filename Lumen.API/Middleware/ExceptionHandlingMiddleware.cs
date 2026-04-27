using System.Net;
using System.Text.Json;

namespace Lumen.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,   ex.Message),
            KeyNotFoundException         => (HttpStatusCode.NotFound,       ex.Message),
            InvalidOperationException    => (HttpStatusCode.BadRequest,     ex.Message),
            _                            => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode  = (int)status;

        var body = JsonSerializer.Serialize(new { error = message });
        return ctx.Response.WriteAsync(body);
    }
}
