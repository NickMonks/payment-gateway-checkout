using Microsoft.Extensions.Logging;

using PaymentGateway.Application.Exceptions;

namespace PaymentGateway.Infrastructure.Handlers;

public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("HTTP request failed with status code {StatusCode}", response.StatusCode);

                if (IsDeclined((int)response.StatusCode))
                {
                    throw new PaymentRejectedException("Client error occurred during API call - Payment Declined", response.StatusCode);
                }

                response.EnsureSuccessStatusCode();
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "An error occurred during the HTTP request.");
            throw;
        }
    }

    /// <summary>
    /// Returns true if the error code is considered due to a client malformed request:
    /// 400 Bad Request - Invalid or malformed request
    /// 401 Unathorized -  Missing or invalid authentication
    /// 403 Forbidden - Client lacks permission for the action
    /// 422 Unprocessable Entity - Semantic rules error
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    private bool IsDeclined(int statusCode)
    {
        return statusCode == 400 || statusCode == 403 || statusCode == 401 || statusCode == 422;
    }
}