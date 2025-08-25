using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rentals.Infrastructure.Entities;

[Table("motorcycles")]
public class Motorcycle
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("identifier")]
    [StringLength(100)]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    [Column("year")]
    public int Year { get; set; }

    [Required]
    [Column("model")]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Column("license_plate")]
    [StringLength(10)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
