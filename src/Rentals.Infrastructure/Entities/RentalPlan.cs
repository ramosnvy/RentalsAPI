using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rentals.Infrastructure.Entities;

[Table("rental_plans")]
public class RentalPlan
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("duration_in_days")]
    public int DurationInDays { get; set; }

    [Required]
    [Column("daily_rate", TypeName = "decimal(10,2)")]
    public decimal DailyRate { get; set; }

    [Required]
    [Column("early_return_penalty_percentage", TypeName = "decimal(5,2)")]
    public decimal EarlyReturnPenaltyPercentage { get; set; }

    [Required]
    [Column("late_return_daily_fee", TypeName = "decimal(10,2)")]
    public decimal LateReturnDailyFee { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Relacionamentos
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
