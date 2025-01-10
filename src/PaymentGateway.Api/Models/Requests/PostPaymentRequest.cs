using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentRequest
{
    [Required]
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; }
    [Required]
    [JsonPropertyName("expiry_month")]
    public int ExpiryMonth { get; set; }
    [Required]
    [JsonPropertyName("expiry_year")]
    public int ExpiryYear { get; set; }
    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
    [Required]
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    [Required]
    [JsonPropertyName("cvv")]
    public string Cvv { get; set; }
}