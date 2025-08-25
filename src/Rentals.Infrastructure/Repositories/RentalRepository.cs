using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Infrastructure.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly RentalsDbContext _context;
    private readonly ILogger<RentalRepository> _logger;

    public RentalRepository(RentalsDbContext context, ILogger<RentalRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Rentals.Domain.Vehicles.Rental?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Rentals
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (entity == null) return null;

        return new Rentals.Domain.Vehicles.Rental(
            entity.Id,
            entity.DeliveryDriverId,
            entity.MotorcycleId,
            entity.RentalPlanId,
            entity.StartDate,
            entity.ExpectedEndDate,
            entity.ActualEndDate,
            entity.TotalAmount,
            entity.FinalAmount,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public async Task<IEnumerable<Rentals.Domain.Vehicles.Rental>> GetByDeliveryDriverIdAsync(long deliveryDriverId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Rentals
            .Where(r => r.DeliveryDriverId == deliveryDriverId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new Rentals.Domain.Vehicles.Rental(
            e.Id,
            e.DeliveryDriverId,
            e.MotorcycleId,
            e.RentalPlanId,
            e.StartDate,
            e.ExpectedEndDate,
            e.ActualEndDate,
            e.TotalAmount,
            e.FinalAmount,
            e.Status,
            e.CreatedAt,
            e.UpdatedAt));
    }

    public async Task<IEnumerable<Rentals.Domain.Vehicles.Rental>> GetActiveByMotorcycleIdAsync(long motorcycleId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Rentals
            .Where(r => r.MotorcycleId == motorcycleId && r.Status == "ACTIVE")
            .ToListAsync(cancellationToken);

        return entities.Select(e => new Rentals.Domain.Vehicles.Rental(
            e.Id,
            e.DeliveryDriverId,
            e.MotorcycleId,
            e.RentalPlanId,
            e.StartDate,
            e.ExpectedEndDate,
            e.ActualEndDate,
            e.TotalAmount,
            e.FinalAmount,
            e.Status,
            e.CreatedAt,
            e.UpdatedAt));
    }

    public async Task<Rentals.Domain.Vehicles.Rental> AddAsync(Rentals.Domain.Vehicles.Rental rental, CancellationToken cancellationToken = default)
    {
        var entity = new Entities.Rental
        {
            DeliveryDriverId = rental.DeliveryDriverId,
            MotorcycleId = rental.MotorcycleId,
            RentalPlanId = rental.RentalPlanId,
            StartDate = rental.StartDate,
            ExpectedEndDate = rental.ExpectedEndDate,
            ActualEndDate = rental.ActualEndDate,
            TotalAmount = rental.TotalAmount,
            FinalAmount = rental.FinalAmount,
            Status = rental.Status,
            CreatedAt = rental.CreatedAt,
            UpdatedAt = rental.UpdatedAt
        };

        _context.Rentals.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new Rentals.Domain.Vehicles.Rental(
            entity.Id,
            entity.DeliveryDriverId,
            entity.MotorcycleId,
            entity.RentalPlanId,
            entity.StartDate,
            entity.ExpectedEndDate,
            entity.ActualEndDate,
            entity.TotalAmount,
            entity.FinalAmount,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public async Task UpdateAsync(Rentals.Domain.Vehicles.Rental rental, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Rentals.FindAsync(new object[] { rental.Id }, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Locação com ID {Id} não encontrada para atualização", rental.Id);
            return;
        }

        entity.DeliveryDriverId = rental.DeliveryDriverId;
        entity.MotorcycleId = rental.MotorcycleId;
        entity.RentalPlanId = rental.RentalPlanId;
        entity.StartDate = rental.StartDate;
        entity.ExpectedEndDate = rental.ExpectedEndDate;
        entity.ActualEndDate = rental.ActualEndDate;
        entity.TotalAmount = rental.TotalAmount;
        entity.FinalAmount = rental.FinalAmount;
        entity.Status = rental.Status;
        entity.UpdatedAt = rental.UpdatedAt;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
