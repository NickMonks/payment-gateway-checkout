using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.ApiClient.Response;

namespace PaymentGateway.Application.Contracts.ApiClient;

public interface IApiClient
{
    Task<PostPaymentApiResponse> CreatePaymentAsync(PostPaymentApiRequest request);
}
