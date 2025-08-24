using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rentals.Domain.Drivers;

namespace Rentals.Infrastructure;

public class DeliveryDriverConfiguration : IEntityTypeConfiguration<DeliveryDriver>
{
    public void Configure(EntityTypeBuilder<DeliveryDriver> builder)
    {
        builder.ToTable("delivery_drivers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd(); 

        builder.Property(d => d.Identifier)
            .IsRequired();

        
        builder.Property(d => d.Identifier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(d => d.Cnpj, cnpj =>
        {
            cnpj.Property(c => c.Value)
                .HasColumnName("cnpj")
                .IsRequired()
                .HasMaxLength(14);
        });

        builder.OwnsOne(d => d.Cnh, cnh =>
        {
            cnh.Property(c => c.Number)
                .HasColumnName("cnh_number")
                .IsRequired();

            cnh.Property(c => c.CnhCategory)
                .HasColumnName("cnh_category")
                .IsRequired();
        });

        builder.OwnsOne(d => d.CnhImage, img =>
        {
            img.Property(i => i.BlobPath)
                .HasColumnName("cnh_image_file");

            img.Property(i => i.ContentType)
                .HasColumnName("cnh_image_type");
        });
    }
}