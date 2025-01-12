using System.Text.Json.Serialization;

namespace PaymentGateway.Shared.Models.ApiClient.Response;

public class PostPaymentApiResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized  { get; set; }
    
    [JsonPropertyName("authorization_code")]
    public string AuthorizationCode { get; set; }
}