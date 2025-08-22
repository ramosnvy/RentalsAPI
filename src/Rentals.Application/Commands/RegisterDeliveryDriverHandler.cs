using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;

namespace Rentals.Application.Commands;

public class RegisterDeliveryDriverHandler
{
    private readonly IDeliveryDriverRepository _repository;

    public RegisterDeliveryDriverHandler(IDeliveryDriverRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(RegisterDeliveryDriverCommand command, CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsByCnpjAsync(command.Cnpj))
        {
            throw new Exception("Cnpj already exists");
        }

        if (await _repository.ExistsByCnhNumberAsync(command.CnhNumber))
        {
            throw new Exception("CnhNumber already exists");
        }
        
        var cnpj = Cnpj.Create(command.Cnpj);
        var cnh = Cnh.Create(command.CnhNumber, command.CnhCategory);

        var driver = DeliveryDriver.Register(
            command.Identifier,
            command.Name,
            cnpj,
            command.BirthDate,
            cnh);

        if (!string.IsNullOrWhiteSpace((command.CnhImageBase64)))
        {
            var image = CnhImage.Create("cnh_" + driver.Identifier + ".png", "png");
            driver.UpdateCnhImage(image);
        }
        await _repository.AddAsync(driver);
        return driver.Id;
    }
}