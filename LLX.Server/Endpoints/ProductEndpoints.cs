using LLX.Server.Models.DTOs;
using LLX.Server.Services;
using LLX.Server.Utils;
using Microsoft.AspNetCore.Mvc;

namespace LLX.Server.Endpoints;

/// <summary>
/// 商品端点
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// 注册商品端点
    /// </summary>
    /// <param name="app">Web应用程序</param>
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("商品管理");

        // 获取所有商品
        group.MapGet("/", GetAllProducts)
            .WithName("GetAllProducts")
            .WithSummary("获取所有商品")
            .WithDescription("获取系统中所有可用的商品列表")
            .Produces<ApiResponse<IEnumerable<ProductDto>>>(200);

        // 根据ID获取商品
        group.MapGet("/{id:int}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("根据ID获取商品")
            .WithDescription("根据商品ID获取特定商品的详细信息")
            .Produces<ApiResponse<ProductDto?>>(200)
            .Produces<ApiResponse<ProductDto?>>(404);

        // 搜索商品
        group.MapGet("/search", SearchProducts)
            .WithName("SearchProducts")
            .WithSummary("搜索商品")
            .WithDescription("根据商品名称搜索商品")
            .Produces<ApiResponse<IEnumerable<ProductDto>>>(200);

        // 分页获取商品
        group.MapGet("/paged", GetProductsPaged)
            .WithName("GetProductsPaged")
            .WithSummary("分页获取商品")
            .WithDescription("分页获取商品列表，支持排序和搜索")
            .Produces<ApiResponse<QueryOptimizer.PagedResult<ProductDto>>>(200);

        // 创建商品
        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("创建商品")
            .WithDescription("创建新的商品")
            .Produces<ApiResponse<ProductDto>>(201)
            .Produces<ApiResponse<ProductDto>>(400);

        // 更新商品
        group.MapPut("/{id:int}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("更新商品")
            .WithDescription("更新指定商品的信息")
            .Produces<ApiResponse<ProductDto>>(200)
            .Produces<ApiResponse<ProductDto>>(400)
            .Produces<ApiResponse<ProductDto>>(404);

        // 删除商品
        group.MapDelete("/{id:int}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("删除商品")
            .WithDescription("删除指定的商品")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(404);

        // 更新商品库存
        group.MapPatch("/{id:int}/quantity", UpdateProductQuantity)
            .WithName("UpdateProductQuantity")
            .WithSummary("更新商品库存")
            .WithDescription("更新指定商品的库存数量")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(400)
            .Produces<ApiResponse<bool>>(404);
    }

    /// <summary>
    /// 获取所有商品
    /// </summary>
    /// <param name="productService">商品服务</param>
    /// <returns>返回系统中所有可用的商品列表，包括商品的基本信息如名称、价格、库存等</returns>
    /// <response code="200">成功返回商品列表</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> GetAllProducts(IProductService productService)
    {
        var result = await productService.GetAllProductsAsync();
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 根据ID获取商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="productService">商品服务</param>
    /// <returns>返回指定ID的商品详细信息，包括价格、库存、图片等完整信息</returns>
    /// <response code="200">成功返回商品信息</response>
    /// <response code="404">商品不存在</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> GetProductById(int id, IProductService productService)
    {
        var result = await productService.GetProductByIdAsync(id);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 搜索商品
    /// </summary>
    /// <param name="name">商品名称关键词</param>
    /// <param name="productService">商品服务</param>
    /// <returns>返回包含指定关键词的商品列表，支持模糊匹配</returns>
    /// <response code="200">成功返回搜索结果</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> SearchProducts([FromQuery] string name, IProductService productService)
    {
        var result = await productService.SearchProductsAsync(name);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 创建商品
    /// </summary>
    /// <param name="createDto">创建商品的数据传输对象</param>
    /// <param name="productService">商品服务</param>
    /// <returns>返回新创建的商品信息，包含自动生成的ID和时间戳</returns>
    /// <response code="201">成功创建商品</response>
    /// <response code="400">请求参数错误或商品信息无效</response>
    private static async Task<IResult> CreateProduct(CreateProductDto createDto, IProductService productService)
    {
        var result = await productService.CreateProductAsync(createDto);
        return result.Success ? Results.Created($"/api/products/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="updateDto">更新商品的数据传输对象</param>
    /// <param name="productService">商品服务</param>
    /// <returns>返回更新后的商品信息</returns>
    /// <response code="200">成功更新商品</response>
    /// <response code="400">请求参数错误</response>
    /// <response code="404">商品不存在</response>
    private static async Task<IResult> UpdateProduct(int id, UpdateProductDto updateDto, IProductService productService)
    {
        var result = await productService.UpdateProductAsync(id, updateDto);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 删除商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="productService">商品服务</param>
    /// <returns>返回删除操作的结果</returns>
    /// <response code="200">成功删除商品</response>
    /// <response code="404">商品不存在</response>
    /// <response code="400">删除操作失败</response>
    private static async Task<IResult> DeleteProduct(int id, IProductService productService)
    {
        var result = await productService.DeleteProductAsync(id);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 更新商品库存
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="quantity">新的库存数量</param>
    /// <param name="productService">商品服务</param>
    /// <returns>返回库存更新操作的结果</returns>
    /// <response code="200">成功更新库存</response>
    /// <response code="400">请求参数错误</response>
    /// <response code="404">商品不存在</response>
    private static async Task<IResult> UpdateProductQuantity(int id, [FromBody] int quantity, IProductService productService)
    {
        var result = await productService.UpdateProductQuantityAsync(id, quantity);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 分页获取商品
    /// </summary>
    /// <param name="pageNumber">页码，从1开始</param>
    /// <param name="pageSize">每页大小，默认20</param>
    /// <param name="sortBy">排序字段，可选值：name, price, createdAt等</param>
    /// <param name="sortDescending">是否降序排列</param>
    /// <param name="searchTerm">搜索关键词，支持商品名称模糊搜索</param>
    /// <param name="productService">商品服务</param>
    /// <returns>返回分页的商品列表，包含总数、当前页、总页数等信息</returns>
    /// <response code="200">成功返回分页结果</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> GetProductsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? searchTerm = null,
        IProductService? productService = null)
    {
        if (productService == null)
        {
            return Results.Problem("Product service not available");
        }

        var result = await productService.GetProductsPagedAsync(
            pageNumber, pageSize, sortBy, sortDescending, searchTerm);

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    }
}
