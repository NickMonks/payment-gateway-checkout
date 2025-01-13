using System.Text.Json.Serialization;

namespace PaymentGateway.Shared.Models.ApiClient.Request;

public class PostPaymentApiRequest
{
    [JsonPropertyName("card_number")]
    public required string CardNumber { get; set; }
    
    [JsonPropertyName("expiry_date")]
    public required string ExpiryDate { get; set; }
    
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
    
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    
    [JsonPropertyName("cvv")]
    public required string Cvv { get; set; }
}