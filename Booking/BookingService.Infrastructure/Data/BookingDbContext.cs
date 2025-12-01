using BookingService.Domain.Entities;
using BookingService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // تعيين الـKey
        modelBuilder.Entity<Appointment>(b =>
        {
            b.HasKey(a => a.Id);

            // تهيئة الـValueObjects كملكية مملوكة (Owned)
            b.OwnsOne(a => a.PatientId, p =>
            {
                p.Property(v => v.Value).HasColumnName("PatientId");
            });

            b.OwnsOne(a => a.DentistId, d =>
            {
                d.Property(v => v.Value).HasColumnName("DentistId");
            });

            b.OwnsOne(a => a.Slot, s =>
            {
                s.Property(v => v.Start).HasColumnName("StartTime");
                s.Property(v => v.End).HasColumnName("EndTime");
            });

            b.Property(a => a.Status)
                .HasConversion<string>()
                .HasColumnName("Status");

            b.ToTable("Appointments");
        });
    }
}
