namespace Rentals.Application.Commands;

public record RegisterDeliveryDriverCommand(
    string Identifier,
    string Name,
    string Cnpj,
    DateTime BirthDate,
    string CnhNumber,
    string CnhCategory,
    string? CnhImageBase64 = null
    );