using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using PaymentGateway.Application.Contracts.ApiClient;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.ApiClient.Response;

namespace PaymentGateway.Infrastructure.ApiClient;

public class SimulatorApiClient(HttpClient httpClient, ILogger<SimulatorApiClient> logger) : IApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<SimulatorApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<PostPaymentApiResponse> CreatePaymentAsync(PostPaymentApiRequest request)
    {
        _logger.LogInformation($"{nameof(CreatePaymentAsync)} - Beginning to create payment");

        var bodyContent = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/payments", bodyContent);
        var responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<PostPaymentApiResponse>(responseString)
               ?? throw new InvalidOperationException("Deserialization returned null");
    }
}