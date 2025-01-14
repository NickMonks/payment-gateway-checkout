using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using PaymentGateway.Api.Settings;
using PaymentGateway.Application;
using PaymentGateway.Infrastructure;
using PaymentGateway.Persistence;

namespace PaymentGateway.Api;

public static class StartupExtensions
{
    public static void AddStartupServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistenceServices(configuration);
        services.AddApiClientServices(configuration);
        services.AddApplicationServices(configuration);
    }

    public static void AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var observabilitySettings = configuration
            .GetSection(nameof(ObservabilitySettings))
            .Get<ObservabilitySettings>() ?? throw new NullReferenceException();
        
        services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .AddSource(observabilitySettings.ServiceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName: observabilitySettings.ServiceName, 
                            serviceVersion: observabilitySettings.ServiceVersion
                        ))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(observabilitySettings.JaegerExporterUri);
                })
                .AddConsoleExporter();
        });

        services.AddSingleton(TracerProvider.Default.GetTracer(observabilitySettings.ServiceName));
    }
    
}