using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LLX.Server.Models.Entities;

/// <summary>
/// 收货地址实体
/// </summary>
[Table("Addresses")]
public class Address
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string City { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? District { get; set; }

    [Required]
    [MaxLength(200)]
    public string Detail { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
