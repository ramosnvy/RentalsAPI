using Microsoft.EntityFrameworkCore;
using Rentals.Domain.Drivers;

namespace Rentals.Infrastructure;

public class RentalsDbContext : DbContext
{
    public RentalsDbContext(DbContextOptions<RentalsDbContext> options)
        : base(options)
    {
    }

    public DbSet<DeliveryDriver> DeliveryDrivers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentalsDbContext).Assembly);
    }
}