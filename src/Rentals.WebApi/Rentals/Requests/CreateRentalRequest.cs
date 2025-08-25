using System.ComponentModel.DataAnnotations;

namespace Rentals.WebApi.Rentals.Requests;

public class CreateRentalRequest
{
    [Required(ErrorMessage = "ID do entregador é obrigatório")]
    public long EntregadorId { get; set; }

    [Required(ErrorMessage = "ID da moto é obrigatório")]
    public long MotoId { get; set; }

    [Required(ErrorMessage = "ID do plano é obrigatório")]
    public long PlanoId { get; set; }

    [Required(ErrorMessage = "Data de início é obrigatória")]
    public DateTime DataInicio { get; set; }
}
