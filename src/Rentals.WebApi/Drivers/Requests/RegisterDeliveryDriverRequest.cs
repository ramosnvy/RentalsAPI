namespace Rentals.WebApi.Drivers.Requests;

public class RegisterDeliveryDriverRequest
{
    public string Identificador { get; set; } = default!;
    public string Nome  { get; set; } = default!;
    public string Cnpj  { get; set; } = default!;
    public DateTime Data_Nascimento { get; set; }
    public string Numero_Cnh  { get; set; } = default!;
    public string Tipo_Cnh  { get; set; } = default!;
    public string? Imagem_Cnh  { get; set; }
}