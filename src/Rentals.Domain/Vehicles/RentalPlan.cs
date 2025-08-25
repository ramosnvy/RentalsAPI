using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Vehicles;

public class RentalPlan : Entity
{
    public string Name { get; private set; }
    public int DurationInDays { get; private set; }
    public decimal DailyRate { get; private set; }
    public decimal EarlyReturnPenaltyPercentage { get; private set; }
    public decimal LateReturnDailyFee { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RentalPlan() { } // Para EF Core

    public RentalPlan(long id, string name, int durationInDays, decimal dailyRate, 
        decimal earlyReturnPenaltyPercentage, decimal lateReturnDailyFee, bool isActive, DateTime createdAt)
    {
        Id = id;
        Name = name;
        DurationInDays = durationInDays;
        DailyRate = dailyRate;
        EarlyReturnPenaltyPercentage = earlyReturnPenaltyPercentage;
        LateReturnDailyFee = lateReturnDailyFee;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public static RentalPlan Create(string name, int durationInDays, decimal dailyRate, 
        decimal earlyReturnPenaltyPercentage, decimal lateReturnDailyFee)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do plano é obrigatório", nameof(name));

        if (durationInDays <= 0)
            throw new ArgumentException("Duração deve ser maior que zero", nameof(durationInDays));

        if (dailyRate <= 0)
            throw new ArgumentException("Taxa diária deve ser maior que zero", nameof(dailyRate));

        if (earlyReturnPenaltyPercentage < 0)
            throw new ArgumentException("Percentual de multa não pode ser negativo", nameof(earlyReturnPenaltyPercentage));

        if (lateReturnDailyFee < 0)
            throw new ArgumentException("Taxa de atraso não pode ser negativa", nameof(lateReturnDailyFee));

        return new RentalPlan
        {
            Name = name,
            DurationInDays = durationInDays,
            DailyRate = dailyRate,
            EarlyReturnPenaltyPercentage = earlyReturnPenaltyPercentage,
            LateReturnDailyFee = lateReturnDailyFee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Update(string name, int durationInDays, decimal dailyRate, 
        decimal earlyReturnPenaltyPercentage, decimal lateReturnDailyFee)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do plano é obrigatório", nameof(name));

        if (durationInDays <= 0)
            throw new ArgumentException("Duração deve ser maior que zero", nameof(durationInDays));

        if (dailyRate <= 0)
            throw new ArgumentException("Taxa diária deve ser maior que zero", nameof(dailyRate));

        if (earlyReturnPenaltyPercentage < 0)
            throw new ArgumentException("Percentual de multa não pode ser negativo", nameof(earlyReturnPenaltyPercentage));

        if (lateReturnDailyFee < 0)
            throw new ArgumentException("Taxa de atraso não pode ser negativa", nameof(lateReturnDailyFee));

        Name = name;
        DurationInDays = durationInDays;
        DailyRate = dailyRate;
        EarlyReturnPenaltyPercentage = earlyReturnPenaltyPercentage;
        LateReturnDailyFee = lateReturnDailyFee;
    }
}
