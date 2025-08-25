using Rentals.Domain.Vehicles;

namespace Rentals.Application.Abstractions;

public interface IRentalPlanRepository
{
    Task<RentalPlan?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RentalPlan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RentalPlan>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<RentalPlan> AddAsync(RentalPlan rentalPlan, CancellationToken cancellationToken = default);
    Task UpdateAsync(RentalPlan rentalPlan, CancellationToken cancellationToken = default);
}
