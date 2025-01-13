using PaymentGateway.Shared.Models.Controller.Responses;
using PaymentGateway.Shared.Models.DTO;

namespace PaymentGateway.Application.Contracts.Services;

public interface IPaymentService
{
    Task<CreatePaymentResponseDto> CreatePayment(CreatePaymentRequestDto request);
    Task<GetPaymentResponse?> GetPayment(Guid paymentId);
}