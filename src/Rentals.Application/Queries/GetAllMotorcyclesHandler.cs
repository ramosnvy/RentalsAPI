using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Domain.Vehicles;

namespace Rentals.Application.Queries;

public class GetAllMotorcyclesHandler
{
    private readonly IMotorcycleRepository _motorcycleRepository;
    private readonly ILogger<GetAllMotorcyclesHandler> _logger;

    public GetAllMotorcyclesHandler(
        IMotorcycleRepository motorcycleRepository,
        ILogger<GetAllMotorcyclesHandler> logger)
    {
        _motorcycleRepository = motorcycleRepository;
        _logger = logger;
    }

    public async Task<GetAllMotorcyclesResult> Handle(string? licensePlate = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(licensePlate))
        {
            _logger.LogInformation("Iniciando busca de motos com filtro por placa: {LicensePlate}", licensePlate);
        }
        else
        {
            _logger.LogInformation("Iniciando busca de todas as motos cadastradas");
        }
        
        try
        {
            IEnumerable<Motorcycle> motorcycles;
            
            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                // Buscar por placa específica
                var motorcycle = await _motorcycleRepository.GetByLicensePlateAsync(licensePlate, cancellationToken);
                motorcycles = motorcycle != null ? new[] { motorcycle } : Enumerable.Empty<Motorcycle>();
            }
            else
            {
                // Buscar todas as motos
                motorcycles = await _motorcycleRepository.GetAllAsync(cancellationToken);
            }
            
            var motorcycleDtos = motorcycles.Select(m => new MotorcycleDto
            {
                Id = m.Id,
                Identificador = m.Identifier,
                Ano = m.Year,
                Modelo = m.Model,
                Placa = m.LicensePlate.Value,
                DataCadastro = m.CreatedAt,
                Ativo = m.IsActive
            }).ToList();

            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                _logger.LogInformation("Busca por placa concluída. {Count} motos encontradas para placa {LicensePlate}", 
                    motorcycleDtos.Count, licensePlate);
            }
            else
            {
                _logger.LogInformation("Busca concluída. {Count} motos encontradas", motorcycleDtos.Count);
            }
            
            return new GetAllMotorcyclesResult 
            { 
                Success = true, 
                Motorcycles = motorcycleDtos 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar motos cadastradas");
            return new GetAllMotorcyclesResult 
            { 
                Success = false, 
                ErrorMessage = "Erro interno ao buscar motos" 
            };
        }
    }
}

public class GetAllMotorcyclesResult
{
    public bool Success { get; set; }
    public List<MotorcycleDto> Motorcycles { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class MotorcycleDto
{
    public long Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public int Ano { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }
}
