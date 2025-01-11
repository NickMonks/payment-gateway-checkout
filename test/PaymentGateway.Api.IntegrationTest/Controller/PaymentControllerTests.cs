using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using PaymentGateway.Api.ApiClient;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.IntegrationTest.Controller;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

public class PaymentsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<TestEnvironment>
{
    private readonly HttpClient _client;
    private readonly TestEnvironment _testEnvironment;

    public PaymentsControllerTests(WebApplicationFactory<Program> factory, TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<PaymentsDbContext>(options =>
                    options.UseNpgsql(testEnvironment.PostgresConnectionString));

                services.AddHttpClient<SimulatorApiClient>(client =>
                {
                    client.BaseAddress = new Uri(testEnvironment.SimulatorBaseUrl);
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task CreatePayment_ShouldReturnAuthorizedPaymentAndStoreInDatabase()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 04,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", paymentRequest);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(8877, paymentResponse.CardNumberLastFour);
        Assert.Equal(PaymentStatus.Authorized.ToString(), paymentResponse.Status);

        await using var dbContext = new PaymentsDbContext(_testEnvironment.CreateDbContextOptions());
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentResponse.Id);
        Assert.NotNull(payment);
        Assert.Equal(paymentResponse.Id, payment.PaymentId);
    }
    
    [Fact]
    public async Task CreatePayment_ShouldReturnDeclinedPaymentAndStoreInDatabase()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "2222405343248112",
            ExpiryMonth = 01,
            ExpiryYear = 2026,
            Currency = "USD",
            Amount = 60000,
            Cvv = "456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", paymentRequest);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(8112, paymentResponse.CardNumberLastFour);
        Assert.Equal(PaymentStatus.Declined.ToString(), paymentResponse.Status);

        await using var dbContext = new PaymentsDbContext(_testEnvironment.CreateDbContextOptions());
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentResponse.Id);
        Assert.NotNull(payment);
        Assert.Equal(paymentResponse.Id, payment.PaymentId);
    }
}
