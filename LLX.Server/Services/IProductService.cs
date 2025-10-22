using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;
using LLX.Server.Utils;

namespace LLX.Server.Services;

/// <summary>
/// 商品服务接口
/// </summary>
public interface IProductService
{
    /// <summary>
    /// 获取所有商品
    /// </summary>
    /// <returns>商品列表响应</returns>
    Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync();

    /// <summary>
    /// 根据ID获取商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品响应</returns>
    Task<ApiResponse<ProductDto?>> GetProductByIdAsync(int id);

    /// <summary>
    /// 根据名称搜索商品
    /// </summary>
    /// <param name="name">商品名称</param>
    /// <returns>商品列表响应</returns>
    Task<ApiResponse<IEnumerable<ProductDto>>> SearchProductsAsync(string name);

    /// <summary>
    /// 创建商品
    /// </summary>
    /// <param name="createDto">创建商品DTO</param>
    /// <returns>商品响应</returns>
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createDto);

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="updateDto">更新商品DTO</param>
    /// <returns>商品响应</returns>
    Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateDto);

    /// <summary>
    /// 删除商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> DeleteProductAsync(int id);

    /// <summary>
    /// 更新商品库存
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="quantity">库存数量</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> UpdateProductQuantityAsync(int id, int quantity);

    /// <summary>
    /// 分页获取商品列表
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDescending">是否降序</param>
    /// <param name="searchTerm">搜索词</param>
    /// <returns>分页商品列表响应</returns>
    Task<ApiResponse<QueryOptimizer.PagedResult<ProductDto>>> GetProductsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false,
        string? searchTerm = null);
}
