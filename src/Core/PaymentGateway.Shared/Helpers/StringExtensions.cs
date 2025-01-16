using System.Numerics;

namespace PaymentGateway.Shared.Helpers;

public static class StringExtensions
{
    /// <summary>
    /// Method to extract the last 4 digits of the payment card. Throws if the string is not a number,
    /// or if the number of characters is less than 4. 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    public static int GetLastFourDigits(this string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 4)
        {
            throw new ArgumentException("Input must be at least 4 characters long", nameof(input));
        }


        if (!BigInteger.TryParse(input, out _))
        {
            throw new FormatException("The last 4 characters are not numeric.");
        }

        string lastFourDigits = input[^4..];

        if (!int.TryParse(lastFourDigits, out int result))
        {
            throw new FormatException("The last 4 characters are not numeric.");
        }

        return result;
    }
}