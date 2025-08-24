namespace Rentals.Application.Abstractions;

public interface IJwtProvider
{
    string GenerateToken(long id, string identifier, string role);
}