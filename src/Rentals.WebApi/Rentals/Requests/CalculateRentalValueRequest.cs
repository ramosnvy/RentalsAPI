using System.ComponentModel.DataAnnotations;

namespace Rentals.WebApi.Rentals.Requests;

public class CalculateRentalValueRequest
{
    [Required(ErrorMessage = "ID do plano é obrigatório")]
    public long PlanoId { get; set; }

    [Required(ErrorMessage = "Data de início é obrigatória")]
    public DateTime DataInicio { get; set; }

    [Required(ErrorMessage = "Data de término é obrigatória")]
    public DateTime DataTermino { get; set; }
}

