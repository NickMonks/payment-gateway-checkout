using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services;

public class PaymentsDbContext :DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment?> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Payment>().HasIndex(u => u.PaymentId).IsUnique();
    }
}