namespace LLX.Server.Models.DTOs;

/// <summary>
/// 订单数据传输对象
/// </summary>
public class OrderDto
{
    /// <summary>
    /// 订单ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;
    
    /// <summary>
    /// 收货地址ID
    /// </summary>
    public int AddressId { get; set; }
    
    /// <summary>
    /// 商品总价（元）
    /// </summary>
    public decimal TotalRicePrice { get; set; }
    
    /// <summary>
    /// 总重量（kg）
    /// </summary>
    public decimal TotalWeight { get; set; }
    
    /// <summary>
    /// 运费单价（元/kg）
    /// </summary>
    public decimal ShippingRate { get; set; }
    
    /// <summary>
    /// 总运费（元）
    /// </summary>
    public decimal TotalShipping { get; set; }
    
    /// <summary>
    /// 订单总金额（元）
    /// </summary>
    public decimal GrandTotal { get; set; }
    
    /// <summary>
    /// 支付状态
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// 订单状态
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 收货地址信息
    /// </summary>
    public AddressDto? Address { get; set; }
    
    /// <summary>
    /// 订单明细列表
    /// </summary>
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
/// 订单明细数据传输对象
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// 订单明细ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 订单ID
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// 商品ID
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// 商品名称
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品单价（元）
    /// </summary>
    public decimal ProductPrice { get; set; }
    
    /// <summary>
    /// 商品单位
    /// </summary>
    public string ProductUnit { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品重量（kg）
    /// </summary>
    public decimal ProductWeight { get; set; }
    
    /// <summary>
    /// 购买数量
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 小计金额（元）
    /// </summary>
    public decimal Subtotal { get; set; }
}

/// <summary>
/// 创建订单数据传输对象
/// </summary>
public class CreateOrderDto
{
    /// <summary>
    /// 收货地址ID
    /// </summary>
    public int AddressId { get; set; }
    
    /// <summary>
    /// 订单商品列表
    /// </summary>
    public List<CreateOrderItemDto> Items { get; set; } = new();
    
    /// <summary>
    /// 支付状态（默认：未付款）
    /// </summary>
    public string PaymentStatus { get; set; } = "未付款";
    
    /// <summary>
    /// 订单状态（默认：待发货）
    /// </summary>
    public string Status { get; set; } = "待发货";
}

/// <summary>
/// 创建订单明细数据传输对象
/// </summary>
public class CreateOrderItemDto
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// 购买数量
    /// </summary>
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
/// 订单计算数据传输对象
/// </summary>
public class OrderCalculationDto
{
    /// <summary>
    /// 商品总价（元）
    /// </summary>
    public decimal TotalRicePrice { get; set; }
    
    /// <summary>
    /// 总重量（kg）
    /// </summary>
    public decimal TotalWeight { get; set; }
    
    /// <summary>
    /// 运费单价（元/kg）
    /// </summary>
    public decimal ShippingRate { get; set; }
    
    /// <summary>
    /// 总运费（元）
    /// </summary>
    public decimal TotalShipping { get; set; }
    
    /// <summary>
    /// 订单总金额（元）
    /// </summary>
    public decimal GrandTotal { get; set; }
    
    /// <summary>
    /// 省份名称
    /// </summary>
    public string Province { get; set; } = string.Empty;
}

/// <summary>
/// 更新订单状态请求 DTO
/// </summary>
public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 更新支付状态请求 DTO
/// </summary>
public class UpdatePaymentStatusRequest
{
    public string PaymentStatus { get; set; } = string.Empty;
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
