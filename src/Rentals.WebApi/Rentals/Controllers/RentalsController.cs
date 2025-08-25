using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rentals.Application.Commands;
using Rentals.Application.Queries;
using Rentals.Application.Services;
using Rentals.WebApi.Rentals.Requests;

namespace Rentals.WebApi.Rentals.Controllers;

[ApiController]
[Route("locacoes")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly CreateRentalHandler _createRentalHandler;
    private readonly ReturnRentalHandler _returnRentalHandler;
    private readonly GetRentalByIdHandler _getRentalByIdHandler;
    private readonly IRentalCalculationService _calculationService;
    private readonly ILogger<RentalsController> _logger;

    public RentalsController(
        CreateRentalHandler createRentalHandler,
        ReturnRentalHandler returnRentalHandler,
        GetRentalByIdHandler getRentalByIdHandler,
        IRentalCalculationService calculationService,
        ILogger<RentalsController> logger)
    {
        _createRentalHandler = createRentalHandler;
        _returnRentalHandler = returnRentalHandler;
        _getRentalByIdHandler = getRentalByIdHandler;
        _calculationService = calculationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentalRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando criação de locação para entregador {DeliveryDriverId} e moto {MotorcycleId}", 
                request.EntregadorId, request.MotoId);

            var result = await _createRentalHandler.Handle(
                request.EntregadorId,
                request.MotoId,
                request.PlanoId,
                request.DataInicio,
                CancellationToken.None);

            if (!result.Success)
            {
                _logger.LogWarning("Criação da locação falhou: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Locação criada com sucesso. ID: {RentalId}", result.RentalId);

            return CreatedAtAction(nameof(GetById), new { id = result.RentalId }, new
            {
                id = result.RentalId,
                valorTotal = result.TotalAmount,
                dataTerminoPrevista = result.ExpectedEndDate,
                message = "Locação criada com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a criação da locação");
            return StatusCode(500, new { error = "Erro interno do servidor durante a criação da locação" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            _logger.LogInformation("Buscando locação {RentalId}", id);
            var result = await _getRentalByIdHandler.Handle(id, CancellationToken.None);

            if (!result.Success)
            {
                _logger.LogWarning("Busca da locação falhou: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Locação {RentalId} encontrada com sucesso", id);

            return Ok(new
            {
                success = true,
                locacao = result.Locacao
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a busca da locação {RentalId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor durante a busca da locação" });
        }
    }

    [HttpPost("{id}/devolver")]
    public async Task<IActionResult> Return(long id, [FromBody] ReturnRentalRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando devolução da locação {RentalId} para {DataDevolucao}", id, request.DataDevolucao);

            var result = await _returnRentalHandler.Handle(id, request.DataDevolucao, CancellationToken.None);

            if (!result.Success)
            {
                _logger.LogWarning("Devolução da locação falhou: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Locação {RentalId} devolvida com sucesso. Valor final: {ValorFinal}", id, result.ValorFinal);

            return Ok(new
            {
                valorFinal = result.ValorFinal,
                dataDevolucao = result.DataDevolucao,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a devolução da locação {RentalId}", id);
            return StatusCode(500, new { error = "Erro interno do servidor durante a devolução da locação" });
        }
    }

    [HttpPost("calcular")]
    public async Task<IActionResult> CalculateRentalValue([FromBody] CalculateRentalValueRequest request)
    {
        try
        {
            _logger.LogInformation("Calculando valor da locação para plano {PlanoId} de {DataInicio} até {DataTermino}", 
                request.PlanoId, request.DataInicio, request.DataTermino);

            // Buscar o plano
            var plan = await _calculationService.GetRentalPlanById(request.PlanoId);
            if (plan == null)
            {
                return BadRequest(new { error = "Plano de locação não encontrado" });
            }

            // Calcular valor total
            var valorTotal = _calculationService.CalculateTotalAmount(plan, request.DataInicio, request.DataTermino);

            _logger.LogInformation("Valor calculado: R$ {ValorTotal} para {Dias} dias", valorTotal, 
                (request.DataTermino - request.DataInicio).Days);

            return Ok(new
            {
                valorTotal = valorTotal,
                dias = (request.DataTermino - request.DataInicio).Days,
                taxaDiaria = plan.DailyRate,
                plano = plan.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o cálculo do valor da locação");
            return StatusCode(500, new { error = "Erro interno do servidor durante o cálculo" });
        }
    }
}
