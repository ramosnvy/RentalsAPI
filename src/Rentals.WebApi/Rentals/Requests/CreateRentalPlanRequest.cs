using System.ComponentModel.DataAnnotations;

namespace Rentals.WebApi.Rentals.Requests;

public class CreateRentalPlanRequest
{
    [Required(ErrorMessage = "Nome do plano é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duração em dias é obrigatória")]
    [Range(1, 365, ErrorMessage = "Duração deve estar entre 1 e 365 dias")]
    public int DuracaoDias { get; set; }

    [Required(ErrorMessage = "Taxa diária é obrigatória")]
    [Range(0.01, 10000, ErrorMessage = "Taxa diária deve estar entre R$ 0,01 e R$ 10.000,00")]
    public decimal TaxaDiaria { get; set; }

    [Required(ErrorMessage = "Percentual de multa antecipada é obrigatório")]
    [Range(0, 100, ErrorMessage = "Percentual de multa deve estar entre 0% e 100%")]
    public decimal PercentualMultaAntecipada { get; set; }

    [Required(ErrorMessage = "Taxa de atraso diária é obrigatória")]
    [Range(0, 10000, ErrorMessage = "Taxa de atraso deve estar entre R$ 0,00 e R$ 10.000,00")]
    public decimal TaxaAtrasoDiaria { get; set; }
}
