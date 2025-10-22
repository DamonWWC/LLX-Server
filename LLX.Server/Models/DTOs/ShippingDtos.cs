namespace LLX.Server.Models.DTOs;

/// <summary>
/// 运费配置 DTO
/// </summary>
public class ShippingRateDto
{
    public int Id { get; set; }
    public string Province { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 运费计算结果 DTO
/// </summary>
public class ShippingCalculationDto
{
    public string Province { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Rate { get; set; }
    public decimal TotalShipping { get; set; }
}

/// <summary>
/// 创建运费配置 DTO
/// </summary>
public class CreateShippingRateDto
{
    public string Province { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

/// <summary>
/// 更新运费配置 DTO
/// </summary>
public class UpdateShippingRateDto
{
    public string Province { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

/// <summary>
/// 运费计算请求 DTO
/// </summary>
public class CalculateShippingDto
{
    public string Province { get; set; } = string.Empty;
    public decimal Weight { get; set; }
}
