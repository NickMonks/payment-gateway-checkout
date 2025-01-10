using System.Text.Json.Serialization;

namespace PaymentGateway.Api.ApiClient.Models.Response;

public class PostPaymentApiResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized  { get; set; }
    
    [JsonPropertyName("authorization_code")]
    public string AuthorizationCode { get; set; }
}