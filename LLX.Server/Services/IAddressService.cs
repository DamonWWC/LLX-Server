using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;

namespace LLX.Server.Services;

/// <summary>
/// 地址服务接口
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// 获取所有地址
    /// </summary>
    /// <returns>地址列表响应</returns>
    Task<ApiResponse<IEnumerable<AddressDto>>> GetAllAddressesAsync();

    /// <summary>
    /// 根据ID获取地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>地址响应</returns>
    Task<ApiResponse<AddressDto?>> GetAddressByIdAsync(int id);

    /// <summary>
    /// 获取默认地址
    /// </summary>
    /// <returns>地址响应</returns>
    Task<ApiResponse<AddressDto?>> GetDefaultAddressAsync();

    /// <summary>
    /// 根据手机号获取地址列表
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>地址列表响应</returns>
    Task<ApiResponse<IEnumerable<AddressDto>>> GetAddressesByPhoneAsync(string phone);

    /// <summary>
    /// 创建地址
    /// </summary>
    /// <param name="createDto">创建地址DTO</param>
    /// <returns>地址响应</returns>
    Task<ApiResponse<AddressDto>> CreateAddressAsync(CreateAddressDto createDto);

    /// <summary>
    /// 更新地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <param name="updateDto">更新地址DTO</param>
    /// <returns>地址响应</returns>
    Task<ApiResponse<AddressDto>> UpdateAddressAsync(int id, UpdateAddressDto updateDto);

    /// <summary>
    /// 删除地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> DeleteAddressAsync(int id);

    /// <summary>
    /// 设置默认地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> SetDefaultAddressAsync(int id);

    /// <summary>
    /// 智能解析地址
    /// </summary>
    /// <param name="fullAddress">完整地址字符串</param>
    /// <returns>解析后的地址响应</returns>
    Task<ApiResponse<ParsedAddressDto>> ParseAddressAsync(string fullAddress);
}
