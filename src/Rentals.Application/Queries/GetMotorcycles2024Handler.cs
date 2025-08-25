using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Application.Queries;

public class GetMotorcycles2024Handler
{
    private readonly IMotorcycleRepository _motorcycleRepository;
    private readonly IMotorcycleNotificationRepository _notificationRepository;
    private readonly ILogger<GetMotorcycles2024Handler> _logger;

    public GetMotorcycles2024Handler(
        IMotorcycleRepository motorcycleRepository,
        IMotorcycleNotificationRepository notificationRepository,
        ILogger<GetMotorcycles2024Handler> logger)
    {
        _motorcycleRepository = motorcycleRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<GetMotorcycles2024Result> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando busca de motos de 2024 e suas notificações");
        
        try
        {
            // Buscar motos de 2024 do PostgreSQL
            var motorcycles2024 = await _motorcycleRepository.GetAllAsync(cancellationToken);
            motorcycles2024 = motorcycles2024.Where(m => m.Year == 2024 && m.IsActive).ToList();

            // Buscar notificações de 2024 do MongoDB
            var notifications2024 = await _notificationRepository.GetByTypeAsync("2024_MODEL", cancellationToken);

            var motorcycleDtos = motorcycles2024.Select(m => new Motorcycle2024Dto
            {
                Id = m.Id,
                Identificador = m.Identifier,
                Ano = m.Year,
                Modelo = m.Model,
                Placa = m.LicensePlate.Value,
                DataCadastro = m.CreatedAt,
                Ativo = m.IsActive,
                Notificacao = notifications2024.FirstOrDefault(n => n.MotorcycleId == m.Id)?.Message
            }).ToList();

            _logger.LogInformation("Busca concluída. {Count} motos de 2024 encontradas", motorcycleDtos.Count);
            
            return new GetMotorcycles2024Result 
            { 
                Success = true, 
                Motorcycles = motorcycleDtos 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar motos de 2024");
            return new GetMotorcycles2024Result 
            { 
                Success = false, 
                ErrorMessage = "Erro interno ao buscar motos de 2024" 
            };
        }
    }
}

public class GetMotorcycles2024Result
{
    public bool Success { get; set; }
    public List<Motorcycle2024Dto> Motorcycles { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class Motorcycle2024Dto
{
    public long Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public int Ano { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }
    public string? Notificacao { get; set; }
}
