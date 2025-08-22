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
        // rejeita sequências repetidas
        if (new string(d[0], d.Length) == d) return false;

        int Calc(string s, int[] mults) =>
            (11 - (s.Select((c, i) => (c - '0') * mults[i]).Sum() % 11)) % 10;

        var m1 = new[] {5,4,3,2,9,8,7,6,5,4,3,2};
        var m2 = new[] {6,5,4,3,2,9,8,7,6,5,4,3,2};

        var dv1 = Calc(d[..12], m1);
        var dv2 = Calc(d[..12] + dv1, m2);

        return d[12] - '0' == dv1 && d[13] - '0' == dv2;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}