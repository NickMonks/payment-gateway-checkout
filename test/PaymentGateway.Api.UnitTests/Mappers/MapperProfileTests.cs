using AutoMapper;

using PaymentGateway.Application.Profiles;
using PaymentGateway.Domain.ValueObjects;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.ApiClient.Response;
using PaymentGateway.Shared.Models.Controller.Responses;
using PaymentGateway.Shared.Models.DTO;

namespace PaymentGateway.Api.Tests.Mappers;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        config.AssertConfigurationIsValid(); 
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Map_CreatePaymentRequestDto_To_PostPaymentApiRequest_Success()
    {
        // Arrange
        var source = new CreatePaymentRequestDto
        {
            CardNumber = "4111111111111111", 
            ExpiryMonth = 12, 
            ExpiryYear = 2025, 
            Cvv = "123",
            Amount = 100,
            Currency = "USD",
        };

        // Act
        var result = _mapper.Map<PostPaymentApiRequest>(source);

        // Assert
        Assert.Equal(source.CardNumber, result.CardNumber);
        Assert.Equal("12/2025", result.ExpiryDate);
        Assert.Equal("123", result.Cvv);
    }

    [Fact]
    public void Map_PostPaymentApiResponse_To_PaymentStatus_Success()
    {
        // Arrange
        var authorizedResponse = new PostPaymentApiResponse
        {
            Authorized = true,
            AuthorizationCode = "1234-abcd-56789"
        };
        var declinedResponse = new PostPaymentApiResponse
        {
            Authorized = false,
            AuthorizationCode = "1234-abcd-56789"
        };

        // Act
        var authorizedResult = _mapper.Map<PaymentStatus>(authorizedResponse);
        var declinedResult = _mapper.Map<PaymentStatus>(declinedResponse);

        // Assert
        Assert.Equal(PaymentStatus.Authorized, authorizedResult);
        Assert.Equal(PaymentStatus.Declined, declinedResult);
    }

    [Fact]
    public void Map_PostPaymentRequest_To_CreatePaymentRequestDto_Success()
    {
        // Arrange
        var source = new CreatePaymentRequestDto
        {
            CardNumber = "4111111111111111", 
            ExpiryMonth = 12, 
            ExpiryYear = 2025, 
            Cvv = "123",
            Amount = 100,
            Currency = "USD",
        };

        // Act
        var result = _mapper.Map<CreatePaymentRequestDto>(source);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Map_CreatePaymentResponseDto_To_PostPaymentResponse_Success()
    {
        // Arrange
        var source = new CreatePaymentResponseDto
        {
            // Populate with test data as required
        };

        // Act
        var result = _mapper.Map<PostPaymentResponse>(source);

        // Assert
        Assert.NotNull(result);
    }
}