// using System.Net;
// using System.Text.Json;
//
// using PaymentGateway.Api.Exceptions;
//
// namespace PaymentGateway.Api.Middlewares;
//
// public class ExceptionsMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<ExceptionsMiddleware> _logger;
//
//     public ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger)
//     {
//         _next = next;
//         _logger = logger;
//     }
//
//     public async Task InvokeAsync(HttpContext context)
//     {
//         try
//         {
//             await _next(context);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "An unhandled exception occurred.");
//             await HandleExceptionAsync(context, ex);
//         }
//     }
//
//     private async Task HandleExceptionAsync(HttpContext context, Exception exception)
//     {
//         context.Response.ContentType = "application/json";
//
//         int statusCode;
//         string message;
//         
//         if (exception is ClientApiException clientApiException)
//         {
//             throw ;
//         }
//         
//         if (exception is HttpRequestException httpRequestException)
//         {
//             var httpStatusCode = (HttpStatusCode)httpRequestException.StatusCode!;
//
//             statusCode = httpStatusCode switch
//             {
//                 HttpStatusCode.Unauthorized => StatusCodes.Status401Unauthorized,
//                 HttpStatusCode.Forbidden => StatusCodes.Status403Forbidden,
//                 HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
//                 HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
//                 _ => StatusCodes.Status500InternalServerError
//             };
//             message = httpRequestException.Message;
//         }
//         else
//         {
//             // Fallback for other exceptions
//             statusCode = StatusCodes.Status500InternalServerError;
//             message = "An unexpected error occurred.";
//         }
//
//         context.Response.StatusCode = statusCode;
//         var response = new { error = message };
//         await context.Response.WriteAsync(JsonSerializer.Serialize(response));
//     }
// }
