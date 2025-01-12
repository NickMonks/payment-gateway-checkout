using PaymentGateway.Api.Helpers;

namespace PaymentGateway.Api.Tests.Helpers;

public class StringExtensionsTests
{
    [Fact]
    public void GetLastFourDigits_ShouldReturnCorrectValue_WhenInputIsWithinRange()
    {
        // Arrange
        var input = "2222405343248877";

        // Act
        var result = input.GetLastFourDigits();

        // Assert
        Assert.Equal(8877, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    public void GetLastFourDigits_ShouldThrowArgumentException_WhenInputInvalid(string? input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => input!.GetLastFourDigits());
    }

    [Theory]
    [InlineData("123456abcd")]
    [InlineData("*234567890")]
    public void GetLastFourDigits_ShouldThrowFormatException_WhenLNotAllDigits(string input)
    {
        Assert.Throws<FormatException>(() => input.GetLastFourDigits());
    }

    [Theory]
    [InlineData("00001234", 1234)]
    [InlineData("1234", 1234)]
    [InlineData("56781234", 1234)]
    public void GetLastFourDigits_ShouldReturnCorrectValue_WhenDigits(string input, int expected)
    {
        // Act
        var result = input.GetLastFourDigits();

        // Assert
        Assert.Equal(expected, result);
    }
}