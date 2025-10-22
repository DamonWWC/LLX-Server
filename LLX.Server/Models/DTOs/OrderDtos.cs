namespace LLX.Server.Models.DTOs;

/// <summary>
/// 订单 DTO
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public int AddressId { get; set; }
    public decimal TotalRicePrice { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal ShippingRate { get; set; }
    public decimal TotalShipping { get; set; }
    public decimal GrandTotal { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AddressDto? Address { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

/// <summary>
/// 订单详情 DTO
/// </summary>
public class OrderDetailDto : OrderDto
{
    // 继承 OrderDto 的所有属性
}

/// <summary>
/// 订单明细 DTO
/// </summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string ProductUnit { get; set; } = string.Empty;
    public decimal ProductWeight { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// 创建订单 DTO
/// </summary>
public class CreateOrderDto
{
    public int AddressId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// 创建订单明细 DTO
/// </summary>
public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// 更新订单状态 DTO
/// </summary>
public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 订单计算 DTO
/// </summary>
public class OrderCalculationDto
{
    public decimal TotalRicePrice { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal ShippingRate { get; set; }
    public decimal TotalShipping { get; set; }
    public decimal GrandTotal { get; set; }
    public string Province { get; set; } = string.Empty;
}

/// <summary>
/// 分页结果 DTO
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
