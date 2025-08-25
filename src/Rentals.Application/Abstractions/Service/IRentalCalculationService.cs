using Rentals.Domain.Vehicles;

namespace Rentals.Application.Services;

public interface IRentalCalculationService
{
    decimal CalculateTotalAmount(RentalPlan plan, DateTime startDate, DateTime expectedEndDate);
    Task<decimal> CalculateFinalAmount(Rental rental, DateTime actualReturnDate, CancellationToken cancellationToken = default);
    Task InitializeDefaultPlans(CancellationToken cancellationToken = default);
    Task<RentalPlan?> GetRentalPlanById(long planId, CancellationToken cancellationToken = default);
}
