using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Domain.Vehicles;
using Rentals.Domain.Vehicles.ValueObjects;

namespace Rentals.Application.Commands;

public class UpdateMotorcycleLicensePlateHandler
{
    private readonly IMotorcycleRepository _motorcycleRepository;
    private readonly ILogger<UpdateMotorcycleLicensePlateHandler> _logger;

    public UpdateMotorcycleLicensePlateHandler(
        IMotorcycleRepository motorcycleRepository,
        ILogger<UpdateMotorcycleLicensePlateHandler> logger)
    {
        _motorcycleRepository = motorcycleRepository;
        _logger = logger;
    }

    public async Task<UpdateMotorcycleLicensePlateResult> Handle(
        long id, string novaPlaca, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando atualização de placa para moto ID: {Id}, Nova placa: {NovaPlaca}", id, novaPlaca);
        
        try
        {
            // Validar formato da placa
            try
            {
                var licensePlate = LicensePlate.Create(novaPlaca);
                _logger.LogInformation("Formato da placa {Placa} é válido", novaPlaca);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Formato de placa inválido: {Placa}. Erro: {Error}", novaPlaca, ex.Message);
                return new UpdateMotorcycleLicensePlateResult 
                { 
                    Success = false, 
                    ErrorMessage = "Formato de placa inválido" 
                };
            }

            // Buscar a moto
            var motorcycle = await _motorcycleRepository.GetByIdAsync(id, cancellationToken);
            if (motorcycle == null)
            {
                _logger.LogWarning("Moto com ID {Id} não encontrada para atualização de placa", id);
                return new UpdateMotorcycleLicensePlateResult 
                { 
                    Success = false, 
                    ErrorMessage = "Moto não encontrada" 
                };
            }

            // Verificar se a nova placa já existe (exceto para a própria moto)
            var existingMotorcycle = await _motorcycleRepository.GetByLicensePlateAsync(novaPlaca, cancellationToken);
            if (existingMotorcycle != null && existingMotorcycle.Id != id)
            {
                _logger.LogWarning("Placa {Placa} já está em uso por outra moto", novaPlaca);
                return new UpdateMotorcycleLicensePlateResult 
                { 
                    Success = false, 
                    ErrorMessage = "Placa já está em uso" 
                };
            }

            // Atualizar a placa
            var novaLicensePlate = LicensePlate.Create(novaPlaca);
            motorcycle.UpdateLicensePlate(novaLicensePlate);
            
            await _motorcycleRepository.UpdateAsync(motorcycle, cancellationToken);

            _logger.LogInformation("Placa da moto ID {Id} atualizada com sucesso para {NovaPlaca}", id, novaPlaca);
            
            return new UpdateMotorcycleLicensePlateResult 
            { 
                Success = true, 
                Message = "Placa atualizada com sucesso" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar placa da moto ID {Id}", id);
            return new UpdateMotorcycleLicensePlateResult 
            { 
                Success = false, 
                ErrorMessage = "Erro interno ao atualizar placa" 
            };
        }
    }
}

public class UpdateMotorcycleLicensePlateResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}
