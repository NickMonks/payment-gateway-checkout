using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    Task<Payment?> CreatePaymentAsync(Payment? payment);
    Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
}