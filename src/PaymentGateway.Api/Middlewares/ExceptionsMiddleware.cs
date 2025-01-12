using System.Net;
using System.Text.Json;

namespace PaymentGateway.Api.Middlewares;

public class ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        
        if (exception is HttpRequestException httpRequestException)
        {
            var httpStatusCode = (HttpStatusCode)httpRequestException.StatusCode!;

            statusCode = httpStatusCode switch
            {
                HttpStatusCode.Unauthorized => StatusCodes.Status401Unauthorized,
                HttpStatusCode.Forbidden => StatusCodes.Status403Forbidden,
                HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
                HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
            message = httpRequestException.Message;
        }
        else
        {
            // Fallback
            statusCode = StatusCodes.Status500InternalServerError;
            message = "An unexpected error occurred.";
        }

        context.Response.StatusCode = statusCode;
        var response = new { error = message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
