using System.ComponentModel.DataAnnotations;

namespace Rentals.WebApi.Vehicles.Requests;

public class RegisterMotorcycleRequest
{
    [Required(ErrorMessage = "Identificador é obrigatório")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ano é obrigatório")]
    [Range(1900, 2030, ErrorMessage = "Ano deve estar entre 1900 e 2030")]
    public int Ano { get; set; }

    [Required(ErrorMessage = "Modelo é obrigatório")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Placa é obrigatória")]
    public string Placa { get; set; } = string.Empty;
}
