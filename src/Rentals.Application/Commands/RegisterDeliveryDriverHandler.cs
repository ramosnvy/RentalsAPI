using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;
using Microsoft.Extensions.Logging;

namespace Rentals.Application.Commands;

public class RegisterDeliveryDriverHandler
{
    private readonly IDeliveryDriverRepository _repository;
    private readonly IJwtProvider _jwtprovider;
    private readonly ILogger<RegisterDeliveryDriverHandler> _logger;
    
    public RegisterDeliveryDriverHandler(
        IDeliveryDriverRepository repository, 
        IJwtProvider jwtProvider,
        ILogger<RegisterDeliveryDriverHandler> logger)
    {
        _repository = repository;
        _jwtprovider = jwtProvider;
        _logger = logger;
    }

    public async Task<string> Handle(RegisterDeliveryDriverCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting registration process for driver {Identifier}", command.Identifier);

        if (await _repository.ExistsByCnpjAsync(command.Cnpj))
        {
            _logger.LogWarning("Registration failed: CNPJ {Cnpj} already exists", command.Cnpj);
            throw new Exception("Cnpj already exists");
        }

        if (await _repository.ExistsByCnhNumberAsync(command.CnhNumber))
        {
            _logger.LogWarning("Registration failed: CNH number {CnhNumber} already exists", command.CnhNumber);
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

        if (!string.IsNullOrWhiteSpace(command.CnhImageBase64))
        {
            _logger.LogInformation("Driver {Identifier} provided an initial CNH image", command.Identifier);
            var image = CnhImage.Create("cnh_" + driver.Identifier + ".png", "png");
            driver.UpdateCnhImage(image);
        }

        await _repository.AddAsync(driver);
        _logger.LogInformation("Driver {Identifier} successfully registered", driver.Identifier);

        var token = _jwtprovider.GenerateToken(driver.Id, driver.Identifier, "Driver");
        _logger.LogInformation("JWT successfully generated for driver {Identifier}", driver.Identifier);

        return token;
    }
}
