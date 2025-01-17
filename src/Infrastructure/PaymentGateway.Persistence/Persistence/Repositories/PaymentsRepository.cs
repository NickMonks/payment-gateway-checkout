using Microsoft.EntityFrameworkCore;

using PaymentGateway.Application.Contracts.Persistence;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Shared.Observability;

namespace PaymentGateway.Persistence.Persistence.Repositories;

public class PaymentsRepository(PaymentsDbContext context) : IPaymentsRepository
{
    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        using var activity = DiagnosticsConfig.Source.StartActivity(
            $"{nameof(PaymentsRepository)}.{nameof(CreatePaymentAsync)}"
            );
        
        activity?.SetPayment(payment);

        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
    {
        using var activity = DiagnosticsConfig.Source.StartActivity(
            $"{nameof(PaymentsRepository)}.{nameof(GetPaymentByIdAsync)}"
            );
        
        return await context.Payments.FirstOrDefaultAsync(u => u.PaymentId == paymentId);
    }
}