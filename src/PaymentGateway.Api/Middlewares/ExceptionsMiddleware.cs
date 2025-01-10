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

        if (exception is HttpRequestException httpRequestException)
        {
            if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
            {
                throw new BadRequestException("The request was invalid or cannot be served.");
            }
            else if (context.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                throw new NotFoundException("The requested resource was not found.");
            }
            else if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
            {
                throw new InternalServerErrorException("An internal server error occurred.");
            }
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred.";
            }
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            message = exception.Message;
        }

        context.Response.StatusCode = statusCode;
        var response = new { error = message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

}