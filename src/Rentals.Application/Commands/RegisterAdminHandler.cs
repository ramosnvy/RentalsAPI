using Rentals.Application.Abstractions;
using Rentals.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Rentals.Application.Commands;

public class RegisterAdminHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly ILogger<RegisterAdminHandler> _logger;

    public RegisterAdminHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        ILogger<RegisterAdminHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _logger = logger;
    }

    public async Task<RegisterAdminResult> Handle(
        string identifier,
        string email,
        string password,
        string nome,
        string? telefone,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processo de registro de administrador {Identifier}", identifier);

        // Verificar se identifier já existe
        if (await _userRepository.UsernameExistsAsync(identifier, cancellationToken))
        {
            _logger.LogWarning("Registro de admin falhou: identificador {Identifier} já está em uso", identifier);
            return new RegisterAdminResult
            {
                Success = false,
                ErrorMessage = "Identificador já está em uso"
            };
        }

        // Verificar se email já existe
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            _logger.LogWarning("Registro de admin falhou: email {Email} já está em uso", email);
            return new RegisterAdminResult
            {
                Success = false,
                ErrorMessage = "Email já está em uso"
            };
        }

        // Criar admin
        var admin = Admin.Create(nome, telefone);

        // Criar hash da senha
        var passwordHash = _passwordHasher.HashPassword(password);

        // Criar usuário
        var user = User.CreateAdmin(
            identifier,
            email,
            passwordHash);

        // Salvar no banco
        await _userRepository.AddAsync(user, cancellationToken);

        // Gerar token JWT
        var token = _jwtProvider.GenerateToken(
            user.Id.ToString(),
            user.Username,
            user.Role.ToString());

        _logger.LogInformation("Administrador {Identifier} registrado com sucesso", identifier);

        return new RegisterAdminResult
        {
            Success = true,
            UserId = user.Id,
            Token = token
        };
    }
}

public class RegisterAdminResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long? UserId { get; set; }
    public string? Token { get; set; }
}
