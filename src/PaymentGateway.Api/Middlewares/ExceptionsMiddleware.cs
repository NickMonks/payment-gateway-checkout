using System.Net;
using System.Text.Json;

namespace PaymentGateway.Api.Exceptions;

public class ExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionsMiddleware> _logger;

    public ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        HttpStatusCode? httpRequestException;

        if (exception is HttpRequestException)
        {
            httpRequestException = (exception as HttpRequestException).StatusCode;
            statusCode = httpRequestException switch
            {
                HttpStatusCode.Unauthorized => StatusCodes.Status401Unauthorized,
                HttpStatusCode.Forbidden => StatusCodes.Status403Forbidden,
                HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
                HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
                HttpStatusCode.InternalServerError => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };

            message = exception.Message;
        }
        else
        {
            // For unexpected exceptions, return a generic message
            statusCode = StatusCodes.Status500InternalServerError;
            message = "An unexpected error occurred.";
        }

        context.Response.StatusCode = statusCode;
        var response = new { error = message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

}