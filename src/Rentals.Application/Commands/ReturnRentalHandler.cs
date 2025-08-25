using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Application.Services;
using Rentals.Domain.Vehicles;

namespace Rentals.Application.Commands;

public class ReturnRentalHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IRentalCalculationService _calculationService;
    private readonly ILogger<ReturnRentalHandler> _logger;

    public ReturnRentalHandler(
        IRentalRepository rentalRepository,
        IRentalCalculationService calculationService,
        ILogger<ReturnRentalHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _calculationService = calculationService;
        _logger = logger;
    }

    public async Task<ReturnRentalResult> Handle(
        long rentalId,
        DateTime dataDevolucao,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando devolução da locação {RentalId} para {DataDevolucao}", rentalId, dataDevolucao);

            var rental = await _rentalRepository.GetByIdAsync(rentalId, cancellationToken);

            if (rental == null)
            {
                _logger.LogWarning("Locação {RentalId} não encontrada", rentalId);
                return new ReturnRentalResult
                {
                    Success = false,
                    ErrorMessage = "Locação não encontrada"
                };
            }

            if (rental.Status == "RETURNED")
            {
                _logger.LogWarning("Locação {RentalId} já foi devolvida", rentalId);
                return new ReturnRentalResult
                {
                    Success = false,
                    ErrorMessage = "Locação já foi devolvida"
                };
            }



            // Calcular valor final
            var valorFinal = await _calculationService.CalculateFinalAmount(rental, dataDevolucao, cancellationToken);

            // Atualizar locação
            rental.ReturnMotorcycle(dataDevolucao, valorFinal);

            await _rentalRepository.UpdateAsync(rental, cancellationToken);

            _logger.LogInformation("Locação {RentalId} devolvida com sucesso. Valor final: {ValorFinal}", rentalId, valorFinal);

            return new ReturnRentalResult
            {
                Success = true,
                ValorFinal = valorFinal,
                DataDevolucao = dataDevolucao,
                Message = "Moto devolvida com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante devolução da locação {RentalId}", rentalId);
            return new ReturnRentalResult
            {
                Success = false,
                ErrorMessage = "Erro interno ao processar devolução"
            };
        }
    }
}

public class ReturnRentalCommand
{
    public long RentalId { get; set; }
    public DateTime DataDevolucao { get; set; }
}

public class ReturnRentalResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? ValorFinal { get; set; }
    public DateTime? DataDevolucao { get; set; }
    public string? Message { get; set; }
}

