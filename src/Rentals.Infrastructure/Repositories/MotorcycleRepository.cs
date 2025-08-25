using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Infrastructure.Repositories;

public class MotorcycleRepository : IMotorcycleRepository
{
    private readonly RentalsDbContext _context;
    private readonly ILogger<MotorcycleRepository> _logger;

    public MotorcycleRepository(RentalsDbContext context, ILogger<MotorcycleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Rentals.Domain.Vehicles.Motorcycle?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Motorcycles
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken);

        if (entity == null) return null;

        return new Rentals.Domain.Vehicles.Motorcycle(
            entity.Id,
            entity.Identifier,
            entity.Year,
            entity.Model,
            entity.LicensePlate,
            entity.CreatedAt,
            entity.IsActive);
    }

    public async Task<Rentals.Domain.Vehicles.Motorcycle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Motorcycles
            .FirstOrDefaultAsync(m => m.LicensePlate == licensePlate && m.IsActive, cancellationToken);

        if (entity == null) return null;

        return new Rentals.Domain.Vehicles.Motorcycle(
            entity.Id,
            entity.Identifier,
            entity.Year,
            entity.Model,
            entity.LicensePlate,
            entity.CreatedAt,
            entity.IsActive);
    }

    public async Task<bool> LicensePlateExistsAsync(string licensePlate, CancellationToken cancellationToken = default)
    {
        return await _context.Motorcycles
            .AnyAsync(m => m.LicensePlate == licensePlate && m.IsActive, cancellationToken);
    }

    public async Task<Rentals.Domain.Vehicles.Motorcycle> AddAsync(Rentals.Domain.Vehicles.Motorcycle motorcycle, CancellationToken cancellationToken = default)
    {
        var entity = new Rentals.Infrastructure.Entities.Motorcycle
        {
            Identifier = motorcycle.Identifier,
            Year = motorcycle.Year,
            Model = motorcycle.Model,
            LicensePlate = motorcycle.LicensePlate.Value,
            CreatedAt = motorcycle.CreatedAt,
            IsActive = motorcycle.IsActive
        };

        _context.Motorcycles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Retornar com o ID gerado
        return new Rentals.Domain.Vehicles.Motorcycle(
            entity.Id,
            entity.Identifier,
            entity.Year,
            entity.Model,
            entity.LicensePlate,
            entity.CreatedAt,
            entity.IsActive);
    }

    public async Task<IEnumerable<Rentals.Domain.Vehicles.Motorcycle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Motorcycles
            .Where(m => m.IsActive)
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new Rentals.Domain.Vehicles.Motorcycle(
            e.Id,
            e.Identifier,
            e.Year,
            e.Model,
            e.LicensePlate,
            e.CreatedAt,
            e.IsActive));
    }

    public async Task UpdateAsync(Rentals.Domain.Vehicles.Motorcycle motorcycle, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Motorcycles.FindAsync(new object[] { motorcycle.Id }, cancellationToken);
        
        if (entity == null)
        {
            _logger.LogWarning("Moto com ID {Id} não encontrada para atualização", motorcycle.Id);
            return;
        }

        entity.Identifier = motorcycle.Identifier;
        entity.Year = motorcycle.Year;
        entity.Model = motorcycle.Model;
        entity.LicensePlate = motorcycle.LicensePlate.Value;
        entity.IsActive = motorcycle.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
