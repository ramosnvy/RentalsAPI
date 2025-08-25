using System.ComponentModel.DataAnnotations;

namespace Rentals.WebApi.Rentals.Requests;

public class ReturnRentalRequest
{
    [Required(ErrorMessage = "Data de devolução é obrigatória")]
    public DateTime DataDevolucao { get; set; }
}
