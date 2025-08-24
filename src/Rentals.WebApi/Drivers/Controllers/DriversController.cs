using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rentals.Application.Commands;
using Rentals.WebApi.Drivers.Requests;
using Microsoft.Extensions.Logging;

namespace Rentals.WebApi.Drivers.Controllers;

[ApiController]
[Route("entregadores")]
public class DriversController : ControllerBase
{
    private readonly RegisterDeliveryDriverHandler _registerDriverHandler;
    private readonly UploadCnhImageDeliveryDriverHandler _uploadCnhImageDeliveryDriverHandler;
    private readonly GetAllDeliveryDriversHandler _getAllDriversHandler;
    private readonly ILogger<DriversController> _logger;

    public DriversController(
        RegisterDeliveryDriverHandler registerDriverHandler,
        UploadCnhImageDeliveryDriverHandler uploadCnhImageDeliveryDriverHandler,
        GetAllDeliveryDriversHandler getAllDriversHandler,
        ILogger<DriversController> logger)
    {
        _registerDriverHandler = registerDriverHandler;
        _uploadCnhImageDeliveryDriverHandler = uploadCnhImageDeliveryDriverHandler;
        _getAllDriversHandler = getAllDriversHandler;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            _logger.LogInformation("Fetching all delivery drivers");

            var result = await _getAllDriversHandler.Handle();

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("Successfully fetched {Count} delivery drivers", result.TotalCount);

            return Ok(new
            {
                drivers = result.Drivers,
                totalCount = result.TotalCount
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar entregadores");
            return StatusCode(500, new { message = "Erro interno do servidor ao buscar entregadores." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDeliveryDriverRequest request)
    {
        try
        {
            _logger.LogInformation("Starting registration for driver {Identifier}", request.Identifier);

            var result = await _registerDriverHandler.Handle(
                request.Identifier,
                request.Email,
                request.Password,
                request.Nome,
                request.Cnpj,
                request.Data_Nascimento,
                request.Numero_Cnh,
                request.Tipo_Cnh,
                request.Imagem_Cnh
            );

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            _logger.LogInformation("Driver {Identifier} successfully registered", request.Identifier);

            return CreatedAtAction(nameof(Register), new { id = result.UserId }, new
            {
                id = result.UserId,
                token = result.Token
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao registrar entregador {Identifier}", request.Identifier);
            return BadRequest(new { message = "Dados inválidos para registro." });
        }
    }

    [Authorize(Roles = "DeliveryDriver")]
    [HttpPost("{userId}/cnh")]
    public async Task<IActionResult> UploadCnhImage(long userId, [FromBody] UploadCnhImageRequest request)
    {
        try
        {
            _logger.LogInformation("Upload CNH request received for user {UserId}", userId);
            
            // Log all claims for debugging
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            _logger.LogInformation("JWT Claims: {Claims}", string.Join(", ", claims));
            
            var jwtIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("JWT Identifier from NameIdentifier claim: {JwtIdentifier}", jwtIdentifier);

            if (jwtIdentifier != userId.ToString())
            {
                _logger.LogWarning("Unauthorized CNH upload attempt. Token identifier={JwtIdentifier}, Route identifier={RouteIdentifier}", jwtIdentifier, userId.ToString());
                return Forbid();
            }

            _logger.LogInformation("Starting CNH upload for user {UserId}", userId.ToString());

            var command = new UploadCnhImageDeliveryDriverCommand(
                Id: userId,
                CnhImageBase64: request.Imagem_Cnh
            );

            await _uploadCnhImageDeliveryDriverHandler.Handle(command);

            _logger.LogInformation("CNH upload completed successfully for user {UserId}", userId.ToString());

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload da CNH para o usuário {UserId}", userId.ToString());
            return BadRequest(new { error = "Erro interno ao processar upload da CNH." });
        }
    }
}
