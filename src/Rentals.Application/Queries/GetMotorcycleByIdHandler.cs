using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Application.Queries;

public class GetMotorcycleByIdHandler
{
    private readonly IMotorcycleRepository _motorcycleRepository;
    private readonly ILogger<GetMotorcycleByIdHandler> _logger;

    public GetMotorcycleByIdHandler(
        IMotorcycleRepository motorcycleRepository,
        ILogger<GetMotorcycleByIdHandler> logger)
    {
        _motorcycleRepository = motorcycleRepository;
        _logger = logger;
    }

    public async Task<GetMotorcycleByIdResult> Handle(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando busca de moto por ID: {Id}", id);
        
        try
        {
            var motorcycle = await _motorcycleRepository.GetByIdAsync(id, cancellationToken);
            
            if (motorcycle == null)
            {
                _logger.LogWarning("Moto com ID {Id} não encontrada", id);
                return new GetMotorcycleByIdResult 
                { 
                    Success = false, 
                    ErrorMessage = "Moto não encontrada" 
                };
            }

            var motorcycleDto = new MotorcycleDto
            {
                Id = motorcycle.Id,
                Identificador = motorcycle.Identifier,
                Ano = motorcycle.Year,
                Modelo = motorcycle.Model,
                Placa = motorcycle.LicensePlate.Value,
                DataCadastro = motorcycle.CreatedAt,
                Ativo = motorcycle.IsActive
            };

            _logger.LogInformation("Moto com ID {Id} encontrada com sucesso", id);
            
            return new GetMotorcycleByIdResult 
            { 
                Success = true, 
                Motorcycle = motorcycleDto 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar moto com ID {Id}", id);
            return new GetMotorcycleByIdResult 
            { 
                Success = false, 
                ErrorMessage = "Erro interno ao buscar moto" 
            };
        }
    }
}

public class GetMotorcycleByIdResult
{
    public bool Success { get; set; }
    public MotorcycleDto? Motorcycle { get; set; }
    public string? ErrorMessage { get; set; }
}
