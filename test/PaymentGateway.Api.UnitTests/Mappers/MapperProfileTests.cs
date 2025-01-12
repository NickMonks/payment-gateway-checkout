using AutoMapper;

using PaymentGateway.Api.ApiClient.Models.Request;
using PaymentGateway.Api.ApiClient.Models.Response;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Profiles;

namespace PaymentGateway.Api.Tests.Mappers;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void PostPaymentRequest_To_PostPaymentApiRequest_ShouldMapCorrectly()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Act
        var result = _mapper.Map<PostPaymentApiRequest>(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.CardNumber, result.CardNumber);
        Assert.Equal("12/2025", result.ExpiryDate);
        Assert.Equal(request.Cvv, result.Cvv);
    }

    [Theory]
    [InlineData(true, PaymentStatus.Authorized)]
    [InlineData(false, PaymentStatus.Declined)]
    public void PostPaymentApiResponse_To_PaymentStatus_ShouldMapCorrectly(bool authorized, PaymentStatus expectedStatus)
    {
        // Arrange
        var apiResponse = new PostPaymentApiResponse
        {
            Authorized = authorized,
            AuthorizationCode = "1234-5678-9012"
        };

        // Act
        var result = _mapper.Map<PaymentStatus>(apiResponse);

        // Assert
        Assert.Equal(expectedStatus, result);
    }
}