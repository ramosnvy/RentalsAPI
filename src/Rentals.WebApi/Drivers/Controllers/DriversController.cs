using Microsoft.AspNetCore.Mvc;
using Rentals.Application.Commands;
using Rentals.WebApi.Drivers.Requests;

namespace Rentals.WebApi.Drivers.Controllers;

[ApiController]
[Route("entregadores")]
public class DriversController : ControllerBase
{
    private readonly RegisterDeliveryDriverHandler _registerDeliveryDriverHandler;
    private readonly UploadCnhImageDeliveryDriverHandler _uploadCnhImageDeliveryDriverHandler;
    
    public DriversController(RegisterDeliveryDriverHandler registerDeliveryDriverHandler, UploadCnhImageDeliveryDriverHandler uploadCnhImageDeliveryDriverHandler)
    {
        _registerDeliveryDriverHandler = registerDeliveryDriverHandler;
        _uploadCnhImageDeliveryDriverHandler =  uploadCnhImageDeliveryDriverHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDeliveryDriverRequest request)
    {
        try
        {
            var command = new RegisterDeliveryDriverCommand(
                Identifier: request.Identificador,
                Name: request.Nome,
                Cnpj: request.Cnpj,
                BirthDate: request.Data_Nascimento,
                CnhNumber: request.Numero_Cnh,
                CnhCategory: request.Tipo_Cnh,
                CnhImageBase64: request.Imagem_Cnh
            );
        
            await _registerDeliveryDriverHandler.Handle(command);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(new { mensagem = "Dados inválidos." });
        }
    }

    [HttpPost("{id}/cnh")]
    public async Task<IActionResult> UploadCnhImage(Guid id, [FromRoute] UploadCnhImageRequest request)
    {
        try
        {
            var command = new UploadCnhImageDeliveryDriverCommand(
                DriverId: id,
                CnhImageBase64: request.Imagem_Cnh
            );

            await _uploadCnhImageDeliveryDriverHandler.Handle(command);

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}