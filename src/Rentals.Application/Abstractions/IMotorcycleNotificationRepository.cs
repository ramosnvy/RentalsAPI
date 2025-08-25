using Rentals.Application.Commands;

namespace Rentals.Application.Abstractions;

public interface IMotorcycleNotificationRepository
{
    Task<MotorcycleNotification> AddAsync(MotorcycleNotification notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<MotorcycleNotification>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MotorcycleNotification>> GetByTypeAsync(string notificationType, CancellationToken cancellationToken = default);
}
