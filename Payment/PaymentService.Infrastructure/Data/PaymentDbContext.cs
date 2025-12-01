using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b =>
        {
            b.HasKey(p => p.Id);

            // ✅ نربط ValueObject (Money) كـ Owned type
            b.OwnsOne(p => p.Total, mv =>
            {
                mv.Property(v => v.Amount).HasColumnName("Amount");
                mv.Property(v => v.Currency).HasColumnName("Currency");
            });

            // ✅ نحول Enum إلى string في قاعدة البيانات
            b.Property(p => p.Status)
                .HasConversion<string>()
                .HasColumnName("Status");

            b.ToTable("Payments");
        });
    }
}
