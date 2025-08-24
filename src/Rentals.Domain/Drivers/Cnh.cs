using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Drivers;

public sealed class Cnh : ValueObject
{
    public string Number { get; }
    public CnhCategory CnhCategory { get; }

    private Cnh(string number, CnhCategory cnhCategory)
    {
        Number = number;
        CnhCategory = cnhCategory;
    }

    public static Cnh Create(string number, string type)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("É necessário informar o número da Cnh.", nameof(number));
        }

        var category = type switch
        {
            "A" => Drivers.CnhCategory.A,
            "B" => Drivers.CnhCategory.B,
            "AB" => Drivers.CnhCategory.AB,
            "A+B" => Drivers.CnhCategory.AB,
            _ => throw new ArgumentException("Tipo de CNH inválido. Use A, B, AB ou A+B", nameof(type))
        };

        return new Cnh(number.Trim(), category);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Number;
        yield return CnhCategory;
        
    }
}