using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LLX.Server.Models.Entities;

/// <summary>
/// 运费配置实体
/// </summary>
[Table("ShippingRates")]
public class ShippingRate
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Rate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
