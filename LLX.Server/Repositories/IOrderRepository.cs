using LLX.Server.Models.Entities;

namespace LLX.Server.Repositories;

/// <summary>
/// 订单仓储接口
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// 获取所有订单
    /// </summary>
    /// <returns>订单列表</returns>
    Task<IEnumerable<Order>> GetAllAsync();

    /// <summary>
    /// 根据ID获取订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>订单信息</returns>
    Task<Order?> GetByIdAsync(int id);

    /// <summary>
    /// 根据订单号获取订单
    /// </summary>
    /// <param name="orderNo">订单号</param>
    /// <returns>订单信息</returns>
    Task<Order?> GetByOrderNoAsync(string orderNo);

    /// <summary>
    /// 根据状态获取订单列表
    /// </summary>
    /// <param name="status">订单状态</param>
    /// <returns>订单列表</returns>
    Task<IEnumerable<Order>> GetByStatusAsync(string status);

    /// <summary>
    /// 根据地址ID获取订单列表
    /// </summary>
    /// <param name="addressId">地址ID</param>
    /// <returns>订单列表</returns>
    Task<IEnumerable<Order>> GetByAddressIdAsync(int addressId);

    /// <summary>
    /// 添加订单
    /// </summary>
    /// <param name="order">订单信息</param>
    /// <returns>添加后的订单</returns>
    Task<Order> AddAsync(Order order);

    /// <summary>
    /// 更新订单
    /// </summary>
    /// <param name="order">订单信息</param>
    /// <returns>更新后的订单</returns>
    Task<Order> UpdateAsync(Order order);

    /// <summary>
    /// 删除订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 更新订单状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="status">新状态</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateStatusAsync(int id, string status);

    /// <summary>
    /// 更新支付状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="paymentStatus">支付状态</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus);

    /// <summary>
    /// 检查订单是否存在
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 检查订单号是否存在
    /// </summary>
    /// <param name="orderNo">订单号</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsByOrderNoAsync(string orderNo);
}
