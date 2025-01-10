using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Persistence.Repositories;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly PaymentsDbContext _context;

    public PaymentsRepository(PaymentsDbContext context)
    {
        _context = context;
    }
    
    public async Task<Payment?> CreatePaymentAsync(Payment? payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
    {
        return await _context.Payments.FirstOrDefaultAsync(u => u.PaymentId == paymentId);
    }
}