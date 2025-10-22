using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;

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
}
