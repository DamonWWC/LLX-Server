using LLX.Server.Models.DTOs;
using LLX.Server.Services;
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
    /// <returns>商品列表</returns>
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
    /// <returns>商品信息</returns>
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
    /// <param name="name">商品名称</param>
    /// <param name="productService">商品服务</param>
    /// <returns>商品列表</returns>
    private static async Task<IResult> SearchProducts([FromQuery] string name, IProductService productService)
    {
        var result = await productService.SearchProductsAsync(name);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 创建商品
    /// </summary>
    /// <param name="createDto">创建商品DTO</param>
    /// <param name="productService">商品服务</param>
    /// <returns>创建的商品</returns>
    private static async Task<IResult> CreateProduct(CreateProductDto createDto, IProductService productService)
    {
        var result = await productService.CreateProductAsync(createDto);
        return result.Success ? Results.Created($"/api/products/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="updateDto">更新商品DTO</param>
    /// <param name="productService">商品服务</param>
    /// <returns>更新后的商品</returns>
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
    /// <returns>删除结果</returns>
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
    /// <param name="quantity">库存数量</param>
    /// <param name="productService">商品服务</param>
    /// <returns>更新结果</returns>
    private static async Task<IResult> UpdateProductQuantity(int id, [FromBody] int quantity, IProductService productService)
    {
        var result = await productService.UpdateProductQuantityAsync(id, quantity);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }
}
