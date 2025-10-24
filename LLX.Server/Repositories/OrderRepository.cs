using LLX.Server.Data;
using LLX.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LLX.Server.Repositories;

/// <summary>
/// 订单仓储实现
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有订单
    /// </summary>
    /// <returns>订单列表</returns>
    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>订单信息</returns>
    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// 根据订单号获取订单
    /// </summary>
    /// <param name="orderNo">订单号</param>
    /// <returns>订单信息</returns>
    public async Task<Order?> GetByOrderNoAsync(string orderNo)
    {
        return await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNo == orderNo);
    }

    /// <summary>
    /// 根据状态获取订单列表
    /// </summary>
    /// <param name="status">订单状态</param>
    /// <returns>订单列表</returns>
    public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
    {
        return await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 根据地址ID获取订单列表
    /// </summary>
    /// <param name="addressId">地址ID</param>
    /// <returns>订单列表</returns>
    public async Task<IEnumerable<Order>> GetByAddressIdAsync(int addressId)
    {
        return await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
            .Where(o => o.AddressId == addressId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 添加订单
    /// </summary>
    /// <param name="order">订单信息</param>
    /// <returns>添加后的订单</returns>
    public async Task<Order> AddAsync(Order order)
    {
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// 更新订单
    /// </summary>
    /// <param name="order">订单信息</param>
    /// <returns>更新后的订单</returns>
    public async Task<Order> UpdateAsync(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// 删除订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 批量删除订单
    /// </summary>
    /// <param name="ids">订单ID列表</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAllAsync(List<int> ids)
    {
        if (ids == null || !ids.Any())
            return false;

        // 批量查询要删除的订单
        var orders = await _context.Orders
            .Where(o => ids.Contains(o.Id))
            .ToListAsync();

        if (!orders.Any())
            return false;

        // 批量删除订单（级联删除订单明细）
        _context.Orders.RemoveRange(orders);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 更新订单状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="status">新状态</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 更新支付状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="paymentStatus">支付状态</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        order.PaymentStatus = paymentStatus;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查订单是否存在
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Orders.AnyAsync(o => o.Id == id);
    }

    /// <summary>
    /// 检查订单号是否存在
    /// </summary>
    /// <param name="orderNo">订单号</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsByOrderNoAsync(string orderNo)
    {
        return await _context.Orders.AnyAsync(o => o.OrderNo == orderNo);
    }
}
