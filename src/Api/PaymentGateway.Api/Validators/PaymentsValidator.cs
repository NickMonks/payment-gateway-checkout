using FluentValidation;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Application;

namespace PaymentGateway.Api.Validators;

public class PaymentsValidator : AbstractValidator<PostPaymentRequest>
{
    public PaymentsValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .Must(card => HasDigits(card, 14,19))
            .WithMessage("Card number must be between 19 and 14 characters.");
        
        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between January and December (numeric)");
        
        RuleFor(x => new { x.ExpiryMonth, x.ExpiryYear })
            .Must(x => IsValidDate(x.ExpiryMonth, x.ExpiryYear))
            .WithMessage("Expiry date cannot be in the past");
        
        RuleFor(x => x.Amount)
            .NotEmpty().GreaterThan(0).WithMessage("Amount must be greater than zero");
        
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(cvv => cvv.ToString().Length == 3)
            .WithMessage("Currency should be 3 character long.")
            .Must(BeValidCurrency)
            .WithMessage("Currency is not valid.");
        
        RuleFor(x => x.Cvv)
            .NotEmpty()
            .Must(number => HasDigits(number, 3,3))
            .WithMessage("Card number must have at least 3 characters.");
    }
    
    
    private bool IsValidDate(int month, int year)
    {
        return year > DateTime.Now.Year || (year == DateTime.Now.Year && month >= DateTime.Now.Month);
    }

    private bool HasDigits(string number, int min, int max)
    {
        if (string.IsNullOrEmpty(number))
        {
            return false;
        }

        return number.All(char.IsDigit) && number.Length >= min && number.Length <= max;
    }

    private bool BeValidCurrency(string currency)
    {
        return Enum.IsDefined(typeof(Currency), currency);
    }
    
}