using Rentals.Domain.Abstractions;

namespace Rentals.Domain.Users;

public class Admin : Entity
{
    public long Id { get; set; }
    public string Name { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private Admin() { }

    public static Admin Create(string name, string? phoneNumber = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Nome é obrigatório", nameof(name));

        return new Admin
        {
            Name = name,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }
}
