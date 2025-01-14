using OpenTelemetry.Trace;

namespace PaymentGateway.Api.Middlewares;

public class OpenTelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Tracer _tracer;
    private readonly ILogger<OpenTelemetryMiddleware> _logger;

    public OpenTelemetryMiddleware(RequestDelegate next, Tracer tracer, ILogger<OpenTelemetryMiddleware> logger)
    {
        _next = next;
        _tracer = tracer;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var span = _tracer.StartActiveSpan("HTTP Request", SpanKind.Server);
        try
        {
            span.SetAttribute("http.method", context.Request.Method);
            span.SetAttribute("http.url", context.Request.Path);
            span.SetAttribute("http.query", context.Request.QueryString.ToString());

            await _next(context);

            span.SetAttribute("http.status_code", context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            span.SetStatus(Status.Error.WithDescription(ex.Message));

            throw;
        }
    }
}