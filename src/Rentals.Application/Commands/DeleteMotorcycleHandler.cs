using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Application.Commands;

public class DeleteMotorcycleHandler
{
    private readonly IMotorcycleRepository _motorcycleRepository;
    private readonly ILogger<DeleteMotorcycleHandler> _logger;

    public DeleteMotorcycleHandler(
        IMotorcycleRepository motorcycleRepository,
        ILogger<DeleteMotorcycleHandler> logger)
    {
        _motorcycleRepository = motorcycleRepository;
        _logger = logger;
    }

    public async Task<DeleteMotorcycleResult> Handle(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando exclusão da moto ID: {Id}", id);
        
        try
        {
            // Buscar a moto
            var motorcycle = await _motorcycleRepository.GetByIdAsync(id, cancellationToken);
            if (motorcycle == null)
            {
                _logger.LogWarning("Moto com ID {Id} não encontrada para exclusão", id);
                return new DeleteMotorcycleResult 
                { 
                    Success = false, 
                    ErrorMessage = "Moto não encontrada" 
                };
            }

            // Deletar a moto (soft delete - marcar como inativa)
            motorcycle.Deactivate();
            await _motorcycleRepository.UpdateAsync(motorcycle, cancellationToken);

            _logger.LogInformation("Moto ID {Id} deletada com sucesso", id);
            
            return new DeleteMotorcycleResult 
            { 
                Success = true, 
                Message = "Moto deletada com sucesso" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar moto ID {Id}", id);
            return new DeleteMotorcycleResult 
            { 
                Success = false, 
                ErrorMessage = "Erro interno ao deletar moto" 
            };
        }
    }
}

public class DeleteMotorcycleResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}
