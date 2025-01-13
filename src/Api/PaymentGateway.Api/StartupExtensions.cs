using PaymentGateway.Application;
using PaymentGateway.Infrastructure;
using PaymentGateway.Persistence;

namespace PaymentGateway.Api;

public static class StartupExtensions
{
    public static IServiceCollection AddStartupServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistenceServices(configuration);
        services.AddApiClientServices(configuration);
        services.AddApplicationServices(configuration);
        
        return services;
    }
    
}