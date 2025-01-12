using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Application.Contracts.Services;

public interface IPaymentService
{
    Task<PostPaymentResponse> CreatePayment(PostPaymentRequest request);
    Task<GetPaymentResponse?> GetPayment(Guid paymentId);
}