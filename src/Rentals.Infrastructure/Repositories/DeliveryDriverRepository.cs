using Microsoft.EntityFrameworkCore;
using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;

namespace Rentals.Infrastructure.Repositories;

public class DeliveryDriverRepository : IDeliveryDriverRepository
{
    private readonly RentalsDbContext _context;

    public DeliveryDriverRepository(RentalsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DeliveryDriver driver)
    {
        _context.DeliveryDrivers.Add(driver);
        await _context.SaveChangesAsync();
    }

    public async Task<DeliveryDriver?> GetByIdAsync(long id)
        => await _context.DeliveryDrivers.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<bool> ExistsByCnpjAsync(string cnpj)
        => await _context.DeliveryDrivers.AnyAsync(d => d.Cnpj.Value == cnpj);

    public async Task<bool> ExistsByCnhNumberAsync(string cnhNumber)
        => await _context.DeliveryDrivers.AnyAsync(d => d.Cnh.Number == cnhNumber);

    public async Task UpdateAsync(DeliveryDriver driver)
    {
        _context.DeliveryDrivers.Update(driver);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<DeliveryDriver>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.DeliveryDrivers
            .Include("Cnh")
            .Include("CnhImage")
            .ToListAsync(cancellationToken);
}