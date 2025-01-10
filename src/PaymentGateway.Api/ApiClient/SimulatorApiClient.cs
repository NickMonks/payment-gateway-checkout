using System.Text;
using System.Text.Json;

using PaymentGateway.Api.ApiClient.Models.Request;
using PaymentGateway.Api.ApiClient.Models.Response;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.ApiClient;

public class SimulatorApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SimulatorApiClient> _logger;

    public SimulatorApiClient(HttpClient httpClient, ILogger<SimulatorApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PostPaymentApiResponse> CreatePaymentAsync(PostPaymentApiRequest request)
    {
        _logger.LogInformation("Creating payment");
        var bodyRequest = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );
        
        var response = await _httpClient.PostAsync("/payments",bodyRequest);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        var postPaymentResponse = JsonSerializer.Deserialize<PostPaymentApiResponse>(responseString);
        if (postPaymentResponse == null)
        {
            throw new InvalidOperationException("Deserialization failed: JSON returned null.");
        }
        
        return postPaymentResponse;
    }
}