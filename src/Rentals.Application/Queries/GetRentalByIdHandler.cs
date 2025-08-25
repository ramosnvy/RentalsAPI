using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Application.Queries;

public class GetRentalByIdHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly ILogger<GetRentalByIdHandler> _logger;

    public GetRentalByIdHandler(
        IRentalRepository rentalRepository,
        ILogger<GetRentalByIdHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _logger = logger;
    }

    public async Task<GetRentalByIdResult> Handle(
        long rentalId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Buscando locação {RentalId}", rentalId);

            var rental = await _rentalRepository.GetByIdAsync(rentalId, cancellationToken);

            if (rental == null)
            {
                _logger.LogWarning("Locação {RentalId} não encontrada", rentalId);
                return new GetRentalByIdResult
                {
                    Success = false,
                    ErrorMessage = "Locação não encontrada"
                };
            }

            var rentalDto = new RentalDto
            {
                Id = rental.Id,
                EntregadorId = rental.DeliveryDriverId,
                MotoId = rental.MotorcycleId,
                PlanoId = rental.RentalPlanId,
                DataInicio = rental.StartDate,
                DataTerminoPrevista = rental.ExpectedEndDate,
                DataTerminoReal = rental.ActualEndDate,
                ValorTotal = rental.TotalAmount,
                ValorFinal = rental.FinalAmount,
                Status = rental.Status,
                DataCriacao = rental.CreatedAt,
                DataAtualizacao = rental.UpdatedAt
            };

            return new GetRentalByIdResult
            {
                Success = true,
                Locacao = rentalDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar locação {RentalId}", rentalId);
            return new GetRentalByIdResult
            {
                Success = false,
                ErrorMessage = "Erro interno ao buscar locação"
            };
        }
    }
}

public class GetRentalByIdQuery
{
    public long RentalId { get; set; }
}

public class GetRentalByIdResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public RentalDto? Locacao { get; set; }
}

public class RentalDto
{
    public long Id { get; set; }
    public long EntregadorId { get; set; }
    public long MotoId { get; set; }
    public long PlanoId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataTerminoPrevista { get; set; }
    public DateTime? DataTerminoReal { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal? ValorFinal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

