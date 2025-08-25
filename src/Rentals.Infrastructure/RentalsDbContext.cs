using Microsoft.EntityFrameworkCore;
using Rentals.Domain.Drivers;
using Rentals.Domain.Users;
using Rentals.Infrastructure.Entities;

namespace Rentals.Infrastructure;

public class RentalsDbContext : DbContext
{
    public RentalsDbContext(DbContextOptions<RentalsDbContext> options)
        : base(options)
    {
    }

    public DbSet<DeliveryDriver> DeliveryDrivers { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Admin> Admins { get; set; } = null!;
    public DbSet<Motorcycle> Motorcycles { get; set; } = null!;
    public DbSet<RentalPlan> RentalPlans { get; set; } = null!;
    public DbSet<Rental> Rentals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentalsDbContext).Assembly);
    }
}