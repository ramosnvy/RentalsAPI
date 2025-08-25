using Microsoft.Extensions.Logging;
using Rentals.Application.Abstractions;
using Rentals.Application.Services;
using Rentals.Domain.Vehicles;
using Rentals.Domain.Drivers;

namespace Rentals.Application.Commands;

public class CreateRentalHandler
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IRentalPlanRepository _rentalPlanRepository;
    private readonly IMotorcycleRepository _motorcycleRepository;
    private readonly IDeliveryDriverRepository _deliveryDriverRepository;
    private readonly IRentalCalculationService _calculationService;
    private readonly ILogger<CreateRentalHandler> _logger;

    public CreateRentalHandler(
        IRentalRepository rentalRepository,
        IRentalPlanRepository rentalPlanRepository,
        IMotorcycleRepository motorcycleRepository,
        IDeliveryDriverRepository deliveryDriverRepository,
        IRentalCalculationService calculationService,
        ILogger<CreateRentalHandler> logger)
    {
        _rentalRepository = rentalRepository;
        _rentalPlanRepository = rentalPlanRepository;
        _motorcycleRepository = motorcycleRepository;
        _deliveryDriverRepository = deliveryDriverRepository;
        _calculationService = calculationService;
        _logger = logger;
    }

    public async Task<CreateRentalResult> Handle(
        long deliveryDriverId,
        long motorcycleId,
        long rentalPlanId,
        DateTime startDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando criação de locação para entregador {DeliveryDriverId} e moto {MotorcycleId}", 
            deliveryDriverId, motorcycleId);

        try
        {
            // Verificar se o entregador existe
            var deliveryDriver = await _deliveryDriverRepository.GetByIdAsync(deliveryDriverId);
            if (deliveryDriver == null)
            {
                _logger.LogWarning("Entregador {DeliveryDriverId} não encontrado", deliveryDriverId);
                return new CreateRentalResult
                {
                    Success = false,
                    ErrorMessage = "Entregador não encontrado"
                };
            }

            // Verificar se o entregador possui CNH categoria A
            if (deliveryDriver.Cnh.CnhCategory != CnhCategory.A && deliveryDriver.Cnh.CnhCategory != CnhCategory.AB)
            {
                _logger.LogWarning("Entregador {DeliveryDriverId} não possui CNH categoria A. Categoria atual: {CnhCategory}", 
                    deliveryDriverId, deliveryDriver.Cnh.CnhCategory);
                return new CreateRentalResult
                {
                    Success = false,
                    ErrorMessage = "Somente entregadores com CNH categoria A podem alugar motocicletas"
                };
            }

            // Verificar se o plano existe e está ativo
            var rentalPlan = await _rentalPlanRepository.GetByIdAsync(rentalPlanId, cancellationToken);
            if (rentalPlan == null)
            {
                _logger.LogWarning("Plano de locação {RentalPlanId} não encontrado", rentalPlanId);
                return new CreateRentalResult
                {
                    Success = false,
                    ErrorMessage = "Plano de locação não encontrado"
                };
            }

            if (!rentalPlan.IsActive)
            {
                _logger.LogWarning("Plano de locação {RentalPlanId} não está ativo", rentalPlanId);
                return new CreateRentalResult
                {
                    Success = false,
                    ErrorMessage = "Plano de locação não está ativo"
                };
            }

            // Verificar se a moto existe e está ativa
            var motorcycle = await _motorcycleRepository.GetByIdAsync(motorcycleId, cancellationToken);
            if (motorcycle == null)
            {
                _logger.LogWarning("Moto {MotorcycleId} não encontrada", motorcycleId);
                return new CreateRentalResult
                {
                    Success = false,
                    ErrorMessage = "Moto não encontrada"
                };
            }

            if (!motorcycle.IsActive)
            {
                _logger.LogWarning("Moto {MotorcycleId} não está ativa", motorcycleId);
                return new CreateRentalResult
                {
                    Success = false,
                    ErrorMessage = "Moto não está ativa"
                };
            }

            // Verificar se a moto não está alugada
            var activeRentals = await _rentalRepository.GetActiveByMotorcycleIdAsync(motorcycleId, cancellationToken);
            if (activeRentals.Any())
            {
                _logger.LogWarning("Moto {MotorcycleId} já está alugada", motorcycleId);
                return new CreateRentalResult
                {
                    Success = false,
                    ErrorMessage = "Moto já está alugada"
                };
            }

            // Calcular data de término esperada
            var expectedEndDate = startDate.AddDays(rentalPlan.DurationInDays);

            // Calcular valor total
            var totalAmount = _calculationService.CalculateTotalAmount(rentalPlan, startDate, expectedEndDate);

            // Criar a locação
            var rental = Rental.Create(
                deliveryDriverId,
                motorcycleId,
                rentalPlanId,
                startDate,
                expectedEndDate,
                totalAmount);

            await _rentalRepository.AddAsync(rental, cancellationToken);

            _logger.LogInformation("Locação criada com sucesso. ID: {RentalId}", rental.Id);

            return new CreateRentalResult
            {
                Success = true,
                RentalId = rental.Id,
                TotalAmount = rental.TotalAmount,
                ExpectedEndDate = rental.ExpectedEndDate
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Criação da locação falhou: {ErrorMessage}", ex.Message);
            return new CreateRentalResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante criação da locação");
            return new CreateRentalResult
            {
                Success = false,
                ErrorMessage = "Erro interno ao criar locação"
            };
        }
    }
}

public class CreateRentalResult
{
    public bool Success { get; set; }
    public long RentalId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public string? ErrorMessage { get; set; }
}
