using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using PaymentGateway.Api.IntegrationTests.Helpers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Persistence;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.ValueObjects;
using PaymentGateway.Infrastructure.ApiClient;
using PaymentGateway.Shared.Models.Controller.Requests;
using PaymentGateway.Shared.Models.Controller.Responses;

namespace PaymentGateway.Api.IntegrationTests.Controller;

using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

public class PaymentsControllerTests :
    IClassFixture<WebApplicationFactory<Program>>, IClassFixture<TestEnvironment>
{
    private readonly HttpClient _client;
    private readonly TestEnvironment _testEnvironment;
    private readonly IMemoryCache _memoryCache;

    public PaymentsControllerTests(
        WebApplicationFactory<Program> factory,
        TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;

        var sharedMemoryCache = new MemoryCache(new MemoryCacheOptions());
        _memoryCache = sharedMemoryCache;

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

                services.AddSingleton<IMemoryCache>(sharedMemoryCache);

            });
        }).CreateClient();
    }

    [Theory]
    [InlineData("01", "2024")]
    [InlineData("00", "2025")]
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
    public async Task CreatePayment_ShouldReturn200_WhenAuthorizedPayment_AndStoreInDatabase()
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
    public async Task CreatePayment_ShouldReturn200_WhenDeclinedPayment_AndStoreInDatabase()
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
        // Arrange
        var randomPaymentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Payments/{randomPaymentId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_ShouldReturn200Ok_WhenPaymentExists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentEntity = new Payment
        {
            PaymentId = paymentId,
            CardNumberFourDigits = 8877,
            ExpirationMonth = "04",
            ExpirationYear = "2025",
            Currency = Currency.GBP,
            Amount = 100,
            PaymentStatus = PaymentStatus.Authorized
        };

        await using (var dbContext = new PaymentsDbContext(_testEnvironment.CreateDbContextOptions()))
        {
            dbContext.Payments.Add(paymentEntity);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/api/Payments/{paymentId}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentId, paymentResponse.Id);
        Assert.Equal(paymentEntity.CardNumberFourDigits, paymentResponse.CardNumberLastFour);
        Assert.Equal(paymentEntity.Currency.ToString(), paymentResponse.Currency);
        Assert.Equal(paymentEntity.Amount, paymentResponse.Amount);
        Assert.Equal(paymentEntity.PaymentStatus.ToString(), paymentResponse.Status);
    }

    [Fact]
    public async Task GetPayment_ShouldReturnOkPayment_Cached()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentResponse = new GetPaymentResponse
        {
            Id = paymentId,
            CardNumberLastFour = 8877,
            ExpiryMonth = "04",
            ExpiryYear = "2025",
            Currency = "GBP",
            Amount = 100,
            Status = PaymentStatus.Authorized.ToString()
        };

        _memoryCache.Set(paymentId, paymentResponse);

        // Act
        var response = await _client.GetAsync($"/api/Payments/{paymentId}");
        var cachedPaymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cachedPaymentResponse);
        Assert.Equal(paymentResponse.Id, cachedPaymentResponse.Id);
        Assert.Equal(paymentResponse.CardNumberLastFour, cachedPaymentResponse.CardNumberLastFour);
        Assert.Equal(paymentResponse.Currency, cachedPaymentResponse.Currency);
        Assert.Equal(paymentResponse.Amount, cachedPaymentResponse.Amount);
        Assert.Equal(paymentResponse.Status, cachedPaymentResponse.Status);
    }
}