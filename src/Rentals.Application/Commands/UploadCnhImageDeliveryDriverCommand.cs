using Rentals.Domain.Drivers;

namespace Rentals.Application.Commands;

public record UploadCnhImageDeliveryDriverCommand
(
    Guid DriverId,
    string? CnhImageBase64 = null
);