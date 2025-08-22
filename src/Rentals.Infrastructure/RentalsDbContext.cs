using Microsoft.EntityFrameworkCore;

namespace Rentals.Infrastructure
{
    public class RentalsDbContext : DbContext
    {
        public RentalsDbContext(DbContextOptions<RentalsDbContext> options)
            : base(options)
        {
        }

        // Exemplo: DbSets serão adicionados conforme as features
        // public DbSet<Motorcycle> Motorcycles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configurações de entidades irão aqui
        }
    }
}