using Rentals.Domain.Abstractions;
using Rentals.Domain.Drivers;

namespace Rentals.Domain.Users;

public class User : Entity
{
    public long Id { get; set; }
    public string Username { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    // Navigation properties
    public DeliveryDriver? DeliveryDriver { get; private set; }
    public Admin? Admin { get; private set; }
    
    private User() { }

    public static User CreateDeliveryDriver(
        string username,
        string email,
        string passwordHash,
        DeliveryDriver deliveryDriver)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username é obrigatório", nameof(username));
            
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email é obrigatório", nameof(email));
            
        if (string.IsNullOrEmpty(passwordHash))
            throw new ArgumentException("Password hash é obrigatório", nameof(passwordHash));
            
        if (deliveryDriver == null)
            throw new ArgumentNullException(nameof(deliveryDriver));

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = UserRole.DeliveryDriver,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            DeliveryDriver = deliveryDriver
        };
    }

    public static User CreateAdmin(
        string username,
        string email,
        string passwordHash)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("Username é obrigatório", nameof(username));
            
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email é obrigatório", nameof(email));
            
        if (string.IsNullOrEmpty(passwordHash))
            throw new ArgumentException("Password hash é obrigatório", nameof(passwordHash));

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
