using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Settings;
using PaymentGateway.Application.Contracts.ApiClient;
using PaymentGateway.Infrastructure.ApiClient;
using PaymentGateway.Infrastructure.Handlers;

using Polly;
using Polly.Extensions.Http;

namespace PaymentGateway.Infrastructure;

public static class ApiClientServiceRegistration
{
    public static void AddApiClientServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IApiClient, SimulatorApiClient>(nameof(SimulatorApiClient), client =>
            {
                var simulatorApiSettings = configuration
                    .GetSection(nameof(SimulatorApiSettings))
                    .Get<SimulatorApiSettings>() ?? throw new NullReferenceException();

                client.BaseAddress = new Uri(simulatorApiSettings.BaseUri);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddHttpMessageHandler<ApiExceptionHandler>();
        services.AddTransient<ApiExceptionHandler>();
    }
    
    /// <summary>
    /// Retry policy for our HTTP Client. We will retry on 5xx, 408 (Timeout) errors.
    /// The current configuration has 5 retires with an exponential backoff strategy.
    /// </summary>
    /// <returns></returns>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() 
            .WaitAndRetryAsync(
                retryCount: 5, 
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Retrying... Attempt {retryAttempt}. Waiting {timespan}.");
                });
    }

}