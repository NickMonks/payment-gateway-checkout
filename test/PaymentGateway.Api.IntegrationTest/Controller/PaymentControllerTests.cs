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
    
    [Theory]
    [InlineData("01","2024")]
    [InlineData("00","2025")]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenExpiredCard(string expiryMonth, string expiryYear)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = int.Parse(expiryMonth),
            ExpiryYear = int.Parse(expiryYear),
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", paymentRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenInvalidAmount(int amount)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 04,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = amount,
            Cvv = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", paymentRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Theory]
    [InlineData("1234")]
    [InlineData("12")]
    [InlineData("abc")]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenInvalidCvv(string cvv)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 04,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            Cvv = cvv
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", paymentRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    
    [Theory]
    [InlineData("123456")]
    [InlineData("22224053432488**")]
    [InlineData("dummy-data")]
    public async Task CreatePayment_ShouldReturn400BadRequest_WhenInvalidCardNumber(string cardNumber)
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = 04,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Payments", paymentRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
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

    [Fact]
    public async Task CreatePayment_ShouldReturnRejectedPaymentAndStoreInDatabase()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "2222405343248113",
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
        Assert.Equal(8113, paymentResponse.CardNumberLastFour);
        Assert.Equal(PaymentStatus.Rejected.ToString(), paymentResponse.Status);

        await using var dbContext = new PaymentsDbContext(_testEnvironment.CreateDbContextOptions());
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentResponse.Id);
        Assert.NotNull(payment);
        Assert.Equal(paymentResponse.Id, payment.PaymentId);
    }

    [Fact]
    public async Task GetPayment_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        
    }
    
    [Fact]
    public async Task GetPayment_ShouldReturnOkPayment_WhenPaymentExists()
    {
        
    }
}
