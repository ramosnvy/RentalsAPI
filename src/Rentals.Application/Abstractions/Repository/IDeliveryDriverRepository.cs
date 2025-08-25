using Rentals.Domain.Drivers;

namespace Rentals.Application.Abstractions;

public interface IDeliveryDriverRepository
{
    Task AddAsync(DeliveryDriver driver);
    
    Task UpdateAsync(DeliveryDriver driver);

    Task<bool> ExistsByCnpjAsync(string cnpj);
    
    Task<bool> ExistsByCnhNumberAsync(string cnhNumber);
    
    Task<DeliveryDriver?> GetByIdAsync(long id);
    
    Task<IEnumerable<DeliveryDriver>> GetAllAsync(CancellationToken cancellationToken = default);
}