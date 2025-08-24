using Rentals.Application.Abstractions;
using Rentals.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Rentals.Application.Commands;

public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        ILogger<LoginHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(string identifier, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processo de login para o usuário {Identifier}", identifier);

        var user = await _userRepository.GetByUsernameAsync(identifier, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("Tentativa de login falhou: usuário {Identifier} não encontrado", identifier);
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Usuário ou senha inválidos"
            };
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Tentativa de login falhou: usuário {Identifier} está inativo", identifier);
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Usuário inativo"
            };
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Tentativa de login falhou: senha incorreta para o usuário {Identifier}", identifier);
            return new LoginResult
            {
                Success = false,
                ErrorMessage = "Usuário ou senha inválidos"
            };
        }

        // Update last login
        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        var token = _jwtProvider.GenerateToken(
            user.Id.ToString(), 
            user.Username, 
            user.Role.ToString());

        _logger.LogInformation("Login realizado com sucesso para o usuário {Identifier} com role {Role}", identifier, user.Role);

        return new LoginResult
        {
            Success = true,
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
    public long? UserId { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
}
