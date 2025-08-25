using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rentals.Application.Commands;
using Rentals.Application.Queries;
using Rentals.WebApi.Vehicles.Requests;

namespace Rentals.WebApi.Vehicles.Controllers;

[ApiController]
[Route("motos")]
[Authorize(Roles = "Admin")]
public class MotorcyclesController : ControllerBase
{
    private readonly RegisterMotorcycleHandler _registerMotorcycleHandler;
    private readonly GetAllMotorcyclesHandler _getAllMotorcyclesHandler;
    private readonly GetMotorcycleByIdHandler _getMotorcycleByIdHandler;
    private readonly GetMotorcycles2024Handler _getMotorcycles2024Handler;
    private readonly UpdateMotorcycleLicensePlateHandler _updateMotorcycleLicensePlateHandler;
    private readonly DeleteMotorcycleHandler _deleteMotorcycleHandler;
    private readonly ILogger<MotorcyclesController> _logger;

    public MotorcyclesController(
        RegisterMotorcycleHandler registerMotorcycleHandler,
        GetAllMotorcyclesHandler getAllMotorcyclesHandler,
        GetMotorcycleByIdHandler getMotorcycleByIdHandler,
        GetMotorcycles2024Handler getMotorcycles2024Handler,
        UpdateMotorcycleLicensePlateHandler updateMotorcycleLicensePlateHandler,
        DeleteMotorcycleHandler deleteMotorcycleHandler,
        ILogger<MotorcyclesController> logger)
    {
        _registerMotorcycleHandler = registerMotorcycleHandler;
        _getAllMotorcyclesHandler = getAllMotorcyclesHandler;
        _getMotorcycleByIdHandler = getMotorcycleByIdHandler;
        _getMotorcycles2024Handler = getMotorcycles2024Handler;
        _updateMotorcycleLicensePlateHandler = updateMotorcycleLicensePlateHandler;
        _deleteMotorcycleHandler = deleteMotorcycleHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterMotorcycleRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando cadastro de moto {Identificador}", request.Identificador);

            var result = await _registerMotorcycleHandler.Handle(
                request.Identificador,
                request.Ano,
                request.Modelo,
                request.Placa);

            if (!result.Success)
            {
                _logger.LogWarning("Cadastro de moto falhou para {Identificador}: {ErrorMessage}",
                    request.Identificador, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Moto {Identificador} cadastrada com sucesso", request.Identificador);

            return CreatedAtAction(nameof(Register), new { id = result.MotorcycleId }, new
            {
                id = result.MotorcycleId,
                message = "Moto cadastrada com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o cadastro da moto {Identificador}", request.Identificador);
            return StatusCode(500, new { error = "Erro interno do servidor durante o cadastro" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? placa = null)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(placa))
            {
                _logger.LogInformation("Iniciando busca de motos com filtro por placa: {Placa}", placa);
            }
            else
            {
                _logger.LogInformation("Iniciando busca de todas as motos cadastradas");
            }

            var result = await _getAllMotorcyclesHandler.Handle(placa);

            if (!result.Success)
            {
                _logger.LogWarning("Busca de motos falhou: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            if (!string.IsNullOrWhiteSpace(placa))
            {
                _logger.LogInformation("Busca de motos por placa concluída com sucesso. {Count} motos encontradas para placa {Placa}",
                    result.Motorcycles.Count, placa);
                return Ok(new
                {
                    motos = result.Motorcycles,
                    total = result.Motorcycles.Count,
                    placa_filtro = placa,
                    message = result.Motorcycles.Count > 0 
                        ? $"Moto(s) encontrada(s) com sucesso para placa {placa}" 
                        : $"Nenhuma moto encontrada para placa {placa}"
                });
            }
            else
            {
                _logger.LogInformation("Busca de motos concluída com sucesso. {Count} motos encontradas",
                    result.Motorcycles.Count);
                return Ok(new
                {
                    motos = result.Motorcycles,
                    total = result.Motorcycles.Count,
                    message = "Motos encontradas com sucesso"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a busca de motos");
            return StatusCode(500, new { error = "Erro interno do servidor durante a busca" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            _logger.LogInformation("Iniciando busca de moto por ID: {Id}", id);
            var result = await _getMotorcycleByIdHandler.Handle(id);

            if (!result.Success)
            {
                _logger.LogWarning("Busca de moto por ID falhou: {ErrorMessage}", result.ErrorMessage);
                return NotFound(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Moto com ID {Id} encontrada com sucesso", id);
            return Ok(new
            {
                moto = result.Motorcycle,
                message = "Moto encontrada com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a busca de moto por ID {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor durante a busca" });
        }
    }

    [HttpPut("{id}/placa")]
    public async Task<IActionResult> UpdateLicensePlate(long id, [FromBody] UpdateMotorcycleLicensePlateRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando atualização de placa para moto ID: {Id}, Nova placa: {NovaPlaca}", id, request.NovaPlaca);
            var result = await _updateMotorcycleLicensePlateHandler.Handle(id, request.NovaPlaca);

            if (!result.Success)
            {
                _logger.LogWarning("Atualização de placa falhou para moto ID {Id}: {ErrorMessage}", id, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Placa da moto ID {Id} atualizada com sucesso", id);
            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a atualização de placa da moto ID {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor durante a atualização" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            _logger.LogInformation("Iniciando exclusão da moto ID: {Id}", id);
            var result = await _deleteMotorcycleHandler.Handle(id);

            if (!result.Success)
            {
                _logger.LogWarning("Exclusão de moto falhou para ID {Id}: {ErrorMessage}", id, result.ErrorMessage);
                return NotFound(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Moto ID {Id} deletada com sucesso", id);
            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a exclusão da moto ID {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor durante a exclusão" });
        }
    }

    [HttpGet("2024")]
    public async Task<IActionResult> Get2024Motorcycles()
    {
        try
        {
            _logger.LogInformation("Iniciando busca de motos de 2024");
            var result = await _getMotorcycles2024Handler.Handle();

            if (!result.Success)
            {
                _logger.LogWarning("Busca de motos de 2024 falhou: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Busca de motos de 2024 concluída com sucesso. {Count} motos encontradas", result.Motorcycles.Count);
            return Ok(new
            {
                motos = result.Motorcycles,
                total = result.Motorcycles.Count,
                message = "Motos de 2024 encontradas com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a busca de motos de 2024");
            return StatusCode(500, new { error = "Erro interno do servidor durante a busca" });
        }
    }
}