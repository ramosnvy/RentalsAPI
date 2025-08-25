using Rentals.Domain.Vehicles;

namespace Rentals.Application.Abstractions;

public interface IMotorcycleRepository
{
    Task<Motorcycle?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Motorcycle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default);
    Task<bool> LicensePlateExistsAsync(string licensePlate, CancellationToken cancellationToken = default);
    Task<Motorcycle> AddAsync(Motorcycle motorcycle, CancellationToken cancellationToken = default);
    Task<IEnumerable<Motorcycle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Motorcycle motorcycle, CancellationToken cancellationToken = default);
}
