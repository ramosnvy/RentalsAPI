using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Vehicles.ValueObjects;

public class LicensePlate : ValueObject
{
    public string Value { get; private set; }

    private LicensePlate() { } // Para EF Core

    public static LicensePlate Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Placa é obrigatória", nameof(value));

        // Remove espaços e converte para maiúsculo
        var cleanValue = value.Replace(" ", "").ToUpper();

        // Validação de formato brasileiro (Mercosul ou antigo)
        if (!IsValidBrazilianLicensePlate(cleanValue))
            throw new ArgumentException("Formato de placa inválido. Use formato Mercosul (ABC1D23) ou antigo (ABC1234)", nameof(value));

        return new LicensePlate { Value = cleanValue };
    }

    private static bool IsValidBrazilianLicensePlate(string plate)
    {
        // Formato Mercosul: ABC1D23 (3 letras + 1 número + 1 letra + 2 números)
        var mercosulPattern = @"^[A-Z]{3}[0-9][A-Z][0-9]{2}$";
        
        // Formato antigo: ABC1234 (3 letras + 4 números)
        var oldPattern = @"^[A-Z]{3}[0-9]{4}$";

        return System.Text.RegularExpressions.Regex.IsMatch(plate, mercosulPattern) ||
               System.Text.RegularExpressions.Regex.IsMatch(plate, oldPattern);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
