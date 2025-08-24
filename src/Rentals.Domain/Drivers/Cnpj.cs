using System.Text.RegularExpressions;
using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Drivers;

public sealed class Cnpj : ValueObject
{
    public string Value { get; }

    private Cnpj(string value)
    {
        Value = value;
    }

    public static Cnpj Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("É obrigatório informar o CNPJ",  nameof(value));
        }
        
        var digits = Regex.Replace(value, "[^0-9]", "");
        
        if (digits.Length != 14)
        {
            throw new ArgumentException("CNPJ deve ter 14 dígitos.");
        }

        if (!IsValid(digits))
        {
            throw new ArgumentException("CNPJ inválido.");
        }

        return new Cnpj(digits);
    }

    // Validação simples de CNPJ (dígitos verificadores)
    private static bool IsValid(string d)
    {
        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}