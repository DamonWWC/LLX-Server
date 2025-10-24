using LLX.Server.Models.Entities;

namespace LLX.Server.Repositories;

/// <summary>
/// 订单明细仓储接口
/// </summary>
public interface IOrderItemRepository
{
    /// <summary>
    /// 获取所有订单明细
    /// </summary>
    /// <returns>订单明细列表</returns>
    Task<IEnumerable<OrderItem>> GetAllAsync();

    /// <summary>
    /// 根据ID获取订单明细
    /// </summary>
    /// <param name="id">订单明细ID</param>
    /// <returns>订单明细信息</returns>
    Task<OrderItem?> GetByIdAsync(int id);

    /// <summary>
    /// 根据订单ID获取订单明细列表
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <returns>订单明细列表</returns>
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// 根据商品ID获取订单明细列表
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>订单明细列表</returns>
    Task<IEnumerable<OrderItem>> GetByProductIdAsync(int productId);

    /// <summary>
    /// 添加订单明细
    /// </summary>
    /// <param name="orderItem">订单明细信息</param>
    /// <returns>添加后的订单明细</returns>
    Task<OrderItem> AddAsync(OrderItem orderItem);

    /// <summary>
    /// 批量添加订单明细
    /// </summary>
    /// <param name="orderItems">订单明细列表</param>
    /// <returns>添加后的订单明细列表</returns>
    Task<IEnumerable<OrderItem>> AddRangeAsync(IEnumerable<OrderItem> orderItems);

    /// <summary>
    /// 更新订单明细
    /// </summary>
    /// <param name="orderItem">订单明细信息</param>
    /// <returns>更新后的订单明细</returns>
    Task<OrderItem> UpdateAsync(OrderItem orderItem);

    /// <summary>
    /// 删除订单明细
    /// </summary>
    /// <param name="id">订单明细ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 根据订单ID删除订单明细
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <returns>删除的记录数</returns>
    Task<int> DeleteByOrderIdAsync(int orderId);

    /// <summary>
    /// 检查订单明细是否存在
    /// </summary>
    /// <param name="id">订单明细ID</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 检查订单是否有明细
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <returns>是否有明细</returns>
    Task<bool> ExistsByOrderIdAsync(int orderId);
}
