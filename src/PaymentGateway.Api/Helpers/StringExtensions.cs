namespace PaymentGateway.Api.Utility;

public static class StringExtensions
{
    public static int GetLastFourDigits(this string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 4)
        {
            throw new ArgumentException("Input must be at least 4 characters long", nameof(input));
        }

        string lastFourDigits = input[^4..]; 
        if (!int.TryParse(lastFourDigits, out int result))
        {
            throw new FormatException("The last 4 characters are not numeric.");
        }

        return result;
    }
}