using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;

namespace Rentals.Application.Commands;

public class UploadCnhImageDeliveryDriverHandler
{
    private readonly IDeliveryDriverRepository _repository;
    private readonly IStorageService _storageService;
    
    public UploadCnhImageDeliveryDriverHandler(IDeliveryDriverRepository repository, IStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
    }
    
    public async Task Handle(UploadCnhImageDeliveryDriverCommand command, CancellationToken cancellationToken = default)
    {
        var driver = await _repository.GetByIdAsync(command.DriverId);
        if (driver is null)
            throw new InvalidOperationException("Driver not found.");

        var fileName = $"cnh_{driver.Identifier}_{Guid.NewGuid()}.png";
        await _storageService.UploadAsync(fileName, Convert.FromBase64String(command.CnhImageBase64));

        var image = CnhImage.Create(fileName, "png");
        driver.UpdateCnhImage(image);

        await _repository.UpdateAsync(driver);
    }
}