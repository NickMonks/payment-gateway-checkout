using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Persistence.Persistence.Contracts;

public interface IPaymentsRepository
{
    Task<Payment?> CreatePaymentAsync(Payment? payment);
    Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
}