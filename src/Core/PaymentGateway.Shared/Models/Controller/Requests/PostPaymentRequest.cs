using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PaymentGateway.Shared.Models.Controller.Requests;

public class PostPaymentRequest
{
    [Required]
    [JsonPropertyName("card_number")]
    public required string CardNumber { get; set; }
    [Required]
    [JsonPropertyName("expiry_month")]
    public required int ExpiryMonth { get; set; }
    [Required]
    [JsonPropertyName("expiry_year")]
    public required int ExpiryYear { get; set; }
    [Required]
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
    [Required]
    [JsonPropertyName("amount")]
    public required int Amount { get; set; }
    [Required]
    [JsonPropertyName("cvv")]
    public required string Cvv { get; set; }
}