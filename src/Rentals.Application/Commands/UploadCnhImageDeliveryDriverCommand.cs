using Rentals.Domain.Drivers;

namespace Rentals.Application.Commands;

public record UploadCnhImageDeliveryDriverCommand(
    long Id,
    string CnhImageBase64
);
