

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Application.Mappers;

public static class PaymentMapper
{
    public static Payment ToPayment(this PostPaymentResponse response, PaymentStatus status)
    {
        return new Payment
        {
            PaymentId = response.Id,
            PaymentStatus = status,
            CardNumberFourDigits = response.CardNumberLastFour,
            ExpirationMonth = response.ExpiryMonth.ToString("D2"),
            ExpirationYear = response.ExpiryYear.ToString(),
            Currency = Enum.Parse<Currency>(response.Currency, true),
            Amount = response.Amount
        };
    }
    
    public static GetPaymentResponse TogGetPaymentResponse(this Payment response)
    {
        return new GetPaymentResponse
        {
            Id = response.PaymentId,
            Status = response.PaymentStatus.ToString(),
            CardNumberLastFour = response.CardNumberFourDigits,
            ExpiryMonth = response.ExpirationMonth,
            ExpiryYear = response.ExpirationYear,
            Currency = response.Currency.ToString(),
            Amount = response.Amount
        };
    }
}