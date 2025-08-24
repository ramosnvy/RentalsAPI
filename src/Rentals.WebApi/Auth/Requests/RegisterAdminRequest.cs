namespace Rentals.WebApi.Auth.Requests;

public record RegisterAdminRequest(
    string Identifier,
    string Email,
    string Password,
    string Nome,
    string? Telefone);
