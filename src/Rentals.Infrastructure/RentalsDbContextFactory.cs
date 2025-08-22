using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Rentals.Infrastructure;

public class RentalsDbContextFactory : IDesignTimeDbContextFactory<RentalsDbContext>
{
    public RentalsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RentalsDbContext>();

        // NO ACCESS
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=rentals;Username=postgres;Password=postgres");

        return new RentalsDbContext(optionsBuilder.Options);
    }
}