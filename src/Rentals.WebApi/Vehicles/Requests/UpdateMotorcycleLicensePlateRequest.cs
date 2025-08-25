using System.ComponentModel.DataAnnotations;

namespace Rentals.WebApi.Vehicles.Requests;

public class UpdateMotorcycleLicensePlateRequest
{
    [Required(ErrorMessage = "Nova placa é obrigatória")]
    public string NovaPlaca { get; set; } = string.Empty;
}
