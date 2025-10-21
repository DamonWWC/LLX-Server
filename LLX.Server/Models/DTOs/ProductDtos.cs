namespace LLX.Server.Models.DTOs;

/// <summary>
/// 商品 DTO
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? Image { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 创建商品 DTO
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? Image { get; set; }
    public int Quantity { get; set; } = 0;
}

/// <summary>
/// 更新商品 DTO
/// </summary>
public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? Image { get; set; }
    public int Quantity { get; set; }
}
