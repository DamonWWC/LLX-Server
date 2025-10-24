using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;
using LLX.Server.Repositories;
using LLX.Server.Utils;
using Microsoft.Extensions.Logging;

namespace LLX.Server.Services;

/// <summary>
/// 订单服务实现
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IShippingService _shippingService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<OrderService> _logger;

    private const string CACHE_KEY_PREFIX = "order:";
    private const string CACHE_KEY_ALL = "order:all";
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IProductRepository productRepository,
        IAddressRepository addressRepository,
        IShippingService shippingService,
        ICacheService cacheService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _addressRepository = addressRepository;
        _shippingService = shippingService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有订单
    /// </summary>
    /// <returns>订单列表响应</returns>
    public async Task<ApiResponse<IEnumerable<OrderDto>>> GetAllOrdersAsync()
    {
        try
        {
            _logger.LogInformation("Getting all orders");

            // 尝试从缓存获取
            var cachedOrders = await _cacheService.GetAsync<IEnumerable<OrderDto>>(CACHE_KEY_ALL);
            if (cachedOrders != null)
            {
                _logger.LogInformation("Retrieved {Count} orders from cache", cachedOrders.Count());
                return ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(cachedOrders, "获取订单列表成功");
            }

            // 从数据库获取
            var orders = await _orderRepository.GetAllAsync();
            var orderDtos = orders.Select(MapToDto).ToList();

            // 缓存结果
            await _cacheService.SetAsync(CACHE_KEY_ALL, orderDtos, _cacheExpiry);

            _logger.LogInformation("Retrieved {Count} orders from database", orderDtos.Count);
            return ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(orderDtos, "获取订单列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("获取订单列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>订单响应</returns>
    public async Task<ApiResponse<OrderDto?>> GetOrderByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting order by ID: {OrderId}", id);

            // 尝试从缓存获取
            var cacheKey = $"{CACHE_KEY_PREFIX}{id}";
            var cachedOrder = await _cacheService.GetAsync<OrderDto>(cacheKey);
            if (cachedOrder != null)
            {
                _logger.LogInformation("Retrieved order {OrderId} from cache", id);
                return ApiResponse<OrderDto?>.SuccessResponse(cachedOrder, "获取订单成功");
            }

            // 从数据库获取
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return ApiResponse<OrderDto?>.ErrorResponse("订单不存在");
            }

            var orderDto = MapToDto(order);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, orderDto, _cacheExpiry);

            _logger.LogInformation("Retrieved order {OrderId} from database", id);
            return ApiResponse<OrderDto?>.SuccessResponse(orderDto, "获取订单成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by ID: {OrderId}", id);
            return ApiResponse<OrderDto?>.ErrorResponse("获取订单失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据订单号获取订单
    /// </summary>
    /// <param name="orderNo">订单号</param>
    /// <returns>订单响应</returns>
    public async Task<ApiResponse<OrderDto?>> GetOrderByOrderNoAsync(string orderNo)
    {
        try
        {
            _logger.LogInformation("Getting order by order number: {OrderNo}", orderNo);

            if (string.IsNullOrWhiteSpace(orderNo))
            {
                return ApiResponse<OrderDto?>.ErrorResponse("订单号不能为空");
            }

            var order = await _orderRepository.GetByOrderNoAsync(orderNo);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderNo} not found", orderNo);
                return ApiResponse<OrderDto?>.ErrorResponse("订单不存在");
            }

            var orderDto = MapToDto(order);

            _logger.LogInformation("Retrieved order {OrderNo} from database", orderNo);
            return ApiResponse<OrderDto?>.SuccessResponse(orderDto, "获取订单成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by order number: {OrderNo}", orderNo);
            return ApiResponse<OrderDto?>.ErrorResponse("获取订单失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据状态获取订单列表
    /// </summary>
    /// <param name="status">订单状态</param>
    /// <returns>订单列表响应</returns>
    public async Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByStatusAsync(string status)
    {
        try
        {
            _logger.LogInformation("Getting orders by status: {Status}", status);

            if (string.IsNullOrWhiteSpace(status))
            {
                return ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("订单状态不能为空");
            }

            var orders = await _orderRepository.GetByStatusAsync(status);
            var orderDtos = orders.Select(MapToDto).ToList();

            _logger.LogInformation("Found {Count} orders with status {Status}", orderDtos.Count, status);
            return ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(orderDtos, "获取订单列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status: {Status}", status);
            return ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("获取订单列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据地址ID获取订单列表
    /// </summary>
    /// <param name="addressId">地址ID</param>
    /// <returns>订单列表响应</returns>
    public async Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByAddressIdAsync(int addressId)
    {
        try
        {
            _logger.LogInformation("Getting orders by address ID: {AddressId}", addressId);

            var orders = await _orderRepository.GetByAddressIdAsync(addressId);
            var orderDtos = orders.Select(MapToDto).ToList();

            _logger.LogInformation("Found {Count} orders for address {AddressId}", orderDtos.Count, addressId);
            return ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(orderDtos, "获取订单列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by address ID: {AddressId}", addressId);
            return ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("获取订单列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="createDto">创建订单DTO</param>
    /// <returns>订单响应</returns>
    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating order for address: {AddressId}", createDto.AddressId);

            // 验证输入
            var validationErrors = ValidateCreateOrder(createDto);
            if (validationErrors.Any())
            {
                return ApiResponse<OrderDto>.ErrorResponse("输入验证失败", validationErrors);
            }

            // 验证地址是否存在
            var address = await _addressRepository.GetByIdAsync(createDto.AddressId);
            if (address == null)
            {
                return ApiResponse<OrderDto>.ErrorResponse("地址不存在");
            }

            // 验证商品并计算总金额
            var calculationResult = await CalculateOrderAsync(createDto.Items, createDto.AddressId);
            if (!calculationResult.Success)
            {
                return ApiResponse<OrderDto>.ErrorResponse("订单计算失败", calculationResult.Errors);
            }

            var calculation = calculationResult.Data!;

            // 生成订单号
            var orderNo = OrderNumberGenerator.Generate();

            // 创建订单
            var order = new Order
            {
                OrderNo = orderNo,
                AddressId = createDto.AddressId,
                TotalRicePrice = calculation.TotalRicePrice,
                TotalWeight = calculation.TotalWeight,
                ShippingRate = calculation.ShippingRate,
                TotalShipping = calculation.TotalShipping,
                GrandTotal = calculation.GrandTotal,
                PaymentStatus = createDto.PaymentStatus,
                Status = createDto.Status
            };

            var createdOrder = await _orderRepository.AddAsync(order);

            // 创建订单明细
            var orderItems = new List<OrderItem>();
            foreach (var item in createDto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null) continue;

                var orderItem = new OrderItem
                {
                    OrderId = createdOrder.Id,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    ProductPrice = product.Price,
                    ProductUnit = product.Unit,
                    ProductWeight = product.Weight,
                    Quantity = item.Quantity,
                    Subtotal = product.Price * item.Quantity
                };

                orderItems.Add(orderItem);
            }

            // 批量添加订单明细到数据库
            if (orderItems.Any())
            {
                await _orderItemRepository.AddRangeAsync(orderItems);
                _logger.LogInformation("Created {Count} order items for order {OrderId}", orderItems.Count, createdOrder.Id);
            }

            var orderDto = MapToDto(createdOrder);

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);

            _logger.LogInformation("Created order {OrderId} with order number {OrderNo}", createdOrder.Id, createdOrder.OrderNo);
            return ApiResponse<OrderDto>.SuccessResponse(orderDto, "创建订单成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for address: {AddressId}", createDto.AddressId);
            return ApiResponse<OrderDto>.ErrorResponse("创建订单失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 更新订单状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="status">新状态</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, string status)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId} status to {Status}", id, status);

            if (string.IsNullOrWhiteSpace(status))
            {
                return ApiResponse<bool>.ErrorResponse("订单状态不能为空");
            }

            var result = await _orderRepository.UpdateStatusAsync(id, status);
            if (!result)
            {
                _logger.LogWarning("Order {OrderId} not found for status update", id);
                return ApiResponse<bool>.ErrorResponse("订单不存在");
            }

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Updated order {OrderId} status to {Status}", id, status);
            return ApiResponse<bool>.SuccessResponse(true, "更新订单状态成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status", id);
            return ApiResponse<bool>.ErrorResponse("更新订单状态失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 更新支付状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="paymentStatus">支付状态</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> UpdatePaymentStatusAsync(int id, string paymentStatus)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId} payment status to {PaymentStatus}", id, paymentStatus);

            if (string.IsNullOrWhiteSpace(paymentStatus))
            {
                return ApiResponse<bool>.ErrorResponse("支付状态不能为空");
            }

            var result = await _orderRepository.UpdatePaymentStatusAsync(id, paymentStatus);
            if (!result)
            {
                _logger.LogWarning("Order {OrderId} not found for payment status update", id);
                return ApiResponse<bool>.ErrorResponse("订单不存在");
            }

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Updated order {OrderId} payment status to {PaymentStatus}", id, paymentStatus);
            return ApiResponse<bool>.SuccessResponse(true, "更新支付状态成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} payment status", id);
            return ApiResponse<bool>.ErrorResponse("更新支付状态失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 删除订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> DeleteOrderAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting order {OrderId}", id);

            var result = await _orderRepository.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("Order {OrderId} not found for deletion", id);
                return ApiResponse<bool>.ErrorResponse("订单不存在");
            }

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Deleted order {OrderId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "删除订单成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            return ApiResponse<bool>.ErrorResponse("删除订单失败", new List<string> { ex.Message });
        }
    }
    /// <summary>
    /// 批量删除订单
    /// </summary>
    /// <param name="ids">订单ID列表</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> DeleteOrdersAsync(List<int> ids)
    {
        try
        {
            _logger.LogInformation("Deleting orders: {OrderIds}", string.Join(", ", ids));

            // 验证输入
            if (ids == null || !ids.Any())
            {
                return ApiResponse<bool>.ErrorResponse("订单ID列表不能为空");
            }

            if (ids.Any(id => id <= 0))
            {
                return ApiResponse<bool>.ErrorResponse("订单ID必须大于0");
            }

            // 去重
            var uniqueIds = ids.Distinct().ToList();

            var result = await _orderRepository.DeleteAllAsync(uniqueIds);
            if (!result)
            {
                _logger.LogWarning("Orders {OrderIds} not found for deletion", string.Join(", ", uniqueIds));
                return ApiResponse<bool>.ErrorResponse("订单不存在");
            }

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            foreach (var id in uniqueIds)
            {
                await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");
            }

            _logger.LogInformation("Successfully deleted {Count} orders: {OrderIds}", uniqueIds.Count, string.Join(", ", uniqueIds));
            return ApiResponse<bool>.SuccessResponse(true, $"成功删除 {uniqueIds.Count} 个订单");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting orders: {OrderIds}", string.Join(", ", ids));
            return ApiResponse<bool>.ErrorResponse("删除订单失败", new List<string> { ex.Message });
        }
    }
    /// <summary>
    /// 计算订单总金额
    /// </summary>
    /// <param name="items">订单明细</param>
    /// <param name="addressId">地址ID</param>
    /// <returns>计算结果响应</returns>
    public async Task<ApiResponse<OrderCalculationDto>> CalculateOrderAsync(List<CreateOrderItemDto> items, int addressId)
    {
        try
        {
            _logger.LogInformation("Calculating order for address: {AddressId}", addressId);

            if (items == null || !items.Any())
            {
                return ApiResponse<OrderCalculationDto>.ErrorResponse("订单明细不能为空");
            }

            // 获取地址信息
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null)
            {
                return ApiResponse<OrderCalculationDto>.ErrorResponse("地址不存在");
            }

            decimal totalRicePrice = 0;
            decimal totalWeight = 0;

            // 计算商品总价和总重量
            foreach (var item in items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    return ApiResponse<OrderCalculationDto>.ErrorResponse($"商品ID {item.ProductId} 不存在");
                }

                // 移除库存验证，默认商品有无限库存
                // if (product.Quantity < item.Quantity)
                // {
                //     return ApiResponse<OrderCalculationDto>.ErrorResponse($"商品 {product.Name} 库存不足");
                // }

                totalRicePrice += product.Price * item.Quantity;
                totalWeight += product.Weight * item.Quantity;
            }

            // 计算运费
            var shippingResult = await _shippingService.CalculateShippingAsync(address.Province, totalWeight);
            if (!shippingResult.Success)
            {
                return ApiResponse<OrderCalculationDto>.ErrorResponse("运费计算失败", shippingResult.Errors);
            }

            var shipping = shippingResult.Data!;
            var grandTotal = totalRicePrice + shipping.TotalShipping;

            var calculation = new OrderCalculationDto
            {
                TotalRicePrice = totalRicePrice,
                TotalWeight = totalWeight,
                ShippingRate = shipping.Rate,
                TotalShipping = shipping.TotalShipping,
                GrandTotal = grandTotal,
                Province = address.Province
            };

            _logger.LogInformation("Calculated order: Total={Total}, Weight={Weight}, Shipping={Shipping}", 
                grandTotal, totalWeight, shipping.TotalShipping);

            return ApiResponse<OrderCalculationDto>.SuccessResponse(calculation, "订单计算成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating order for address: {AddressId}", addressId);
            return ApiResponse<OrderCalculationDto>.ErrorResponse("订单计算失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 映射实体到DTO
    /// </summary>
    /// <param name="order">订单实体</param>
    /// <returns>订单DTO</returns>
    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            AddressId = order.AddressId,
            TotalRicePrice = order.TotalRicePrice,
            TotalWeight = order.TotalWeight,
            ShippingRate = order.ShippingRate,
            TotalShipping = order.TotalShipping,
            GrandTotal = order.GrandTotal,
            PaymentStatus = order.PaymentStatus,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Address = order.Address != null ? new AddressDto
            {
                Id = order.Address.Id,
                Name = order.Address.Name,
                Phone = order.Address.Phone,
                Province = order.Address.Province,
                City = order.Address.City,
                District = order.Address.District,
                Detail = order.Address.Detail,
                IsDefault = order.Address.IsDefault,
                CreatedAt = order.Address.CreatedAt,
                UpdatedAt = order.Address.UpdatedAt
            } : null,
            OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                ProductPrice = oi.ProductPrice,
                ProductUnit = oi.ProductUnit,
                ProductWeight = oi.ProductWeight,
                Quantity = oi.Quantity,
                Subtotal = oi.Subtotal
            }).ToList() ?? new List<OrderItemDto>()
        };
    }

    /// <summary>
    /// 验证创建订单输入
    /// </summary>
    /// <param name="createDto">创建订单DTO</param>
    /// <returns>验证错误列表</returns>
    private static List<string> ValidateCreateOrder(CreateOrderDto createDto)
    {
        var errors = new List<string>();

        if (createDto.AddressId <= 0)
            errors.Add("地址ID必须大于0");

        if (createDto.Items == null || !createDto.Items.Any())
            errors.Add("订单明细不能为空");

        if (string.IsNullOrWhiteSpace(createDto.PaymentStatus))
            errors.Add("支付状态不能为空");

        if (string.IsNullOrWhiteSpace(createDto.Status))
            errors.Add("订单状态不能为空");

        if (createDto.Items != null)
        {
            foreach (var item in createDto.Items)
            {
                if (item.ProductId <= 0)
                    errors.Add("商品ID必须大于0");

                if (item.Quantity <= 0)
                    errors.Add("商品数量必须大于0");
            }
        }

        return errors;
    }
}
