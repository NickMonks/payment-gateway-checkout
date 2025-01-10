using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Entities;

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