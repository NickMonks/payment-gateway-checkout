using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Handlers;
using PaymentGateway.Api.Settings;
using PaymentGateway.Application.Contracts.ApiClient;
using PaymentGateway.Infrastructure.ApiClient;

namespace PaymentGateway.Infrastructure;

public static class ApiClientServiceRegistration
{
    public static IServiceCollection AddApiClientServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IApiClient, SimulatorApiClient>(nameof(SimulatorApiClient), client =>
            {
                var simulatorApiSettings = configuration
                    .GetSection(nameof(SimulatorApiSettings))
                    .Get<SimulatorApiSettings>() ?? throw new NullReferenceException();

                client.BaseAddress = new Uri(simulatorApiSettings.BaseUri);
            })
            .AddHttpMessageHandler<ApiExceptionHandler>();
        services.AddTransient<ApiExceptionHandler>();
        return services;
    }

}