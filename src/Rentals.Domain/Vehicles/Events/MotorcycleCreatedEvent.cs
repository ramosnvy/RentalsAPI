using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Vehicles.Events;

public class MotorcycleCreatedEvent : IDomainEvent
{
    public long MotorcycleId { get; }
    public string Identifier { get; }
    public int Year { get; }
    public string Model { get; }
    public string LicensePlate { get; }
    public DateTime CreatedAt { get; }

    public MotorcycleCreatedEvent(long motorcycleId, string identifier, int year, string model, string licensePlate, DateTime createdAt)
    {
        MotorcycleId = motorcycleId;
        Identifier = identifier;
        Year = year;
        Model = model;
        LicensePlate = licensePlate;
        CreatedAt = createdAt;
    }
}
