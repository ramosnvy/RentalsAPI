using Rentals.Application.Abstractions;
using Rentals.Domain.Drivers;
using Microsoft.Extensions.Logging;

namespace Rentals.Application.Commands;

public class GetAllDeliveryDriversHandler
{
    private readonly IDeliveryDriverRepository _driverRepository;
    private readonly ILogger<GetAllDeliveryDriversHandler> _logger;

    public GetAllDeliveryDriversHandler(
        IDeliveryDriverRepository driverRepository,
        ILogger<GetAllDeliveryDriversHandler> logger)
    {
        _driverRepository = driverRepository;
        _logger = logger;
    }

    public async Task<GetAllDeliveryDriversResult> Handle(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando busca de todos os entregadores");

        try
        {
            var drivers = await _driverRepository.GetAllAsync(cancellationToken);

            var driverDtos = drivers.Select(driver => new DeliveryDriverDto
            {
                Id = driver.Id,
                Nome = driver.Name,
                Cnpj = driver.Cnpj.Value,
                Data_Nascimento = driver.BirthDate,
                Numero_Cnh = driver.Cnh.Number,
                Tipo_Cnh = driver.Cnh.CnhCategory.ToString(),
                Possui_Imagem_Cnh = driver.CnhImage != null
            }).ToList();

            _logger.LogInformation("Busca concluída com sucesso. Total de entregadores: {Count}", driverDtos.Count);

            return new GetAllDeliveryDriversResult
            {
                Success = true,
                Drivers = driverDtos,
                TotalCount = driverDtos.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar entregadores");
            return new GetAllDeliveryDriversResult
            {
                Success = false,
                ErrorMessage = "Erro interno ao buscar entregadores"
            };
        }
    }
}

public class GetAllDeliveryDriversResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<DeliveryDriverDto> Drivers { get; set; } = new();
    public int TotalCount { get; set; }
}

public class DeliveryDriverDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = default!;
    public string Cnpj { get; set; } = default!;
    public DateTime Data_Nascimento { get; set; }
    public string Numero_Cnh { get; set; } = default!;
    public string Tipo_Cnh { get; set; } = default!;
    public bool Possui_Imagem_Cnh { get; set; }
}
