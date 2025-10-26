using LLX.Server.Models.DTOs;
using LLX.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLX.Server.Endpoints;

/// <summary>
/// 地址端点
/// </summary>
public static class AddressEndpoints
{
    /// <summary>
    /// 注册地址端点
    /// </summary>
    /// <param name="app">Web应用程序</param>
    public static void MapAddressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/addresses")
            .WithTags("地址管理");

        // 获取所有地址
        group.MapGet("/", GetAllAddresses)
            .WithName("GetAllAddresses")
            .WithSummary("获取所有地址")
            .WithDescription("获取系统中所有地址列表")
            .Produces<ApiResponse<IEnumerable<AddressDto>>>(200);

        // 根据ID获取地址
        group.MapGet("/{id:int}", GetAddressById)
            .WithName("GetAddressById")
            .WithSummary("根据ID获取地址")
            .WithDescription("根据地址ID获取特定地址的详细信息")
            .Produces<ApiResponse<AddressDto?>>(200)
            .Produces<ApiResponse<AddressDto?>>(404);

        // 获取默认地址
        group.MapGet("/default", GetDefaultAddress)
            .WithName("GetDefaultAddress")
            .WithSummary("获取默认地址")
            .WithDescription("获取当前设置的默认地址")
            .Produces<ApiResponse<AddressDto?>>(200);

        // 根据手机号获取地址列表
        group.MapGet("/phone/{phone}", GetAddressesByPhone)
            .WithName("GetAddressesByPhone")
            .WithSummary("根据手机号获取地址列表")
            .WithDescription("根据手机号获取相关的地址列表")
            .Produces<ApiResponse<IEnumerable<AddressDto>>>(200);

        // 创建地址
        group.MapPost("/", CreateAddress)
            .WithName("CreateAddress")
            .WithSummary("创建地址")
            .WithDescription("创建新的收货地址")
            .Produces<ApiResponse<AddressDto>>(201)
            .Produces<ApiResponse<AddressDto>>(400);

        // 更新地址
        group.MapPut("/{id:int}", UpdateAddress)
            .WithName("UpdateAddress")
            .WithSummary("更新地址")
            .WithDescription("更新指定地址的信息")
            .Produces<ApiResponse<AddressDto>>(200)
            .Produces<ApiResponse<AddressDto>>(400)
            .Produces<ApiResponse<AddressDto>>(404);

        // 删除地址
        group.MapDelete("/{id:int}", DeleteAddress)
            .WithName("DeleteAddress")
            .WithSummary("删除地址")
            .WithDescription("删除指定的地址")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(404);

        // 设置默认地址
        group.MapPatch("/{id:int}/default", SetDefaultAddress)
            .WithName("SetDefaultAddress")
            .WithSummary("设置默认地址")
            .WithDescription("将指定地址设置为默认地址")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(404);

        // 智能解析地址
        group.MapPost("/parse", ParseAddress)
            .WithName("ParseAddress")
            .WithSummary("智能解析地址")
            .WithDescription("智能解析地址文本，提取省市区和详细信息")
            .Produces<ApiResponse<ParsedAddressDto>>(200)
            .Produces<ApiResponse<ParsedAddressDto>>(400);
    }

    /// <summary>
    /// 获取所有地址
    /// </summary>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回系统中所有地址列表</returns>
    /// <response code="200">成功返回地址列表</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> GetAllAddresses(IAddressService addressService)
    {
        var result = await addressService.GetAllAddressesAsync();
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 根据ID获取地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回指定ID的地址详细信息</returns>
    /// <response code="200">成功返回地址信息</response>
    /// <response code="404">地址不存在</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> GetAddressById(int id, IAddressService addressService)
    {
        var result = await addressService.GetAddressByIdAsync(id);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 获取默认地址
    /// </summary>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回当前设置的默认地址</returns>
    /// <response code="200">成功返回默认地址</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> GetDefaultAddress(IAddressService addressService)
    {
        var result = await addressService.GetDefaultAddressAsync();
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 根据手机号获取地址列表
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回使用指定手机号的所有地址列表</returns>
    /// <response code="200">成功返回地址列表</response>
    /// <response code="400">请求参数错误</response>
    private static async Task<IResult> GetAddressesByPhone(string phone, IAddressService addressService)
    {
        var result = await addressService.GetAddressesByPhoneAsync(phone);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 创建地址
    /// </summary>
    /// <param name="createDto">创建地址的数据传输对象</param>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回新创建的地址信息</returns>
    /// <response code="201">成功创建地址</response>
    /// <response code="400">请求参数错误或地址信息无效</response>
    private static async Task<IResult> CreateAddress(CreateAddressDto createDto, IAddressService addressService)
    {
        var result = await addressService.CreateAddressAsync(createDto);
        return result.Success ? Results.Created($"/api/addresses/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 更新地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <param name="updateDto">更新地址的数据传输对象</param>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回更新后的地址信息</returns>
    /// <response code="200">成功更新地址</response>
    /// <response code="400">请求参数错误</response>
    /// <response code="404">地址不存在</response>
    private static async Task<IResult> UpdateAddress(int id, UpdateAddressDto updateDto, IAddressService addressService)
    {
        var result = await addressService.UpdateAddressAsync(id, updateDto);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 删除地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回删除操作的结果</returns>
    /// <response code="200">成功删除地址</response>
    /// <response code="404">地址不存在</response>
    /// <response code="400">删除操作失败</response>
    private static async Task<IResult> DeleteAddress(int id, IAddressService addressService)
    {
        var result = await addressService.DeleteAddressAsync(id);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 设置默认地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回设置默认地址操作的结果</returns>
    /// <response code="200">成功设置默认地址</response>
    /// <response code="404">地址不存在</response>
    /// <response code="400">设置操作失败</response>
    private static async Task<IResult> SetDefaultAddress(int id, IAddressService addressService)
    {
        var result = await addressService.SetDefaultAddressAsync(id);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 智能解析地址
    /// </summary>
    /// <param name="request">解析请求</param>
    /// <param name="addressService">地址服务</param>
    /// <returns>返回解析后的地址信息，包含姓名、手机号、省市区和详细地址</returns>
    /// <response code="200">成功解析地址</response>
    /// <response code="400">请求参数错误或解析失败</response>
    private static async Task<IResult> ParseAddress([FromBody] ParseAddressRequest request, IAddressService addressService)
    {
        var result = await addressService.ParseAddressAsync(request.FullAddress);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 解析地址请求
    /// </summary>
    public class ParseAddressRequest
    {
        public string FullAddress { get; set; } = string.Empty;
    }
}
