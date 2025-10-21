using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LLX.Server.Models.Entities;

/// <summary>
/// 订单实体
/// </summary>
[Table("Orders")]
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNo { get; set; } = string.Empty;

    [Required]
    public int AddressId { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalRicePrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalWeight { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal ShippingRate { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalShipping { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal GrandTotal { get; set; }

    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "未付款";

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "待发货";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    [ForeignKey("AddressId")]
    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
