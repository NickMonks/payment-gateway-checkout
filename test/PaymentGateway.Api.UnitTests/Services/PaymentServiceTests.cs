using System.Net;

using AutoMapper;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Api.Models;
using PaymentGateway.Application.Contracts.ApiClient;
using PaymentGateway.Application.Contracts.Persistence;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Profiles;
using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.ValueObjects;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.ApiClient.Response;
using PaymentGateway.Shared.Models.Controller.Responses;
using PaymentGateway.Shared.Models.DTO;

namespace PaymentGateway.Api.Tests.Services;

public class PaymentServiceTests
{
    private readonly PaymentService _service;
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly Mock<IPaymentsRepository> _repositoryMock;
    private readonly IMemoryCache _memoryCache;

    public PaymentServiceTests()
    {
        Mock<ILogger<PaymentService>> loggerMock = new();
        _apiClientMock = new Mock<IApiClient>();
        _repositoryMock = new Mock<IPaymentsRepository>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        var mapper = configuration.CreateMapper();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());


        _service = new PaymentService(
            loggerMock.Object,
            _apiClientMock.Object,
            mapper,
            _repositoryMock.Object,
            _memoryCache
        );
    }

    [Fact]
    public async Task CreatePayment_ShouldReturnExpectedResponse()
    {
        // Arrange
        var request = new CreatePaymentRequestDto()
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 04,
            ExpiryYear = 2025,
            Amount = 100,
            Currency = "GBP",
            Cvv = "123"
        };

        var paymentEntity = new Payment();
        var apiResponse = new PostPaymentApiResponse
        {
            Authorized = true,
            AuthorizationCode = "1234567789-abcd-1234",
        };

        _apiClientMock.Setup(c => c.CreatePaymentAsync(It.IsAny<PostPaymentApiRequest>())).ReturnsAsync(apiResponse);
        _repositoryMock.Setup(r => r.CreatePaymentAsync(It.IsAny<Payment>())).ReturnsAsync(paymentEntity);

        // Act
        var result = await _service.CreatePayment(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.ExpiryYear, result.ExpiryYear);
        Assert.Equal(request.ExpiryMonth, result.ExpiryMonth);
        Assert.Equal(PaymentStatus.Authorized.ToString(), result.Status);
        Assert.Equal(request.Currency, result.Currency);
        Assert.Equal(request.Amount, result.Amount);
        Assert.Equal(request.CardNumber[^4..], result.CardNumberLastFour.ToString());

        var lastFourDigits = request.CardNumber[^4..];
        _apiClientMock.Verify(r => r.CreatePaymentAsync(It.IsAny<PostPaymentApiRequest>()), Times.Once);
        _repositoryMock.Verify(r => r.CreatePaymentAsync(It.IsAny<Payment>()), Times.Once);
        _repositoryMock.Verify(r => r.CreatePaymentAsync(It.Is<Payment>(p =>
            p.PaymentStatus == PaymentStatus.Authorized &&
            p.Amount == request.Amount &&
            p.Currency == Currency.GBP &&
            p.CardNumberFourDigits.ToString() == lastFourDigits &&
            p.ExpirationMonth == request.ExpiryMonth.ToString("D2") &&
            p.ExpirationYear == request.ExpiryYear.ToString()
        )), Times.Once);
    }

    //TODO: Review test with checkout
    [Fact]
    public async Task CreatePayment_ShouldStoreRejectedPayment_WhenClientExceptionOccurs()
    {
        // Arrange
        var request = new CreatePaymentRequestDto()
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        var apiRequest = new PostPaymentApiRequest
        {
            CardNumber = null,
            ExpiryDate = null,
            Currency = null,
            Cvv = null,
            Amount = 10,
        };

        _apiClientMock
            .Setup(c => c.CreatePaymentAsync(It.IsAny<PostPaymentApiRequest>()))
            .ThrowsAsync(new PaymentRejectedException("Api call error", HttpStatusCode.BadRequest));

        // Mock repository to store rejected payment
        _repositoryMock.Setup(r => r.CreatePaymentAsync(It.IsAny<Payment>()))
            .ReturnsAsync(new Payment
            {
                PaymentId = Guid.NewGuid(),
                PaymentStatus = PaymentStatus.Rejected,
                Amount = request.Amount
            });

        // Act
        var result = await _service.CreatePayment(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected.ToString(), result.Status);
        _repositoryMock.Verify(r => r.CreatePaymentAsync(It.Is<Payment>(p =>
            p.PaymentStatus == PaymentStatus.Rejected)), Times.Once);
    }

    [Fact]
    public async Task GetPayment_ShouldReturnCachedPayment_WhenAvailable()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var getResponse = new GetPaymentResponse
        {
            Id = paymentId,
            Status = "Authorized",
            CardNumberLastFour = 345,
            Amount = 100,
            Currency = "USD"
        };

        _memoryCache.Set(paymentId, getResponse, TimeSpan.FromMinutes(10));

        // Act
        var result = await _service.GetPayment(paymentId);

        // Assert
        Assert.Equal(getResponse, result);
        _repositoryMock.Verify(r => r.GetPaymentByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetPayment_ShouldRetrievePaymentFromDatabase_WhenNotCached()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentEntity = new Payment
        {
            PaymentId = paymentId,
            PaymentStatus = PaymentStatus.Authorized,
            Amount = 100,
            CardNumberFourDigits = 345,
            ExpirationMonth = "12",
            ExpirationYear = "2025",
            Currency = Currency.USD
        };

        var getResponse = new GetPaymentResponse
        {
            Id = paymentId,
            Status = PaymentStatus.Authorized.ToString(),
            CardNumberLastFour = 345,
            Amount = 100,
            Currency = Currency.USD.ToString(),
            ExpiryMonth = "12",
            ExpiryYear = "2025",
        };

        _repositoryMock.Setup(r => r.GetPaymentByIdAsync(paymentId)).ReturnsAsync(paymentEntity);

        // Act
        var result = await _service.GetPayment(paymentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(getResponse.ExpiryYear, result?.ExpiryYear);
        Assert.Equal(getResponse.ExpiryMonth, result?.ExpiryMonth);
        Assert.Equal(getResponse.Status, result?.Status);
        Assert.Equal(getResponse.Currency, result?.Currency);
        Assert.Equal(getResponse.Amount, result?.Amount);
        Assert.Equal(getResponse.CardNumberLastFour, result?.CardNumberLastFour);
        _repositoryMock.Verify(r => r.GetPaymentByIdAsync(paymentId), Times.Once);
    }

    [Fact]
    public async Task GetPayment_ShouldReturnNull_WhenPaymentNotFoundInDatabase()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetPaymentByIdAsync(paymentId)).ReturnsAsync((Payment?)null);

        // Act
        var result = await _service.GetPayment(paymentId);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetPaymentByIdAsync(paymentId), Times.Once);
    }
}