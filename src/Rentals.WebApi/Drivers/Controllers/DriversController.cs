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
    private readonly RegisterDeliveryDriverHandler _registerDeliveryDriverHandler;
    private readonly UploadCnhImageDeliveryDriverHandler _uploadCnhImageDeliveryDriverHandler;
    private readonly ILogger<DriversController> _logger;

    public DriversController(
        RegisterDeliveryDriverHandler registerDeliveryDriverHandler,
        UploadCnhImageDeliveryDriverHandler uploadCnhImageDeliveryDriverHandler,
        ILogger<DriversController> logger)
    {
        _registerDeliveryDriverHandler = registerDeliveryDriverHandler;
        _uploadCnhImageDeliveryDriverHandler = uploadCnhImageDeliveryDriverHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDeliveryDriverRequest request)
    {
        try
        {
            _logger.LogInformation("Starting registration for driver {Identifier}", request.Identificador);

            var command = new RegisterDeliveryDriverCommand(
                Identifier: request.Identificador,
                Name: request.Nome,
                Cnpj: request.Cnpj,
                BirthDate: request.Data_Nascimento,
                CnhNumber: request.Numero_Cnh,
                CnhCategory: request.Tipo_Cnh,
                CnhImageBase64: request.Imagem_Cnh
            );

            var token = await _registerDeliveryDriverHandler.Handle(command);

            _logger.LogInformation("Driver {Identifier} successfully registered", request.Identificador);

            return Ok(new { token });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while registering driver {Identifier}", request.Identificador);
            return BadRequest(new { message = "Invalid data." });
        }
    }

    [Authorize(Roles = "Driver")]
    [HttpPost("{id}/cnh")]
    public async Task<IActionResult> UploadCnhImage(long id, [FromBody] UploadCnhImageRequest request)
    {
        try
        {
            var jwtIdentifier = User.FindFirstValue("id");

            if (jwtIdentifier != id.ToString())
            {
                _logger.LogWarning("Unauthorized CNH upload attempt. Token identifier={JwtIdentifier}, Route identifier={RouteIdentifier}", jwtIdentifier, id.ToString());
                return Forbid();
            }

            _logger.LogInformation("Starting CNH upload for driver {Identifier}", id.ToString());

            var command = new UploadCnhImageDeliveryDriverCommand(
                Id: id,
                CnhImageBase64: request.Imagem_Cnh
            );

            await _uploadCnhImageDeliveryDriverHandler.Handle(command);

            _logger.LogInformation("CNH upload completed successfully for driver {Identifier}", id.ToString());

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while uploading CNH for driver {Identifier}", id.ToString());
            return BadRequest(new { error = ex.Message });
        }
    }
}
