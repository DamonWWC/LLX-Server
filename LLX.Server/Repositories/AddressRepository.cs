using LLX.Server.Data;
using LLX.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LLX.Server.Repositories;

/// <summary>
/// 地址仓储实现
/// </summary>
public class AddressRepository : IAddressRepository
{
    private readonly AppDbContext _context;

    public AddressRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有地址
    /// </summary>
    /// <returns>地址列表</returns>
    public async Task<IEnumerable<Address>> GetAllAsync()
    {
        return await _context.Addresses
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>地址信息</returns>
    public async Task<Address?> GetByIdAsync(int id)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// 获取默认地址
    /// </summary>
    /// <returns>默认地址</returns>
    public async Task<Address?> GetDefaultAsync()
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.IsDefault);
    }

    /// <summary>
    /// 根据手机号获取地址列表
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>地址列表</returns>
    public async Task<IEnumerable<Address>> GetByPhoneAsync(string phone)
    {
        return await _context.Addresses
            .Where(a => a.Phone == phone)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 添加地址
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>添加后的地址</returns>
    public async Task<Address> AddAsync(Address address)
    {
        address.CreatedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;
        
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return address;
    }

    /// <summary>
    /// 更新地址
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>更新后的地址</returns>
    public async Task<Address> UpdateAsync(Address address)
    {
        address.UpdatedAt = DateTime.UtcNow;
        
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
        return address;
    }

    /// <summary>
    /// 删除地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address == null)
            return false;

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 设置默认地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>是否设置成功</returns>
    public async Task<bool> SetDefaultAsync(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address == null)
            return false;

        // 先取消所有默认地址
        var allAddresses = await _context.Addresses.ToListAsync();
        foreach (var addr in allAddresses)
        {
            addr.IsDefault = false;
            addr.UpdatedAt = DateTime.UtcNow;
        }

        // 设置新的默认地址
        address.IsDefault = true;
        address.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查地址是否存在
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Addresses.AnyAsync(a => a.Id == id);
    }
}
