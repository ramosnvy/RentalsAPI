using Rentals.Domain.Vehicles;

namespace Rentals.Application.Abstractions;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rental>> GetByDeliveryDriverIdAsync(long deliveryDriverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rental>> GetActiveByMotorcycleIdAsync(long motorcycleId, CancellationToken cancellationToken = default);
    Task<Rental> AddAsync(Rental rental, CancellationToken cancellationToken = default);
    Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default);
}
