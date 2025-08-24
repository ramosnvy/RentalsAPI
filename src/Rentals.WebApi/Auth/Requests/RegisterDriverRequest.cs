namespace Rentals.WebApi.Auth.Requests;

public record RegisterDriverRequest(
    string Identifier,
    string Email,
    string Password,
    string Nome,
    string Cnpj,
    DateTime Data_Nascimento,
    string Numero_Cnh,
    string Tipo_Cnh,
    string? Imagem_Cnh = null);
