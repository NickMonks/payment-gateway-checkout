using PaymentGateway.Api.Contracts;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    
    public Task<PostPaymentResponse> CreatePayment(PostPaymentRequest payment)
    {
        throw new NotImplementedException();
    }

    public Task<GetPaymentResponse> GetPayment(Guid paymentId)
    {
        throw new NotImplementedException();
    }
}