using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Drivers;

public class DeliveryDriver : Entity
{
    public Guid Id { get; set; }
    public string Identifier { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Cnpj Cnpj { get; private set; } = default!;
    public DateTime BirthDate { get; private set; }
    public Cnh Cnh { get; private set; } = default!;
    public CnhImage? CnhImage { get; private set; } //can be null initialy
    
    private DeliveryDriver() { }

    public static DeliveryDriver Register(
        string identifier,
        string name,
        Cnpj cnpj,
        DateTime birthDate,
        Cnh cnh)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException("É obrigatório informar um usuário" ,nameof(identifier));
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("é obrigatório informar um nome.", nameof(name));
        }

        return new DeliveryDriver
        {
            Id = Guid.NewGuid(),
            Identifier = identifier,
            Name = name,
            Cnpj = cnpj,
            BirthDate = birthDate,
            Cnh = cnh
        };
    }
    
    public void UpdateCnhImage(CnhImage image)
    {
        CnhImage = image ?? throw new ArgumentNullException(nameof(image));
    }

}