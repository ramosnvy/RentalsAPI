using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Application.Queries;

public class GetAllRentalPlansHandler
{
    private readonly IRentalPlanRepository _rentalPlanRepository;
    private readonly ILogger<GetAllRentalPlansHandler> _logger;

    public GetAllRentalPlansHandler(
        IRentalPlanRepository rentalPlanRepository,
        ILogger<GetAllRentalPlansHandler> logger)
    {
        _rentalPlanRepository = rentalPlanRepository;
        _logger = logger;
    }

    public async Task<GetAllRentalPlansResult> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando busca de todos os planos de locação");

        try
        {
            var rentalPlans = await _rentalPlanRepository.GetAllAsync(cancellationToken);

            var rentalPlanDtos = rentalPlans.Select(p => new RentalPlanDto
            {
                Id = p.Id,
                Nome = p.Name,
                DuracaoDias = p.DurationInDays,
                TaxaDiaria = p.DailyRate,
                PercentualMultaAntecipada = p.EarlyReturnPenaltyPercentage,
                TaxaAtrasoDiaria = p.LateReturnDailyFee,
                Ativo = p.IsActive,
                DataCriacao = p.CreatedAt
            }).ToList();

            _logger.LogInformation("Busca concluída. {Count} planos encontrados", rentalPlanDtos.Count);

            return new GetAllRentalPlansResult
            {
                Success = true,
                RentalPlans = rentalPlanDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar planos de locação");
            return new GetAllRentalPlansResult
            {
                Success = false,
                ErrorMessage = "Erro interno ao buscar planos de locação"
            };
        }
    }
}

public class GetAllRentalPlansResult
{
    public bool Success { get; set; }
    public List<RentalPlanDto> RentalPlans { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class RentalPlanDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int DuracaoDias { get; set; }
    public decimal TaxaDiaria { get; set; }
    public decimal PercentualMultaAntecipada { get; set; }
    public decimal TaxaAtrasoDiaria { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}
