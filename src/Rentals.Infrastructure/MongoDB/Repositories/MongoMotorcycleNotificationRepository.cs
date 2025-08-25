using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rentals.Application.Abstractions;
using Rentals.Application.Commands;
using Rentals.Infrastructure.MongoDB.Documents;

namespace Rentals.Infrastructure.MongoDB.Repositories;

public class MongoMotorcycleNotificationRepository : IMotorcycleNotificationRepository
{
    private readonly IMongoCollection<MotorcycleNotificationDocument> _collection;

    public MongoMotorcycleNotificationRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<MotorcycleNotificationDocument>(settings.Value.NotificationsCollectionName);
    }

    public async Task<MotorcycleNotification> AddAsync(MotorcycleNotification notification, CancellationToken cancellationToken = default)
    {
        // Gerar ID sequencial
        var lastDocument = await _collection.Find(_ => true)
            .SortByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        
        var nextId = lastDocument?.Id + 1 ?? 1;
        
        var document = MotorcycleNotificationDocument.FromDomain(notification, nextId);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        
        return document.ToDomainWithId();
    }

    public async Task<IEnumerable<MotorcycleNotification>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(_ => true).ToListAsync(cancellationToken);
        return documents.Select(d => d.ToDomain());
    }

    public async Task<IEnumerable<MotorcycleNotification>> GetByTypeAsync(string notificationType, CancellationToken cancellationToken = default)
    {
        var documents = await _collection.Find(x => x.NotificationType == notificationType).ToListAsync(cancellationToken);
        return documents.Select(d => d.ToDomain());
    }
}
