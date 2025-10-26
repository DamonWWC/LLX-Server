namespace LLX.Server.Models.DTOs;

/// <summary>
/// 商品数据传输对象
/// </summary>
public class ProductDto
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品价格（元）
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// 商品单位（如：袋、斤、kg等）
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品重量（kg）
    /// </summary>
    public decimal Weight { get; set; }
    
    /// <summary>
    /// 商品图片（Base64编码）
    /// </summary>
    public string? Image { get; set; }
    
    /// <summary>
    /// 库存数量
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 创建商品数据传输对象
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品价格（元）
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// 商品单位（如：袋、斤、kg等）
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品重量（kg）
    /// </summary>
    public decimal Weight { get; set; }
    
    /// <summary>
    /// 商品图片（Base64编码）
    /// </summary>
    public string? Image { get; set; }
    
    /// <summary>
    /// 库存数量（默认0）
    /// </summary>
    public int Quantity { get; set; } = 0;
}

/// <summary>
/// 更新商品数据传输对象
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品价格（元）
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// 商品单位（如：袋、斤、kg等）
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品重量（kg）
    /// </summary>
    public decimal Weight { get; set; }
    
    /// <summary>
    /// 商品图片（Base64编码）
    /// </summary>
    public string? Image { get; set; }
    
    /// <summary>
    /// 库存数量
    /// </summary>
    public int Quantity { get; set; }
}
