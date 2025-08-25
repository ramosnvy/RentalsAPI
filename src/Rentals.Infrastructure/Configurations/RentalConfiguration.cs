using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentals.Infrastructure.Entities;

namespace Rentals.Infrastructure.Configurations;

public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.ToTable("rentals");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.DeliveryDriverId)
            .IsRequired();

        builder.Property(r => r.MotorcycleId)
            .IsRequired();

        builder.Property(r => r.RentalPlanId)
            .IsRequired();

        builder.Property(r => r.StartDate)
            .IsRequired();

        builder.Property(r => r.ExpectedEndDate)
            .IsRequired();

        builder.Property(r => r.ActualEndDate);

        builder.Property(r => r.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(r => r.FinalAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        // Índices
        builder.HasIndex(r => r.DeliveryDriverId);
        builder.HasIndex(r => r.MotorcycleId);
        builder.HasIndex(r => r.RentalPlanId);
    }
}
