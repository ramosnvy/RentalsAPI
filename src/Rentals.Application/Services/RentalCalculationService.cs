using Rentals.Domain.Vehicles;
using Rentals.Application.Abstractions;

namespace Rentals.Application.Services;

public class RentalCalculationService : IRentalCalculationService
{
    private readonly IRentalPlanRepository _rentalPlanRepository;

    public RentalCalculationService(IRentalPlanRepository rentalPlanRepository)
    {
        _rentalPlanRepository = rentalPlanRepository;
    }

    public decimal CalculateTotalAmount(RentalPlan plan, DateTime startDate, DateTime expectedEndDate)
    {
        var days = (expectedEndDate - startDate).Days;
        return plan.DailyRate * days;
    }

    public async Task<decimal> CalculateFinalAmount(Rental rental, DateTime actualReturnDate, CancellationToken cancellationToken = default)
    {
        // Buscar o plano para obter os valores corretos
        var plan = await _rentalPlanRepository.GetByIdAsync(rental.RentalPlanId, cancellationToken);
        if (plan == null)
        {
            throw new InvalidOperationException("Plano de locação não encontrado");
        }

        var actualDays = (actualReturnDate - rental.StartDate).Days;
        var expectedDays = (rental.ExpectedEndDate - rental.StartDate).Days;

        // Valor base das diárias utilizadas (valor real usado)
        var baseAmount = plan.DailyRate * actualDays;

        if (actualReturnDate < rental.ExpectedEndDate)
        {
            // Devolução antecipada - aplicar multa
            var unusedDays = expectedDays - actualDays;
            var unusedAmount = plan.DailyRate * unusedDays;
            var penaltyAmount = unusedAmount * (plan.EarlyReturnPenaltyPercentage / 100m);
            
            return baseAmount + penaltyAmount;
        }
        else if (actualReturnDate > rental.ExpectedEndDate)
        {
            // Devolução tardia - aplicar taxa adicional
            var extraDays = actualDays - expectedDays;
            var extraFee = extraDays * plan.LateReturnDailyFee;
            
            return baseAmount + extraFee;
        }

        return baseAmount;
    }

    public async Task InitializeDefaultPlans(CancellationToken cancellationToken = default)
    {
        // Verificar se já existem planos
        var existingPlans = await _rentalPlanRepository.GetAllAsync(cancellationToken);
        if (existingPlans.Any())
        {
            return; // Já existem planos, não inicializar novamente
        }

        // Criar os planos padrão conforme especificação
        var defaultPlans = new[]
        {
            RentalPlan.Create("Plano 7 Dias", 7, 30.00m, 20.00m, 50.00m),
            RentalPlan.Create("Plano 15 Dias", 15, 28.00m, 40.00m, 50.00m),
            RentalPlan.Create("Plano 30 Dias", 30, 22.00m, 0.00m, 50.00m), // Sem multa antecipada
            RentalPlan.Create("Plano 45 Dias", 45, 20.00m, 0.00m, 50.00m), // Sem multa antecipada
            RentalPlan.Create("Plano 50 Dias", 50, 18.00m, 0.00m, 50.00m)  // Sem multa antecipada
        };

        foreach (var plan in defaultPlans)
        {
            await _rentalPlanRepository.AddAsync(plan, cancellationToken);
        }
    }

    public async Task<RentalPlan?> GetRentalPlanById(long planId, CancellationToken cancellationToken = default)
    {
        return await _rentalPlanRepository.GetByIdAsync(planId, cancellationToken);
    }
}
