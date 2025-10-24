using LLX.Server.Data;
using LLX.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LLX.Server.Repositories;

/// <summary>
/// 订单明细仓储实现
/// </summary>
public class OrderItemRepository : IOrderItemRepository
{
    private readonly AppDbContext _context;

    public OrderItemRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有订单明细
    /// </summary>
    /// <returns>订单明细列表</returns>
    public async Task<IEnumerable<OrderItem>> GetAllAsync()
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .OrderByDescending(oi => oi.Id)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取订单明细
    /// </summary>
    /// <param name="id">订单明细ID</param>
    /// <returns>订单明细信息</returns>
    public async Task<OrderItem?> GetByIdAsync(int id)
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .FirstOrDefaultAsync(oi => oi.Id == id);
    }

    /// <summary>
    /// 根据订单ID获取订单明细列表
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <returns>订单明细列表</returns>
    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => oi.OrderId == orderId)
            .OrderBy(oi => oi.Id)
            .ToListAsync();
    }

    /// <summary>
    /// 根据商品ID获取订单明细列表
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <returns>订单明细列表</returns>
    public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(int productId)
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.ProductId == productId)
            .OrderByDescending(oi => oi.Id)
            .ToListAsync();
    }

    /// <summary>
    /// 添加订单明细
    /// </summary>
    /// <param name="orderItem">订单明细信息</param>
    /// <returns>添加后的订单明细</returns>
    public async Task<OrderItem> AddAsync(OrderItem orderItem)
    {
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        return orderItem;
    }

    /// <summary>
    /// 批量添加订单明细
    /// </summary>
    /// <param name="orderItems">订单明细列表</param>
    /// <returns>添加后的订单明细列表</returns>
    public async Task<IEnumerable<OrderItem>> AddRangeAsync(IEnumerable<OrderItem> orderItems)
    {
        _context.OrderItems.AddRange(orderItems);
        await _context.SaveChangesAsync();
        return orderItems;
    }

    /// <summary>
    /// 更新订单明细
    /// </summary>
    /// <param name="orderItem">订单明细信息</param>
    /// <returns>更新后的订单明细</returns>
    public async Task<OrderItem> UpdateAsync(OrderItem orderItem)
    {
        _context.OrderItems.Update(orderItem);
        await _context.SaveChangesAsync();
        return orderItem;
    }

    /// <summary>
    /// 删除订单明细
    /// </summary>
    /// <param name="id">订单明细ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var orderItem = await _context.OrderItems.FindAsync(id);
        if (orderItem == null)
            return false;

        _context.OrderItems.Remove(orderItem);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 根据订单ID删除订单明细
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <returns>删除的记录数</returns>
    public async Task<int> DeleteByOrderIdAsync(int orderId)
    {
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        if (orderItems.Any())
        {
            _context.OrderItems.RemoveRange(orderItems);
            await _context.SaveChangesAsync();
        }

        return orderItems.Count;
    }

    /// <summary>
    /// 检查订单明细是否存在
    /// </summary>
    /// <param name="id">订单明细ID</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.OrderItems.AnyAsync(oi => oi.Id == id);
    }

    /// <summary>
    /// 检查订单是否有明细
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <returns>是否有明细</returns>
    public async Task<bool> ExistsByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems.AnyAsync(oi => oi.OrderId == orderId);
    }
}
