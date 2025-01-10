using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Contracts;

public interface IPaymentService
{
    Task<PostPaymentResponse> CreatePayment(PostPaymentRequest request);
    Task<GetPaymentResponse?> GetPayment(Guid paymentId);
}