using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Vehicles;

public class Rental : Entity
{
    public long DeliveryDriverId { get; private set; }
    public long MotorcycleId { get; private set; }
    public long RentalPlanId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime ExpectedEndDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal? FinalAmount { get; private set; }
    public string Status { get; private set; } // ACTIVE, RETURNED, CANCELLED
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Rental() { } // Para EF Core

    public Rental(long id, long deliveryDriverId, long motorcycleId, long rentalPlanId, 
        DateTime startDate, DateTime expectedEndDate, DateTime? actualEndDate, 
        decimal totalAmount, decimal? finalAmount, string status, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        DeliveryDriverId = deliveryDriverId;
        MotorcycleId = motorcycleId;
        RentalPlanId = rentalPlanId;
        StartDate = startDate;
        ExpectedEndDate = expectedEndDate;
        ActualEndDate = actualEndDate;
        TotalAmount = totalAmount;
        FinalAmount = finalAmount;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Rental Create(long deliveryDriverId, long motorcycleId, long rentalPlanId, 
        DateTime startDate, DateTime expectedEndDate, decimal totalAmount)
    {
        if (deliveryDriverId <= 0)
            throw new ArgumentException("ID do entregador é obrigatório", nameof(deliveryDriverId));

        if (motorcycleId <= 0)
            throw new ArgumentException("ID da moto é obrigatório", nameof(motorcycleId));

        if (rentalPlanId <= 0)
            throw new ArgumentException("ID do plano é obrigatório", nameof(rentalPlanId));

        if (startDate <= DateTime.Today)
            throw new ArgumentException("Data de início deve ser posterior a hoje", nameof(startDate));

        if (expectedEndDate <= startDate)
            throw new ArgumentException("Data de término prevista deve ser posterior à data de início", nameof(expectedEndDate));

        if (totalAmount <= 0)
            throw new ArgumentException("Valor total deve ser maior que zero", nameof(totalAmount));

        return new Rental
        {
            DeliveryDriverId = deliveryDriverId,
            MotorcycleId = motorcycleId,
            RentalPlanId = rentalPlanId,
            StartDate = startDate,
            ExpectedEndDate = expectedEndDate,
            TotalAmount = totalAmount,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ReturnMotorcycle(DateTime returnDate, decimal finalAmount)
    {
        if (Status != "ACTIVE")
            throw new InvalidOperationException("Apenas locações ativas podem ser finalizadas");

        if (returnDate < StartDate)
            throw new ArgumentException("Data de devolução não pode ser anterior à data de início", nameof(returnDate));

        ActualEndDate = returnDate;
        FinalAmount = finalAmount;
        Status = "RETURNED";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != "ACTIVE")
            throw new InvalidOperationException("Apenas locações ativas podem ser canceladas");

        Status = "CANCELLED";
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => Status == "ACTIVE";
    public bool IsReturned => Status == "RETURNED";
    public bool IsCancelled => Status == "CANCELLED";
}
