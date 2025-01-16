using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Models;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Domain.Entities;

public class Payment
{
    [Key]
    public Guid PaymentId { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public int CardNumberFourDigits { get; set; }

    public string ExpirationMonth { get; set; }

    public string ExpirationYear { get; set; }

    public Currency Currency { get; set; }

    [Required]
    public int Amount { get; set; }
}