using LLX.Server.Data;
using LLX.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LLX.Server.Repositories;

/// <summary>
/// 运费仓储实现
/// </summary>
public class ShippingRepository : IShippingRepository
{
    private readonly AppDbContext _context;

    public ShippingRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有运费配置
    /// </summary>
    /// <returns>运费配置列表</returns>
    public async Task<IEnumerable<ShippingRate>> GetAllAsync()
    {
        return await _context.ShippingRates
            .OrderBy(s => s.Province)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>运费配置信息</returns>
    public async Task<ShippingRate?> GetByIdAsync(int id)
    {
        return await _context.ShippingRates
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// 根据省份获取运费配置
    /// </summary>
    /// <param name="province">省份</param>
    /// <returns>运费配置信息</returns>
    public async Task<ShippingRate?> GetByProvinceAsync(string province)
    {
        return await _context.ShippingRates
            .FirstOrDefaultAsync(s => s.Province == province);
    }

    /// <summary>
    /// 添加运费配置
    /// </summary>
    /// <param name="shippingRate">运费配置信息</param>
    /// <returns>添加后的运费配置</returns>
    public async Task<ShippingRate> AddAsync(ShippingRate shippingRate)
    {
        shippingRate.CreatedAt = DateTime.UtcNow;
        shippingRate.UpdatedAt = DateTime.UtcNow;
        
        _context.ShippingRates.Add(shippingRate);
        await _context.SaveChangesAsync();
        return shippingRate;
    }

    /// <summary>
    /// 更新运费配置
    /// </summary>
    /// <param name="shippingRate">运费配置信息</param>
    /// <returns>更新后的运费配置</returns>
    public async Task<ShippingRate> UpdateAsync(ShippingRate shippingRate)
    {
        shippingRate.UpdatedAt = DateTime.UtcNow;
        
        _context.ShippingRates.Update(shippingRate);
        await _context.SaveChangesAsync();
        return shippingRate;
    }

    /// <summary>
    /// 删除运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var shippingRate = await _context.ShippingRates.FindAsync(id);
        if (shippingRate == null)
            return false;

        _context.ShippingRates.Remove(shippingRate);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查运费配置是否存在
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ShippingRates.AnyAsync(s => s.Id == id);
    }

    /// <summary>
    /// 检查省份运费配置是否存在
    /// </summary>
    /// <param name="province">省份</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsByProvinceAsync(string province)
    {
        return await _context.ShippingRates.AnyAsync(s => s.Province == province);
    }
}
