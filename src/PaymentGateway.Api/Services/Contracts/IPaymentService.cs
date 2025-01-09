using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Contracts;

public interface IPaymentService
{
    Task<PostPaymentResponse> CreatePayment(PostPaymentRequest payment);
    Task<GetPaymentResponse> GetPayment(Guid paymentId);
}