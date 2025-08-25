using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Domain.Vehicles;

namespace Rentals.Application.Commands;

public class CreateRentalPlanHandler
{
    private readonly IRentalPlanRepository _rentalPlanRepository;
    private readonly ILogger<CreateRentalPlanHandler> _logger;

    public CreateRentalPlanHandler(
        IRentalPlanRepository rentalPlanRepository,
        ILogger<CreateRentalPlanHandler> logger)
    {
        _rentalPlanRepository = rentalPlanRepository;
        _logger = logger;
    }

    public async Task<CreateRentalPlanResult> Handle(
        string name,
        int durationInDays,
        decimal dailyRate,
        decimal earlyReturnPenaltyPercentage,
        decimal lateReturnDailyFee,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando criação do plano de locação {Name}", name);

        try
        {
            var rentalPlan = RentalPlan.Create(
                name,
                durationInDays,
                dailyRate,
                earlyReturnPenaltyPercentage,
                lateReturnDailyFee);

            await _rentalPlanRepository.AddAsync(rentalPlan, cancellationToken);

            _logger.LogInformation("Plano de locação {Name} criado com sucesso", name);

            return new CreateRentalPlanResult
            {
                Success = true,
                RentalPlanId = rentalPlan.Id
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Criação do plano falhou: {ErrorMessage}", ex.Message);
            return new CreateRentalPlanResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante criação do plano {Name}", name);
            return new CreateRentalPlanResult
            {
                Success = false,
                ErrorMessage = "Erro interno ao criar plano de locação"
            };
        }
    }
}

public class CreateRentalPlanResult
{
    public bool Success { get; set; }
    public long RentalPlanId { get; set; }
    public string? ErrorMessage { get; set; }
}
