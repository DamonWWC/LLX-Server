using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;

namespace LLX.Server.Services;

/// <summary>
/// 运费服务接口
/// </summary>
public interface IShippingService
{
    /// <summary>
    /// 获取所有运费配置
    /// </summary>
    /// <returns>运费配置列表响应</returns>
    Task<ApiResponse<IEnumerable<ShippingRateDto>>> GetAllShippingRatesAsync();

    /// <summary>
    /// 根据ID获取运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>运费配置响应</returns>
    Task<ApiResponse<ShippingRateDto?>> GetShippingRateByIdAsync(int id);

    /// <summary>
    /// 根据省份获取运费配置
    /// </summary>
    /// <param name="province">省份</param>
    /// <returns>运费配置响应</returns>
    Task<ApiResponse<ShippingRateDto?>> GetShippingRateByProvinceAsync(string province);

    /// <summary>
    /// 创建运费配置
    /// </summary>
    /// <param name="createDto">创建运费配置DTO</param>
    /// <returns>运费配置响应</returns>
    Task<ApiResponse<ShippingRateDto>> CreateShippingRateAsync(CreateShippingRateDto createDto);

    /// <summary>
    /// 更新运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <param name="updateDto">更新运费配置DTO</param>
    /// <returns>运费配置响应</returns>
    Task<ApiResponse<ShippingRateDto>> UpdateShippingRateAsync(int id, UpdateShippingRateDto updateDto);

    /// <summary>
    /// 删除运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>操作响应</returns>
    Task<ApiResponse<bool>> DeleteShippingRateAsync(int id);

    /// <summary>
    /// 计算运费
    /// </summary>
    /// <param name="province">省份</param>
    /// <param name="weight">重量</param>
    /// <returns>运费计算结果响应</returns>
    Task<ApiResponse<ShippingCalculationDto>> CalculateShippingAsync(string province, decimal weight);
}
