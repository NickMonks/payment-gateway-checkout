using System.ComponentModel;

using DotNet.Testcontainers.Builders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using PaymentGateway.Api.Persistence;

using Testcontainers.PostgreSql;

using IContainer = DotNet.Testcontainers.Containers.IContainer;

public class TestEnvironment : IAsyncLifetime
{
    public PostgreSqlContainer PostgresContainer { get; set; }
    public IContainer BankSimulatorContainer { get; set; }
    public string SimulatorBaseUrl { get; set; }
    public string PostgresConnectionString { get; set; }
    
    public string BankSimulatorContainerName { get; set; } = new string("bank-simulator-" + Guid.NewGuid());
    
    public TestEnvironment()
    {
        // Set up PostgreSQL container
        PostgresContainer = new PostgreSqlBuilder()
            .WithDatabase("payments-db")
            .WithUsername("admin")
            .WithPassword("password")
            .Build();
        
        var impostersPath = Path.GetFullPath("../../../Helpers/imposters");

        if (!Directory.Exists(impostersPath))
        {
            throw new DirectoryNotFoundException($"The imposters directory was not found at: {impostersPath}");
        }
        
        BankSimulatorContainer = new ContainerBuilder()
            .WithImage("bbyars/mountebank:2.8.1")
            .WithName(BankSimulatorContainerName)
            .WithPortBinding(8080, true)
            .WithPortBinding(2525, true)
            .WithCommand("--configfile","/imposters/bank_simulator.ejs")
            .WithBindMount(impostersPath, "/imposters")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start the containers
        await PostgresContainer.StartAsync();
        await BankSimulatorContainer.StartAsync();
        PostgresConnectionString = PostgresContainer.GetConnectionString();
        // Run migrations using the application's DbContext
        await RunMigrationsAsync(PostgresConnectionString);
        var simulatorPort = BankSimulatorContainer.GetMappedPublicPort(8080);
        SimulatorBaseUrl = $"http://localhost:{simulatorPort}";
        
    }

    public async Task DisposeAsync()
    {
        await PostgresContainer.DisposeAsync();
        await BankSimulatorContainer.DisposeAsync();
    }

    public DbContextOptions<PaymentsDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql(PostgresConnectionString)
            .Options;
    }
    
    private async Task RunMigrationsAsync(string connectionString)
    {
        try
        {
            var options = new DbContextOptionsBuilder<PaymentsDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var context = new PaymentsDbContext(options);

            // Apply migrations
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        
    } 
}