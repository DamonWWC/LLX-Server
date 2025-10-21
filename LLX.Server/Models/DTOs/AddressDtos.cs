namespace LLX.Server.Models.DTOs;

/// <summary>
/// 地址 DTO
/// </summary>
public class AddressDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string Detail { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 创建地址 DTO
/// </summary>
public class CreateAddressDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string Detail { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// 更新地址 DTO
/// </summary>
public class UpdateAddressDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string Detail { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

/// <summary>
/// 解析地址 DTO
/// </summary>
public class ParsedAddressDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }
    public string Detail { get; set; } = string.Empty;
}
