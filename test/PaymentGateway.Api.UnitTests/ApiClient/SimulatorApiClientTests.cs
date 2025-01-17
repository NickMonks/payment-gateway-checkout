using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using PaymentGateway.Application.Contracts.ApiClient;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Infrastructure.ApiClient;
using PaymentGateway.Infrastructure.Handlers;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.ApiClient.Response;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PaymentGateway.Api.Tests.ApiClient;

public class SimulatorApiClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly SimulatorApiClient _apiClient;

    public SimulatorApiClientTests()
    {
        _server = WireMockServer.Start();
        var handlerLogger = new LoggerFactory().CreateLogger<ApiExceptionHandler>();
        var apiExceptionHandler = new ApiExceptionHandler(handlerLogger)
        {
            InnerHandler = new HttpClientHandler()
        };

        var httpClient = new HttpClient(apiExceptionHandler) { BaseAddress = new Uri(_server.Url!) };

        var loggerMock = new LoggerFactory().CreateLogger<SimulatorApiClient>();
        _apiClient = new SimulatorApiClient(httpClient, loggerMock);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldSendRequestAndDeserializeResponse_WhenValidRequest()
    {
        // Arrange
        var request = new PostPaymentApiRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2025",
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        var expectedResponse = new PostPaymentApiResponse
        {
            Authorized = true,
            AuthorizationCode = "1234-5678-9012"
        };

        //WireMock with valid JSON response
        _server
            .Given(Request.Create()
                .WithPath("/payments")
                .UsingPost()
                .WithBody(JsonSerializer.Serialize(request)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(expectedResponse)));

        // Act
        var result = await _apiClient.CreatePaymentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Authorized);
        Assert.Equal(expectedResponse.AuthorizationCode, result.AuthorizationCode);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldThrowException_WhenResponseIsEmpty()
    {
        // Arrange
        var request = new PostPaymentApiRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2025",
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Configure WireMock to respond with an empty body
        _server
            .Given(Request.Create()
                .WithPath("/payments")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(string.Empty));

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _apiClient.CreatePaymentAsync(request));
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldThrowJsonException_WhenResponseIsInvalidJson()
    {
        // Arrange
        var request = new PostPaymentApiRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2025",
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Wiremock with invalid json format - we should properly handle this error 
        _server
            .Given(Request.Create()
                .WithPath("/payments")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{ invalid-json }"));

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _apiClient.CreatePaymentAsync(request));
    }

    [Theory]
    [InlineData(400, HttpStatusCode.BadRequest)]
    [InlineData(401, HttpStatusCode.Unauthorized)]
    [InlineData(403, HttpStatusCode.Forbidden)]
    [InlineData(422, HttpStatusCode.UnprocessableEntity)]
    public async Task CreatePaymentAsync_ShouldThrowPaymentDeclinedException_WhenStatusCode(int statusCodeNumber, HttpStatusCode httpStatusCode)
    {
        // Arrange
        var request = new PostPaymentApiRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2025",
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        //Setup invalid request
        _server
            .Given(Request.Create()
                .WithPath("/payments")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCodeNumber)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"error\": \"Invalid request\"}"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PaymentDeclinedException>(() => _apiClient.CreatePaymentAsync(request));

        Assert.Equal(httpStatusCode, exception.StatusCode);
    }

    [Theory]
    [InlineData(408, HttpStatusCode.RequestTimeout)]
    [InlineData(429, HttpStatusCode.TooManyRequests)]
    public async Task CreatePaymentAsync_ShouldThrowHttpException_WhenStatusCode(int statusCodeNumber, HttpStatusCode httpStatusCode)
    {
        // Arrange
        var request = new PostPaymentApiRequest
        {
            CardNumber = "1234567890123456",
            ExpiryDate = "12/2025",
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        //Setup invalid request
        _server
            .Given(Request.Create()
                .WithPath("/payments")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCodeNumber)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"error\": \"Invalid request\"}"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _apiClient.CreatePaymentAsync(request));

        Assert.Equal(httpStatusCode, exception.StatusCode);
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}