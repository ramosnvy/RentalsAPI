using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rentals.Application.Commands;
using Rentals.Application.Queries;
using Rentals.WebApi.Rentals.Requests;

namespace Rentals.WebApi.Rentals.Controllers;

[ApiController]
[Route("planos")]
[Authorize(Roles = "Admin")]
public class RentalPlansController : ControllerBase
{
    private readonly CreateRentalPlanHandler _createRentalPlanHandler;
    private readonly GetAllRentalPlansHandler _getAllRentalPlansHandler;
    private readonly ILogger<RentalPlansController> _logger;

    public RentalPlansController(
        CreateRentalPlanHandler createRentalPlanHandler,
        GetAllRentalPlansHandler getAllRentalPlansHandler,
        ILogger<RentalPlansController> logger)
    {
        _createRentalPlanHandler = createRentalPlanHandler;
        _getAllRentalPlansHandler = getAllRentalPlansHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentalPlanRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando criação do plano de locação {Nome}", request.Nome);

            var result = await _createRentalPlanHandler.Handle(
                request.Nome,
                request.DuracaoDias,
                request.TaxaDiaria,
                request.PercentualMultaAntecipada,
                request.TaxaAtrasoDiaria,
                CancellationToken.None);

            if (!result.Success)
            {
                _logger.LogWarning("Criação do plano falhou: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Plano de locação {Nome} criado com sucesso", request.Nome);

            return CreatedAtAction(nameof(Create), new { id = result.RentalPlanId }, new
            {
                id = result.RentalPlanId,
                message = "Plano de locação criado com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a criação do plano {Nome}", request.Nome);
            return StatusCode(500, new { error = "Erro interno do servidor durante a criação do plano" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            _logger.LogInformation("Iniciando busca de todos os planos de locação");
            var result = await _getAllRentalPlansHandler.Handle(CancellationToken.None);

            if (!result.Success)
            {
                _logger.LogWarning("Busca de planos falhou: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Busca de planos concluída com sucesso. {Count} planos encontrados",
                result.RentalPlans.Count);

            return Ok(new
            {
                success = true,
                planos = result.RentalPlans
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a busca de planos");
            return StatusCode(500, new { error = "Erro interno do servidor durante a busca de planos" });
        }
    }
}
