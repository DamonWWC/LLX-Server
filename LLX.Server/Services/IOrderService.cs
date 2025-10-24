using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;

namespace LLX.Server.Services;

/// <summary>
/// 订单服务接口
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// 获取所有订单
    /// </summary>
    /// <returns>订单列表响应</returns>
    Task<ApiResponse<IEnumerable<OrderDto>>> GetAllOrdersAsync();

    /// <summary>
    /// 根据ID获取订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>订单响应</returns>
    Task<ApiResponse<OrderDto?>> GetOrderByIdAsync(int id);

    /// <summary>
    /// 根据订单号获取订单
    /// </summary>
    /// <param name="orderNo">订单号</param>
    /// <returns>订单响应</returns>
    Task<ApiResponse<OrderDto?>> GetOrderByOrderNoAsync(string orderNo);

    /// <summary>
    /// 根据状态获取订单列表
    /// </summary>
    /// <param name="status">订单状态</param>
    /// <returns>订单列表响应</returns>
    Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByStatusAsync(string status);

    /// <summary>
    /// 根据地址ID获取订单列表
    /// </summary>
    /// <param name="addressId">地址ID</param>
    /// <returns>订单列表响应</returns>
    Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByAddressIdAsync(int addressId);

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="createDto">创建订单DTO</param>
    /// <returns>订单响应</returns>
    Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto createDto);

    /// <summary>
    /// 更新订单状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="status">新状态</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> UpdateOrderStatusAsync(int id, string status);

    /// <summary>
    /// 更新支付状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="paymentStatus">支付状态</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> UpdatePaymentStatusAsync(int id, string paymentStatus);

    /// <summary>
    /// 删除订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> DeleteOrderAsync(int id);

    /// <summary>
    /// 批量删除订单
    /// </summary>
    /// <param name="ids">订单ID列表</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> DeleteOrdersAsync(List<int> ids);

    /// <summary>
    /// 计算订单总金额
    /// </summary>
    /// <param name="items">订单明细</param>
    /// <param name="addressId">地址ID</param>
    /// <returns>计算结果响应</returns>
    Task<ApiResponse<OrderCalculationDto>> CalculateOrderAsync(List<CreateOrderItemDto> items, int addressId);
}
