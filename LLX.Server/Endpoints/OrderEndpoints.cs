using LLX.Server.Models.DTOs;
using LLX.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace LLX.Server.Endpoints;

/// <summary>
/// 订单端点
/// </summary>
public static class OrderEndpoints
{
    /// <summary>
    /// 注册订单端点
    /// </summary>
    /// <param name="app">Web应用程序</param>
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("订单管理");

        // 获取所有订单
        group.MapGet("/", GetAllOrders)
            .WithName("GetAllOrders")
            .WithSummary("获取所有订单")
            .WithDescription("获取系统中所有订单列表")
            .Produces<ApiResponse<IEnumerable<OrderDto>>>(200);

        // 根据ID获取订单
        group.MapGet("/{id:int}", GetOrderById)
            .WithName("GetOrderById")
            .WithSummary("根据ID获取订单")
            .WithDescription("根据订单ID获取特定订单的详细信息")
            .Produces<ApiResponse<OrderDto?>>(200)
            .Produces<ApiResponse<OrderDto?>>(404);

        // 根据订单号获取订单
        group.MapGet("/order-no/{orderNo}", GetOrderByOrderNo)
            .WithName("GetOrderByOrderNo")
            .WithSummary("根据订单号获取订单")
            .WithDescription("根据订单号获取特定订单的详细信息")
            .Produces<ApiResponse<OrderDto?>>(200)
            .Produces<ApiResponse<OrderDto?>>(404);

        // 根据状态获取订单列表
        group.MapGet("/status/{status}", GetOrdersByStatus)
            .WithName("GetOrdersByStatus")
            .WithSummary("根据状态获取订单列表")
            .WithDescription("根据订单状态获取订单列表")
            .Produces<ApiResponse<IEnumerable<OrderDto>>>(200);

        // 根据地址ID获取订单列表
        group.MapGet("/address/{addressId:int}", GetOrdersByAddressId)
            .WithName("GetOrdersByAddressId")
            .WithSummary("根据地址ID获取订单列表")
            .WithDescription("根据地址ID获取相关订单列表")
            .Produces<ApiResponse<IEnumerable<OrderDto>>>(200);

        // 创建订单
        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("创建订单")
            .WithDescription("创建新的订单")
            .Produces<ApiResponse<OrderDto>>(201)
            .Produces<ApiResponse<OrderDto>>(400);

        // 更新订单状态
        group.MapPatch("/{id:int}/status", UpdateOrderStatus)
            .WithName("UpdateOrderStatus")
            .WithSummary("更新订单状态")
            .WithDescription("更新指定订单的状态")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(400)
            .Produces<ApiResponse<bool>>(404);

        // 更新支付状态
        group.MapPatch("/{id:int}/payment-status", UpdatePaymentStatus)
            .WithName("UpdatePaymentStatus")
            .WithSummary("更新支付状态")
            .WithDescription("更新指定订单的支付状态")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(400)
            .Produces<ApiResponse<bool>>(404);

        // 删除订单
        group.MapDelete("/{id:int}", DeleteOrder)
            .WithName("DeleteOrder")
            .WithSummary("删除订单")
            .WithDescription("删除指定的订单")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(404);

        // 批量删除订单
        group.MapPost("/batch/delete", DeleteOrders)
            .WithName("DeleteOrders")
            .WithSummary("批量删除订单")
            .WithDescription("批量删除多个订单")
            .Produces<ApiResponse<bool>>(200)
            .Produces<ApiResponse<bool>>(400);

        // 计算订单
        group.MapPost("/calculate", CalculateOrder)
            .WithName("CalculateOrder")
            .WithSummary("计算订单")
            .WithDescription("计算订单的总金额和运费")
            .Produces<ApiResponse<OrderCalculationDto>>(200)
            .Produces<ApiResponse<OrderCalculationDto>>(400);
    }

    /// <summary>
    /// 获取所有订单
    /// </summary>
    /// <param name="orderService">订单服务</param>
    /// <returns>订单列表</returns>
    private static async Task<IResult> GetAllOrders(IOrderService orderService)
    {
        var result = await orderService.GetAllOrdersAsync();
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 根据ID获取订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>订单信息</returns>
    private static async Task<IResult> GetOrderById(int id, IOrderService orderService)
    {
        var result = await orderService.GetOrderByIdAsync(id);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 根据订单号获取订单
    /// </summary>
    /// <param name="orderNo">订单号</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>订单信息</returns>
    private static async Task<IResult> GetOrderByOrderNo(string orderNo, IOrderService orderService)
    {
        var result = await orderService.GetOrderByOrderNoAsync(orderNo);
        if (!result.Success)
        {
            return result.Data == null ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 根据状态获取订单列表
    /// </summary>
    /// <param name="status">订单状态</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>订单列表</returns>
    private static async Task<IResult> GetOrdersByStatus(string status, IOrderService orderService)
    {
        var result = await orderService.GetOrdersByStatusAsync(status);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 根据地址ID获取订单列表
    /// </summary>
    /// <param name="addressId">地址ID</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>订单列表</returns>
    private static async Task<IResult> GetOrdersByAddressId(int addressId, IOrderService orderService)
    {
        var result = await orderService.GetOrdersByAddressIdAsync(addressId);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="createDto">创建订单DTO</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>创建的订单</returns>
    private static async Task<IResult> CreateOrder(CreateOrderDto createDto, IOrderService orderService)
    {
        var result = await orderService.CreateOrderAsync(createDto);
        return result.Success ? Results.Created($"/api/orders/{result.Data?.Id}", result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 更新订单状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="request">状态更新请求</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>更新结果</returns>
    private static async Task<IResult> UpdateOrderStatus(int id, UpdateStatusRequest request, IOrderService orderService)
    {
        var result = await orderService.UpdateOrderStatusAsync(id, request.Status);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 更新支付状态
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="request">支付状态更新请求</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>更新结果</returns>
    private static async Task<IResult> UpdatePaymentStatus(int id, UpdatePaymentStatusRequest request, IOrderService orderService)
    {
        var result = await orderService.UpdatePaymentStatusAsync(id, request.PaymentStatus);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 删除订单
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>删除结果</returns>
    private static async Task<IResult> DeleteOrder(int id, IOrderService orderService)
    {
        var result = await orderService.DeleteOrderAsync(id);
        if (!result.Success)
        {
            return result.Data == false ? Results.NotFound(result) : Results.BadRequest(result);
        }
        return Results.Ok(result);
    }

    /// <summary>
    /// 批量删除订单
    /// </summary>
    /// <param name="request">批量删除请求</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>删除结果</returns>
    private static async Task<IResult> DeleteOrders(DeleteOrdersRequest request, IOrderService orderService)
    {
        var result = await orderService.DeleteOrdersAsync(request.Ids);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 计算订单
    /// </summary>
    /// <param name="request">计算请求</param>
    /// <param name="orderService">订单服务</param>
    /// <returns>计算结果</returns>
    private static async Task<IResult> CalculateOrder(CalculateOrderRequest request, IOrderService orderService)
    {
        var result = await orderService.CalculateOrderAsync(request.Items, request.AddressId);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// 计算订单请求
    /// </summary>
    public class CalculateOrderRequest
    {
        public List<CreateOrderItemDto> Items { get; set; } = new();
        public int AddressId { get; set; }
    }

    /// <summary>
    /// 批量删除订单请求
    /// </summary>
    public class DeleteOrdersRequest
    {
        public List<int> Ids { get; set; } = new();
    }
}
