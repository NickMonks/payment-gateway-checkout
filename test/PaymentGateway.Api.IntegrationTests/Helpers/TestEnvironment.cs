using DotNet.Testcontainers.Builders;

using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Persistence;

using Testcontainers.PostgreSql;

using IContainer = DotNet.Testcontainers.Containers.IContainer;
namespace PaymentGateway.Api.IntegrationTests.Helpers;

/// <summary>
/// Fixture class to setup and run our test containers in the integration test. We use it to spin up
/// two containers, which are our main dependencies: the postgres db and the bank simulator. 
/// </summary>
public class TestEnvironment : IAsyncLifetime
{
    private PostgreSqlContainer PostgresContainer { get; set; }
    private IContainer BankSimulatorContainer { get; set; }
    public string SimulatorBaseUrl { get; set; }
    public string PostgresConnectionString { get; set; }
    private string BankSimulatorContainerName { get; set; } = new string("bank-simulator-" + Guid.NewGuid());

    public TestEnvironment(string simulatorBaseUrl, string postgresConnectionString)
    {
        SimulatorBaseUrl = simulatorBaseUrl;
        PostgresConnectionString = postgresConnectionString;
        PostgresContainer = new PostgreSqlBuilder()
            .WithDatabase("payments-db")
            .WithUsername("admin")
            .WithPassword("password")
            .Build();

        //TODO: This might be problematic to run into the tests. Fix it
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
            .WithCommand("--configfile", "/imposters/bank_simulator.ejs")
            .WithBindMount(impostersPath, "/imposters")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start the containers
        await PostgresContainer.StartAsync();
        await BankSimulatorContainer.StartAsync();
        PostgresConnectionString = PostgresContainer.GetConnectionString();

        //Ensure we run the migrations after initialization
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

            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }

    }
}