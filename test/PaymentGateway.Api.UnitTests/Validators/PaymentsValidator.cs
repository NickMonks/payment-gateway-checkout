using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests.Validators;

public class PaymentsValidatorTests
{
    private readonly PaymentsValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456789012")]
    [InlineData("12345678901234567890")] 
    [InlineData("1234abcd5678efgh")]
    public void Validate_ShouldFail_WhenCardNumberIsInvalid(string cardNumber)
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    public void Validate_ShouldFail_WhenExpiryMonthIsOutOfRange(int month)
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = month,
            ExpiryYear = DateTime.Now.Year + 1,
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryMonth");
    }

    [Fact]
    public void Validate_ShouldFail_WhenExpiryMonthIsInThePast()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = DateTime.Now.Month - 1,
            ExpiryYear = DateTime.Now.Year,
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpiryMonth");
    }
    
    [Fact]
    public void Validate_ShouldFail_WhenExpiryYearIsInThePast()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = DateTime.Now.Month,
            ExpiryYear = DateTime.Now.Year - 1,
            Amount = 100,
            Currency = "USD",
            Cvv = "123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Expiry date cannot be in the past");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_ShouldFail_WhenAmountIsZeroOrNegative(int amount)
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Amount = amount,
            Currency = "USD",
            Cvv = "123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("USB")]
    [InlineData("ABCD")]
    [InlineData("123")]
    public void Validate_ShouldFail_WhenCurrencyIsInvalid(string currency)
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Amount = 100,
            Currency = currency,
            Cvv = "123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency");
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("12A")]
    public void Validate_ShouldFail_WhenCvvIsInvalid(string cvv)
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Amount = 100,
            Currency = "USD",
            Cvv = cvv
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv");
    }
}