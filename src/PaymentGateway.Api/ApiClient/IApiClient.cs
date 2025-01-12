using PaymentGateway.Api.ApiClient.Models.Request;
using PaymentGateway.Api.ApiClient.Models.Response;

namespace PaymentGateway.Api.ApiClient;

public interface IApiClient
{
    Task<PostPaymentApiResponse> CreatePaymentAsync(PostPaymentApiRequest request);
}
