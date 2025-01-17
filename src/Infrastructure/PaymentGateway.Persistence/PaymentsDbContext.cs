using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Models;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Persistence;

public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresEnum<PaymentStatus>();
        modelBuilder.HasPostgresEnum<Currency>();
        
        modelBuilder.Entity<Payment>()
            .HasIndex(u => u.PaymentId)
            .IsUnique();
        
        modelBuilder.Entity<Payment>()
            .Property(p => p.Currency)
            .HasConversion<string>();
        
        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentStatus)
            .HasConversion<string>();
    }
}