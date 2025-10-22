using LLX.Server.Models.DTOs;
using LLX.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLX.Server.Endpoints;

/// <summary>
/// 运费端点
/// </summary>
public static class ShippingEndpoints
{
    /// <summary>
    /// 注册运费端点
    /// </summary>
    /// <param name="app">Web应用程序</param>
    public static void MapShippingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/shipping")
            .WithTags("运费管理");

        // 获取所有运费配置
        group.MapGet("/rates", GetAllShippingRates)
            .WithName("GetAllShippingRates")
            .WithSummary("获取所有运费配置")
            .WithDescription("获取系统中所有运费配置列表")
            .Produces<ApiResponse<IEnumerable<ShippingRateDto>>>(200);

        // 根据ID获取运费配置
        group.MapGet("/rates/{id:int}", GetShippingRateById)
            .WithName("GetShippingRateById")
            .WithSummary("根据ID获取运费配置")
            .WithDescription("根据运费配置ID获取特定配置的详细信息")
            .Produces<ApiResponse<ShippingRateDto?>>(200)
            .Produces<ApiResponse<ShippingRateDto?>>(404);

        // 根据省份获取运费配置
        group.MapGet("/rates/province/{province}", GetShippingRateByProvince)
            .WithName("GetShippingRateByProvince")
            .WithSummary("根据省份获取运费配置")
            .WithDescription("根据省份获取对应的运费配置")
            .Produces<ApiResponse<ShippingRateDto?>>(200)
            .Produces<ApiResponse<ShippingRateDto?>>(404);

        // 创建运费配置
        group.MapPost("/rates", CreateShippingRate)
            .WithName("CreateShippingRate")
            .WithSummary("创建运费配置")
            .WithDescription("创建新的运费配置")
            .Produces<ApiResponse<ShippingRateDto>>(201)
            .Produces<ApiResponse<ShippingRateDto>>(400);

        // 更新运费配置
        group.MapPut("/rates/{id:int}", UpdateShippingRate)
            .WithName("UpdateShippingRate")
            .WithSummary("更新运费配置")
            .WithDescription("更新指定运费配置的信息")
            .Produces<ApiResponse<ShippingRateDto>>(200)
            .Produces<ApiResponse<ShippingRateDto>>(400)
            .Produces<ApiResponse<ShippingRateDto>>(404);

        // 删除运费配置
        group.MapDelete("/rates/{id:int}", DeleteShippingRate)
            .WithName("DeleteShippingRate")
            .WithSummary("删除运费配置")
            .WithDescription("删除指定的运费配置")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(404);

        // 计算运费
        group.MapPost("/calculate", CalculateShipping)
            .WithName("CalculateShipping")
            .WithSummary("计算运费")
            .WithDescription("根据省份和重量计算运费")
            .Produces<ApiResponse<ShippingCalculationDto>>(200)
            .Produces<ApiResponse<ShippingCalculationDto>>(400);
    }

    /// <summary>
    /// 获取所有运费配置
    /// </summary>
    /// <param name="shippingService">运费服务</param>
    /// <returns>运费配置列表</returns>
    private static async Task<IResult> GetAllShippingRates(IShippingService shippingService)
    {
        var result = await shippingService.GetAllShippingRatesAsync();
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 根据ID获取运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <param name="shippingService">运费服务</param>
    /// <returns>运费配置信息</returns>
    private static async Task<IResult> GetShippingRateById(int id, IShippingService shippingService)
    {
        var result = await shippingService.GetShippingRateByIdAsync(id);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 根据省份获取运费配置
    /// </summary>
    /// <param name="province">省份</param>
    /// <param name="shippingService">运费服务</param>
    /// <returns>运费配置信息</returns>
    private static async Task<IResult> GetShippingRateByProvince(string province, IShippingService shippingService)
    {
        var result = await shippingService.GetShippingRateByProvinceAsync(province);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 创建运费配置
    /// </summary>
    /// <param name="createDto">创建运费配置DTO</param>
    /// <param name="shippingService">运费服务</param>
    /// <returns>创建的运费配置</returns>
    private static async Task<IResult> CreateShippingRate(CreateShippingRateDto createDto, IShippingService shippingService)
    {
        var result = await shippingService.CreateShippingRateAsync(createDto);
        return result.Success ? Results.Created($"/api/shipping/rates/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 更新运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <param name="updateDto">更新运费配置DTO</param>
    /// <param name="shippingService">运费服务</param>
    /// <returns>更新后的运费配置</returns>
    private static async Task<IResult> UpdateShippingRate(int id, UpdateShippingRateDto updateDto, IShippingService shippingService)
    {
        var result = await shippingService.UpdateShippingRateAsync(id, updateDto);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 删除运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <param name="shippingService">运费服务</param>
    /// <returns>删除结果</returns>
    private static async Task<IResult> DeleteShippingRate(int id, IShippingService shippingService)
    {
        var result = await shippingService.DeleteShippingRateAsync(id);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 计算运费
    /// </summary>
    /// <param name="request">计算请求</param>
    /// <param name="shippingService">运费服务</param>
    /// <returns>计算结果</returns>
    private static async Task<IResult> CalculateShipping(CalculateShippingRequest request, IShippingService shippingService)
    {
        var result = await shippingService.CalculateShippingAsync(request.Province, request.Weight);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 计算运费请求
    /// </summary>
    public class CalculateShippingRequest
    {
        public string Province { get; set; } = string.Empty;
        public decimal Weight { get; set; }
    }
}
