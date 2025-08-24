using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;

namespace Rentals.Application.Commands;

public class UploadCnhImageDeliveryDriverHandler
{
    private readonly IDeliveryDriverRepository _repository;
    private readonly IStorageService _storageService;
    private readonly ILogger<UploadCnhImageDeliveryDriverHandler> _logger;
    private readonly IImageConverter _imageConverter;
    public UploadCnhImageDeliveryDriverHandler(
        IDeliveryDriverRepository repository, 
        IStorageService storageService,
        ILogger<UploadCnhImageDeliveryDriverHandler> logger,
        IImageConverter imageConverter)
    {
        _repository = repository;
        _storageService = storageService;
        _logger = logger;
        _imageConverter = imageConverter;
    }
    
    public async Task Handle(UploadCnhImageDeliveryDriverCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting CNH upload for driver Id {Id}", command.Id);

        var driver = await _repository.GetByIdAsync(command.Id);
        if (driver is null)
        {
            _logger.LogWarning("Driver with identifier Id {Identifier} not found", command.Id);
            throw new InvalidOperationException("Driver not found.");
        }
        
        var fileName = $"cnh_{driver.Identifier}_{Guid.NewGuid()}.bmp";

        var bmpBytes = _imageConverter.ConvertToBmp(Convert.FromBase64String(command.CnhImageBase64));
        
            await _storageService.UploadAsync(fileName, bmpBytes);

        var image = CnhImage.Create(fileName, "bmp");
        driver.UpdateCnhImage(image);

        await _repository.UpdateAsync(driver);
                    

        _logger.LogInformation("CNH upload completed successfully for driver {Identifier}", driver.Identifier);
    }
}