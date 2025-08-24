using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;

namespace Rentals.Application.Commands;

public class UploadCnhImageDeliveryDriverHandler
{
    private readonly IDeliveryDriverRepository _driverRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<UploadCnhImageDeliveryDriverHandler> _logger;
    private readonly IImageConverter _imageConverter;
    
    public UploadCnhImageDeliveryDriverHandler(
        IDeliveryDriverRepository driverRepository,
        IUserRepository userRepository,
        IStorageService storageService,
        ILogger<UploadCnhImageDeliveryDriverHandler> logger,
        IImageConverter imageConverter)
    {
        _driverRepository = driverRepository;
        _userRepository = userRepository;
        _storageService = storageService;
        _logger = logger;
        _imageConverter = imageConverter;
    }
    
    public async Task Handle(UploadCnhImageDeliveryDriverCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando upload da CNH para o usuário {UserId}", command.Id);

        try
        {
            // Primeiro, buscar o User pelo ID do JWT
            var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Usuário com ID {UserId} não encontrado", command.Id);
                throw new InvalidOperationException("Usuário não encontrado.");
            }

            // Verificar se o usuário é um DeliveryDriver
            if (user.Role != Domain.Users.UserRole.DeliveryDriver)
            {
                _logger.LogWarning("Usuário {UserId} não é um entregador (Role: {Role})", command.Id, user.Role);
                throw new InvalidOperationException("Usuário não é um entregador.");
            }

            // Buscar o DeliveryDriver relacionado ao User
            var driver = user.DeliveryDriver;
            if (driver is null)
            {
                _logger.LogWarning("Entregador não encontrado para o usuário {UserId}", command.Id);
                throw new InvalidOperationException("Entregador não encontrado.");
            }
            
            var fileName = $"cnh_{driver.Id}.bmp";

            var bmpBytes = _imageConverter.ConvertToBmp(Convert.FromBase64String(command.CnhImageBase64));
            
            await _storageService.UploadAsync(fileName, bmpBytes);

            var image = CnhImage.Create(fileName, "bmp");
            driver.UpdateCnhImage(image);

            await _driverRepository.UpdateAsync(driver);
                        
            _logger.LogInformation("Upload da CNH concluído com sucesso para o entregador {DriverId} (User: {UserId})", driver.Id, command.Id);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Erro ao processar imagem da CNH para o entregador {Id}: formato inválido", command.Id);
            throw new InvalidOperationException("Formato de imagem inválido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante upload da CNH para o entregador {Id}", command.Id);
            throw new InvalidOperationException("Erro interno ao processar upload da CNH.");
        }
    }
}