using Rentals.Domain.Abstractions;
using Rentals.Domain.Vehicles.ValueObjects;
using Rentals.Domain.Vehicles.Events;

namespace Rentals.Domain.Vehicles;

public class Motorcycle : Entity
{
    public string Identifier { get; private set; }
    public int Year { get; private set; }
    public string Model { get; private set; }
    public LicensePlate LicensePlate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Motorcycle() { } // Para EF Core

    // Construtor para reconstruir entidade do banco
    public Motorcycle(long id, string identifier, int year, string model, string licensePlate, DateTime createdAt, bool isActive)
    {
        Id = id;
        Identifier = identifier;
        Year = year;
        Model = model;
        LicensePlate = LicensePlate.Create(licensePlate);
        CreatedAt = createdAt;
        IsActive = isActive;
    }

    public static Motorcycle Create(string identifier, int year, string model, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identificador é obrigatório", nameof(identifier));

        if (year <= 0)
            throw new ArgumentException("Ano deve ser maior que zero", nameof(year));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Modelo é obrigatório", nameof(model));

        var plate = LicensePlate.Create(licensePlate);

        var motorcycle = new Motorcycle
        {
            Identifier = identifier,
            Year = year,
            Model = model,
            LicensePlate = plate,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Adicionar o evento de domínio
        motorcycle.AddDomainEvent(new MotorcycleCreatedEvent(
            motorcycle.Id,
            motorcycle.Identifier,
            motorcycle.Year,
            motorcycle.Model,
            motorcycle.LicensePlate.Value,
            motorcycle.CreatedAt));

        return motorcycle;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateLicensePlate(LicensePlate novaPlaca)
    {
        LicensePlate = novaPlaca;
    }
}
