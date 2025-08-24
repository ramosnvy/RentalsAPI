using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;
using Rentals.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Rentals.Application.Commands;

public class RegisterDeliveryDriverHandler
{
    private readonly IDeliveryDriverRepository _driverRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly ILogger<RegisterDeliveryDriverHandler> _logger;
    
    public RegisterDeliveryDriverHandler(
        IDeliveryDriverRepository driverRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        ILogger<RegisterDeliveryDriverHandler> logger)
    {
        _driverRepository = driverRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _logger = logger;
    }

    public async Task<RegisterDeliveryDriverResult> Handle(
        string identifier,
        string email,
        string password,
        string nome,
        string cnpj,
        DateTime dataNascimento,
        string numeroCnh,
        string tipoCnh,
        string? imagemCnh = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting registration process for driver {Identifier}", identifier);

        // Verificar se identifier já existe
        if (await _userRepository.UsernameExistsAsync(identifier, cancellationToken))
        {
            _logger.LogWarning("Registration failed: Identifier {Identifier} already exists", identifier);
            return new RegisterDeliveryDriverResult
            {
                Success = false,
                ErrorMessage = "Identificador já está em uso"
            };
        }

        // Verificar se email já existe
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", email);
            return new RegisterDeliveryDriverResult
            {
                Success = false,
                ErrorMessage = "Email já está em uso"
            };
        }

        // Verificar se CNPJ já existe
        if (await _driverRepository.ExistsByCnpjAsync(cnpj))
        {
            _logger.LogWarning("Registration failed: CNPJ {Cnpj} already exists", cnpj);
            return new RegisterDeliveryDriverResult
            {
                Success = false,
                ErrorMessage = "CNPJ já está em uso"
            };
        }

        // Verificar se CNH já existe
        if (await _driverRepository.ExistsByCnhNumberAsync(numeroCnh))
        {
            _logger.LogWarning("Registration failed: CNH number {CnhNumber} already exists", numeroCnh);
            return new RegisterDeliveryDriverResult
            {
                Success = false,
                ErrorMessage = "CNH já está em uso"
            };
        }
        
        var cnpjValue = Cnpj.Create(cnpj);
        var cnhValue = Cnh.Create(numeroCnh, tipoCnh);

        var driver = DeliveryDriver.Register(
            nome,
            cnpjValue,
            dataNascimento,
            cnhValue);

        if (!string.IsNullOrWhiteSpace(imagemCnh))
        {
            _logger.LogInformation("Driver {Identifier} provided an initial CNH image", identifier);
            var image = CnhImage.Create("cnh_" + identifier + ".png", "png");
            driver.UpdateCnhImage(image);
        }

        // Criar hash da senha
        var passwordHash = _passwordHasher.HashPassword(password);

        // Criar usuário
        var user = User.CreateDeliveryDriver(
            identifier,
            email,
            passwordHash,
            driver);

        // Salvar no banco
        await _userRepository.AddAsync(user, cancellationToken);
        _logger.LogInformation("Driver {Identifier} successfully registered", identifier);

        // Gerar token JWT
        var token = _jwtProvider.GenerateToken(
            user.Id.ToString(),
            user.Username,
            user.Role.ToString());

        return new RegisterDeliveryDriverResult
        {
            Success = true,
            UserId = user.Id,
            Token = token
        };
    }
}

public class RegisterDeliveryDriverResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long? UserId { get; set; }
    public string? Token { get; set; }
}
