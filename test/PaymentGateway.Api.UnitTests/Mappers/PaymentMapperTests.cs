using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.Mappers;

public class PaymentMapperTests
{
    [Fact]
    public void ToPayment_ShouldMapCorrectly_FromPostPaymentResponse()
    {
        // Arrange
        var response = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            CardNumberLastFour = 123,
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100
        };

        var status = PaymentStatus.Authorized;

        // Act
        var result = response.ToPayment(status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(response.Id, result.PaymentId);
        Assert.Equal(status, result.PaymentStatus);
        Assert.Equal(response.CardNumberLastFour, result.CardNumberFourDigits);
        Assert.Equal(response.ExpiryMonth.ToString("D2"), result.ExpirationMonth);
        Assert.Equal(response.ExpiryYear.ToString(), result.ExpirationYear);
        Assert.Equal(Currency.USD, result.Currency);
        Assert.Equal(response.Amount, result.Amount);
    }

    [Fact]
    public void ToPayment_ShouldThrowException_WhenCurrencyIsInvalid_FromPostPaymentResponse()
    {
        // Arrange
        var response = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            CardNumberLastFour = 123,
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Currency = "INVALID", // Invalid currency
            Amount = 100
        };

        var status = PaymentStatus.Authorized;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => response.ToPayment(status));
    }

    [Fact]
    public void ToPayment_ShouldMapCorrectly_FromPaymentEntity()
    {
        // Arrange
        var paymentEntity = new Payment
        {
            PaymentId = Guid.NewGuid(),
            PaymentStatus = PaymentStatus.Authorized,
            CardNumberFourDigits = 123,
            ExpirationMonth = "12",
            ExpirationYear = "2025",
            Currency = Currency.USD,
            Amount = 100
        };

        // Act
        var result = paymentEntity.TogGetPaymentResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paymentEntity.PaymentId, result.Id);
        Assert.Equal(paymentEntity.PaymentStatus.ToString(), result.Status);
        Assert.Equal(paymentEntity.CardNumberFourDigits, result.CardNumberLastFour);
        Assert.Equal(paymentEntity.ExpirationMonth, result.ExpiryMonth);
        Assert.Equal(paymentEntity.ExpirationYear, result.ExpiryYear);
        Assert.Equal(paymentEntity.Currency.ToString(), result.Currency);
        Assert.Equal(paymentEntity.Amount, result.Amount);
    }

    [Fact]
    public void ToPayment_ShouldMapCurrencyCorrectly_FromPostPaymentResponse()
    {
        // Arrange
        var response = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized.ToString(),
            CardNumberLastFour = 578,
            ExpiryMonth = 5,
            ExpiryYear = 2030,
            Currency = "GBP", // Currency test
            Amount = 250
        };

        var status = PaymentStatus.Authorized;

        // Act
        var result = response.ToPayment(status);

        // Assert
        Assert.Equal(Currency.GBP, result.Currency);
    }

    [Fact]
    public void ToPayment_ShouldThrowException_WhenPaymentEntityIsNull()
    {
        // Arrange
        Payment? paymentEntity = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => paymentEntity!.TogGetPaymentResponse());
    }

    [Fact]
    public void ToPayment_ShouldThrowException_WhenPostPaymentResponseIsNull()
    {
        // Arrange
        PostPaymentResponse? response = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => response!.ToPayment(PaymentStatus.Authorized));
    }
}