namespace Rentals.Application.Abstractions;

public interface IJwtProvider
{
    string GenerateToken(string id, string identifier, string role);
}