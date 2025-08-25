using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Infrastructure.Repositories;

public class RentalPlanRepository : IRentalPlanRepository
{
    private readonly RentalsDbContext _context;
    private readonly ILogger<RentalPlanRepository> _logger;

    public RentalPlanRepository(RentalsDbContext context, ILogger<RentalPlanRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Rentals.Domain.Vehicles.RentalPlan?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RentalPlans
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity == null) return null;

        return new Rentals.Domain.Vehicles.RentalPlan(
            entity.Id,
            entity.Name,
            entity.DurationInDays,
            entity.DailyRate,
            entity.EarlyReturnPenaltyPercentage,
            entity.LateReturnDailyFee,
            entity.IsActive,
            entity.CreatedAt);
    }

    public async Task<IEnumerable<Rentals.Domain.Vehicles.RentalPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.RentalPlans
            .OrderBy(p => p.DurationInDays)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new Rentals.Domain.Vehicles.RentalPlan(
            e.Id,
            e.Name,
            e.DurationInDays,
            e.DailyRate,
            e.EarlyReturnPenaltyPercentage,
            e.LateReturnDailyFee,
            e.IsActive,
            e.CreatedAt));
    }

    public async Task<IEnumerable<Rentals.Domain.Vehicles.RentalPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.RentalPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.DurationInDays)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new Rentals.Domain.Vehicles.RentalPlan(
            e.Id,
            e.Name,
            e.DurationInDays,
            e.DailyRate,
            e.EarlyReturnPenaltyPercentage,
            e.LateReturnDailyFee,
            e.IsActive,
            e.CreatedAt));
    }

    public async Task<Rentals.Domain.Vehicles.RentalPlan> AddAsync(Rentals.Domain.Vehicles.RentalPlan rentalPlan, CancellationToken cancellationToken = default)
    {
        var entity = new Entities.RentalPlan
        {
            Name = rentalPlan.Name,
            DurationInDays = rentalPlan.DurationInDays,
            DailyRate = rentalPlan.DailyRate,
            EarlyReturnPenaltyPercentage = rentalPlan.EarlyReturnPenaltyPercentage,
            LateReturnDailyFee = rentalPlan.LateReturnDailyFee,
            IsActive = rentalPlan.IsActive,
            CreatedAt = rentalPlan.CreatedAt
        };

        _context.RentalPlans.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new Rentals.Domain.Vehicles.RentalPlan(
            entity.Id,
            entity.Name,
            entity.DurationInDays,
            entity.DailyRate,
            entity.EarlyReturnPenaltyPercentage,
            entity.LateReturnDailyFee,
            entity.IsActive,
            entity.CreatedAt);
    }

    public async Task UpdateAsync(Rentals.Domain.Vehicles.RentalPlan rentalPlan, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RentalPlans.FindAsync(new object[] { rentalPlan.Id }, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Plano com ID {Id} não encontrado para atualização", rentalPlan.Id);
            return;
        }

        entity.Name = rentalPlan.Name;
        entity.DurationInDays = rentalPlan.DurationInDays;
        entity.DailyRate = rentalPlan.DailyRate;
        entity.EarlyReturnPenaltyPercentage = rentalPlan.EarlyReturnPenaltyPercentage;
        entity.LateReturnDailyFee = rentalPlan.LateReturnDailyFee;
        entity.IsActive = rentalPlan.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
