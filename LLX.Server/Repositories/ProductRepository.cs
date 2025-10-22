using LLX.Server.Data;
using LLX.Server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LLX.Server.Repositories;

/// <summary>
/// 商品仓储实现
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有商品
    /// </summary>
    /// <returns>商品列表</returns>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品信息</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// 根据名称搜索商品
    /// </summary>
    /// <param name="name">商品名称</param>
    /// <returns>商品列表</returns>
    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        return await _context.Products
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 添加商品
    /// </summary>
    /// <param name="product">商品信息</param>
    /// <returns>添加后的商品</returns>
    public async Task<Product> AddAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="product">商品信息</param>
    /// <returns>更新后的商品</returns>
    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// 删除商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 检查商品是否存在
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// 更新商品库存
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="quantity">库存数量</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateQuantityAsync(int id, int quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        product.Quantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
