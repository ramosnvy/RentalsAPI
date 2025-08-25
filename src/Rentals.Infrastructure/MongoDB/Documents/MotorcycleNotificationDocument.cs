using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Rentals.Application.Commands;

namespace Rentals.Infrastructure.MongoDB.Documents;

public class MotorcycleNotificationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.Int64)]
    public long Id { get; set; }

    [BsonElement("motorcycleId")]
    public long MotorcycleId { get; set; }

    [BsonElement("identifier")]
    public string Identifier { get; set; } = string.Empty;

    [BsonElement("year")]
    public int Year { get; set; }

    [BsonElement("model")]
    public string Model { get; set; } = string.Empty;

    [BsonElement("licensePlate")]
    public string LicensePlate { get; set; } = string.Empty;

    [BsonElement("notificationType")]
    public string NotificationType { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    public static MotorcycleNotificationDocument FromDomain(MotorcycleNotification notification)
    {
        return new MotorcycleNotificationDocument
        {
            Id = notification.Id,
            MotorcycleId = notification.MotorcycleId,
            Identifier = notification.Identifier,
            Year = notification.Year,
            Model = notification.Model,
            LicensePlate = notification.LicensePlate,
            NotificationType = notification.NotificationType,
            Message = notification.Message,
            CreatedAt = notification.CreatedAt
        };
    }

    public static MotorcycleNotificationDocument FromDomain(MotorcycleNotification notification, long id)
    {
        return new MotorcycleNotificationDocument
        {
            Id = id,
            MotorcycleId = notification.MotorcycleId,
            Identifier = notification.Identifier,
            Year = notification.Year,
            Model = notification.Model,
            LicensePlate = notification.LicensePlate,
            NotificationType = notification.NotificationType,
            Message = notification.Message,
            CreatedAt = notification.CreatedAt
        };
    }

    public MotorcycleNotification ToDomain()
    {
        return new MotorcycleNotification
        {
            Id = Id,
            MotorcycleId = MotorcycleId,
            Identifier = Identifier,
            Year = Year,
            Model = Model,
            LicensePlate = LicensePlate,
            NotificationType = NotificationType,
            Message = Message,
            CreatedAt = CreatedAt
        };
    }

    public MotorcycleNotification ToDomainWithId()
    {
        return new MotorcycleNotification
        {
            Id = Id,
            MotorcycleId = MotorcycleId,
            Identifier = Identifier,
            Year = Year,
            Model = Model,
            LicensePlate = LicensePlate,
            NotificationType = NotificationType,
            Message = Message,
            CreatedAt = CreatedAt
        };
    }
}
