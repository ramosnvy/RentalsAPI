using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;

namespace Rentals.Application.Commands;

public class Motorcycle2024NotificationHandler
{
    private readonly IMotorcycleNotificationRepository _notificationRepository;
    private readonly ILogger<Motorcycle2024NotificationHandler> _logger;

    public Motorcycle2024NotificationHandler(
        IMotorcycleNotificationRepository notificationRepository,
        ILogger<Motorcycle2024NotificationHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task Handle(MotorcycleCreatedMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processando notificação para moto {MotorcycleId}", message.MotorcycleId);

        try
        {
            // Verificar se é uma moto de 2024
            if (message.Year == 2024)
            {
                _logger.LogInformation("Moto {MotorcycleId} é de 2024. Criando notificação.", message.MotorcycleId);

                var notification = new MotorcycleNotification
                {
                    MotorcycleId = message.MotorcycleId,
                    Identifier = message.Identifier,
                    Year = message.Year,
                    Model = message.Model,
                    LicensePlate = message.LicensePlate,
                    NotificationType = "2024_MODEL",
                    Message = $"Nova moto de 2024 cadastrada: {message.Model} - Placa: {message.LicensePlate}",
                    CreatedAt = DateTime.UtcNow
                };

                // Salvar no MongoDB (apenas notificações de motos de 2024)
                await _notificationRepository.AddAsync(notification, cancellationToken);

                _logger.LogInformation("Notificação para moto de 2024 {MotorcycleId} salva no MongoDB com sucesso", message.MotorcycleId);
            }
            else
            {
                _logger.LogInformation("Moto {MotorcycleId} não é de 2024 (Ano: {Year}). Ignorando notificação.", message.MotorcycleId, message.Year);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificação para moto {MotorcycleId}", message.MotorcycleId);
            throw;
        }
    }
}

public class MotorcycleNotification
{
    public long Id { get; set; }
    public long MotorcycleId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
