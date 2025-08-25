using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Domain.Vehicles;
using Rentals.Domain.Vehicles.Events;

namespace Rentals.Application.Commands;

public class RegisterMotorcycleHandler
{
    private readonly IMotorcycleRepository _motorcycleRepository;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<RegisterMotorcycleHandler> _logger;

    public RegisterMotorcycleHandler(
        IMotorcycleRepository motorcycleRepository,
        IMessageBus messageBus,
        ILogger<RegisterMotorcycleHandler> logger)
    {
        _motorcycleRepository = motorcycleRepository;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<RegisterMotorcycleResult> Handle(
        string identifier,
        int year,
        string model,
        string licensePlate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando cadastro de moto {Identifier}", identifier);

        try
        {
            // Verificar se a placa já existe
            if (await _motorcycleRepository.LicensePlateExistsAsync(licensePlate, cancellationToken))
            {
                _logger.LogWarning("Cadastro de moto falhou: placa {LicensePlate} já está em uso", licensePlate);
                return new RegisterMotorcycleResult
                {
                    Success = false,
                    ErrorMessage = "Placa já está em uso"
                };
            }

            // Criar a moto
            var motorcycle = Motorcycle.Create(identifier, year, model, licensePlate);

            // Salvar no repositório
            await _motorcycleRepository.AddAsync(motorcycle, cancellationToken);

            // Publicar eventos de domínio
            foreach (var domainEvent in motorcycle.DomainEvents)
            {
                if (domainEvent is MotorcycleCreatedEvent motorcycleEvent)
                {
                    var message = new MotorcycleCreatedMessage
                    {
                        MotorcycleId = motorcycleEvent.MotorcycleId,
                        Identifier = motorcycleEvent.Identifier,
                        Year = motorcycleEvent.Year,
                        Model = motorcycleEvent.Model,
                        LicensePlate = motorcycleEvent.LicensePlate,
                        CreatedAt = motorcycleEvent.CreatedAt
                    };

                    await _messageBus.PublishAsync(message, "motorcycle.created", cancellationToken);
                }
            }

            // Limpar eventos de domínio
            motorcycle.ClearDomainEvents();

            _logger.LogInformation("Moto {Identifier} cadastrada com sucesso", identifier);

            return new RegisterMotorcycleResult
            {
                Success = true,
                MotorcycleId = motorcycle.Id
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Cadastro de moto falhou: {ErrorMessage}", ex.Message);
            return new RegisterMotorcycleResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante cadastro da moto {Identifier}", identifier);
            return new RegisterMotorcycleResult
            {
                Success = false,
                ErrorMessage = "Erro interno ao cadastrar moto"
            };
        }
    }
}

public class RegisterMotorcycleResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long? MotorcycleId { get; set; }
}

public class MotorcycleCreatedMessage
{
    public long MotorcycleId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
