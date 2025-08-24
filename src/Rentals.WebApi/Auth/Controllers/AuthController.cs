using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rentals.Application.Commands;
using Rentals.WebApi.Auth.Requests;

namespace Rentals.WebApi.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginHandler _loginHandler;
    private readonly RegisterAdminHandler _registerAdminHandler;
    private readonly RegisterDeliveryDriverHandler _registerDriverHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        LoginHandler loginHandler,
        RegisterAdminHandler registerAdminHandler,
        RegisterDeliveryDriverHandler registerDriverHandler,
        ILogger<AuthController> logger)
    {
        _loginHandler = loginHandler;
        _registerAdminHandler = registerAdminHandler;
        _registerDriverHandler = registerDriverHandler;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando processo de login para o usuário {Identifier}", request.Identifier);

            var result = await _loginHandler.Handle(request.Identifier, request.Password);

            if (!result.Success)
            {
                _logger.LogWarning("Login falhou para o usuário {Identifier}: {ErrorMessage}", request.Identifier, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Login realizado com sucesso para o usuário {Identifier}", request.Identifier);

            return Ok(new
            {
                id = result.UserId,
                token = result.Token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o processo de login para o usuário {Identifier}", request.Identifier);
            return StatusCode(500, new { error = "Erro interno do servidor durante o login" });
        }
    }

    [HttpPost("registrar/admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando registro de administrador {Identifier}", request.Identifier);

            var result = await _registerAdminHandler.Handle(
                request.Identifier,
                request.Email,
                request.Password,
                request.Nome,
                request.Telefone);

            if (!result.Success)
            {
                _logger.LogWarning("Registro de admin falhou para {Identifier}: {ErrorMessage}", request.Identifier, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Administrador {Identifier} registrado com sucesso", request.Identifier);

            return CreatedAtAction(nameof(RegisterAdmin), new { id = result.UserId }, new
            {
                id = result.UserId,
                token = result.Token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o registro de administrador {Identifier}", request.Identifier);
            return StatusCode(500, new { error = "Erro interno do servidor durante o registro" });
        }
    }

    [HttpPost("registrar/entregador")]
    public async Task<IActionResult> RegisterDriver([FromBody] RegisterDriverRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando registro de entregador {Identifier}", request.Identifier);

            var result = await _registerDriverHandler.Handle(
                request.Identifier,
                request.Email,
                request.Password,
                request.Nome,
                request.Cnpj,
                request.Data_Nascimento,
                request.Numero_Cnh,
                request.Tipo_Cnh,
                request.Imagem_Cnh);

            if (!result.Success)
            {
                _logger.LogWarning("Registro de entregador falhou para {Identifier}: {ErrorMessage}", request.Identifier, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Entregador {Identifier} registrado com sucesso", request.Identifier);

            return CreatedAtAction(nameof(RegisterDriver), new { id = result.UserId }, new
            {
                id = result.UserId,
                token = result.Token
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o registro de entregador {Identifier}", request.Identifier);
            return StatusCode(500, new { error = "Erro interno do servidor durante o registro" });
        }
    }
}
