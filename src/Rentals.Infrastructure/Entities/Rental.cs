using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rentals.Infrastructure.Entities;

[Table("rentals")]
public class Rental
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("delivery_driver_id")]
    public long DeliveryDriverId { get; set; }

    [Required]
    [Column("motorcycle_id")]
    public long MotorcycleId { get; set; }

    [Required]
    [Column("rental_plan_id")]
    public long RentalPlanId { get; set; }

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("expected_end_date")]
    public DateTime ExpectedEndDate { get; set; }

    [Column("actual_end_date")]
    public DateTime? ActualEndDate { get; set; }

    [Required]
    [Column("total_amount", TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Column("final_amount", TypeName = "decimal(10,2)")]
    public decimal? FinalAmount { get; set; }

    [Required]
    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Relacionamentos
    public virtual Rentals.Domain.Drivers.DeliveryDriver? DeliveryDriver { get; set; }
    public virtual Motorcycle? Motorcycle { get; set; }
    public virtual RentalPlan? RentalPlan { get; set; }
}
