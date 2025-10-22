using LLX.Server.Models.Entities;

namespace LLX.Server.Repositories;

/// <summary>
/// 商品仓储接口
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// 获取所有商品
    /// </summary>
    /// <returns>商品列表</returns>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// 根据ID获取商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品信息</returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// 根据名称搜索商品
    /// </summary>
    /// <param name="name">商品名称</param>
    /// <returns>商品列表</returns>
    Task<IEnumerable<Product>> SearchByNameAsync(string name);

    /// <summary>
    /// 添加商品
    /// </summary>
    /// <param name="product">商品信息</param>
    /// <returns>添加后的商品</returns>
    Task<Product> AddAsync(Product product);

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="product">商品信息</param>
    /// <returns>更新后的商品</returns>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// 删除商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 检查商品是否存在
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 更新商品库存
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="quantity">库存数量</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateQuantityAsync(int id, int quantity);
}
