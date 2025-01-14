using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Contracts.Persistence;

public interface IPaymentsRepository
{
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
}