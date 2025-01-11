using System.Net;
using System.Net.Http;
using System.Text.Json;
using PaymentGateway.Api.Exceptions;

namespace PaymentGateway.Api.Handlers;

public class ApiExceptionHandler : DelegatingHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HTTP request failed with status code {StatusCode}", response.StatusCode);

                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    throw new ClientApiException("Client error occurred during API call", response.StatusCode);
                }

                response.EnsureSuccessStatusCode();
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "An error occurred during the HTTP request.");
            throw;
        }
    }
}