using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;
using LLX.Server.Repositories;
using Microsoft.Extensions.Logging;

namespace LLX.Server.Services;

/// <summary>
/// 运费服务实现
/// </summary>
public class ShippingService : IShippingService
{
    private readonly IShippingRepository _shippingRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ShippingService> _logger;

    private const string CACHE_KEY_PREFIX = "shipping:";
    private const string CACHE_KEY_ALL = "shipping:all";
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1); // 运费配置缓存时间更长

    public ShippingService(
        IShippingRepository shippingRepository,
        ICacheService cacheService,
        ILogger<ShippingService> logger)
    {
        _shippingRepository = shippingRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有运费配置
    /// </summary>
    /// <returns>运费配置列表响应</returns>
    public async Task<ApiResponse<IEnumerable<ShippingRateDto>>> GetAllShippingRatesAsync()
    {
        try
        {
            _logger.LogInformation("Getting all shipping rates");

            // 尝试从缓存获取
            var cachedRates = await _cacheService.GetAsync<IEnumerable<ShippingRateDto>>(CACHE_KEY_ALL);
            if (cachedRates != null)
            {
                _logger.LogInformation("Retrieved {Count} shipping rates from cache", cachedRates.Count());
                return ApiResponse<IEnumerable<ShippingRateDto>>.SuccessResponse(cachedRates, "获取运费配置列表成功");
            }

            // 从数据库获取
            var rates = await _shippingRepository.GetAllAsync();
            var rateDtos = rates.Select(MapToDto).ToList();

            // 缓存结果
            await _cacheService.SetAsync(CACHE_KEY_ALL, rateDtos, _cacheExpiry);

            _logger.LogInformation("Retrieved {Count} shipping rates from database", rateDtos.Count);
            return ApiResponse<IEnumerable<ShippingRateDto>>.SuccessResponse(rateDtos, "获取运费配置列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all shipping rates");
            return ApiResponse<IEnumerable<ShippingRateDto>>.ErrorResponse("获取运费配置列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>运费配置响应</returns>
    public async Task<ApiResponse<ShippingRateDto?>> GetShippingRateByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting shipping rate by ID: {ShippingRateId}", id);

            // 尝试从缓存获取
            var cacheKey = $"{CACHE_KEY_PREFIX}{id}";
            var cachedRate = await _cacheService.GetAsync<ShippingRateDto>(cacheKey);
            if (cachedRate != null)
            {
                _logger.LogInformation("Retrieved shipping rate {ShippingRateId} from cache", id);
                return ApiResponse<ShippingRateDto?>.SuccessResponse(cachedRate, "获取运费配置成功");
            }

            // 从数据库获取
            var rate = await _shippingRepository.GetByIdAsync(id);
            if (rate == null)
            {
                _logger.LogWarning("Shipping rate {ShippingRateId} not found", id);
                return ApiResponse<ShippingRateDto?>.ErrorResponse("运费配置不存在");
            }

            var rateDto = MapToDto(rate);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, rateDto, _cacheExpiry);

            _logger.LogInformation("Retrieved shipping rate {ShippingRateId} from database", id);
            return ApiResponse<ShippingRateDto?>.SuccessResponse(rateDto, "获取运费配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipping rate by ID: {ShippingRateId}", id);
            return ApiResponse<ShippingRateDto?>.ErrorResponse("获取运费配置失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据省份获取运费配置
    /// </summary>
    /// <param name="province">省份</param>
    /// <returns>运费配置响应</returns>
    public async Task<ApiResponse<ShippingRateDto?>> GetShippingRateByProvinceAsync(string province)
    {
        try
        {
            _logger.LogInformation("Getting shipping rate by province: {Province}", province);

            if (string.IsNullOrWhiteSpace(province))
            {
                return ApiResponse<ShippingRateDto?>.ErrorResponse("省份不能为空");
            }

            // 尝试从缓存获取
            var cacheKey = $"{CACHE_KEY_PREFIX}province:{province}";
            var cachedRate = await _cacheService.GetAsync<ShippingRateDto>(cacheKey);
            if (cachedRate != null)
            {
                _logger.LogInformation("Retrieved shipping rate for {Province} from cache", province);
                return ApiResponse<ShippingRateDto?>.SuccessResponse(cachedRate, "获取运费配置成功");
            }

            // 从数据库获取
            var rate = await _shippingRepository.GetByProvinceAsync(province);
            if (rate == null)
            {
                _logger.LogWarning("Shipping rate for {Province} not found", province);
                return ApiResponse<ShippingRateDto?>.ErrorResponse("该省份的运费配置不存在");
            }

            var rateDto = MapToDto(rate);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, rateDto, _cacheExpiry);

            _logger.LogInformation("Retrieved shipping rate for {Province} from database", province);
            return ApiResponse<ShippingRateDto?>.SuccessResponse(rateDto, "获取运费配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shipping rate by province: {Province}", province);
            return ApiResponse<ShippingRateDto?>.ErrorResponse("获取运费配置失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 创建运费配置
    /// </summary>
    /// <param name="createDto">创建运费配置DTO</param>
    /// <returns>运费配置响应</returns>
    public async Task<ApiResponse<ShippingRateDto>> CreateShippingRateAsync(CreateShippingRateDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating shipping rate for province: {Province}", createDto.Province);

            // 验证输入
            var validationErrors = ValidateCreateShippingRate(createDto);
            if (validationErrors.Any())
            {
                return ApiResponse<ShippingRateDto>.ErrorResponse("输入验证失败", validationErrors);
            }

            // 检查省份是否已存在
            var existingRate = await _shippingRepository.GetByProvinceAsync(createDto.Province);
            if (existingRate != null)
            {
                return ApiResponse<ShippingRateDto>.ErrorResponse("该省份的运费配置已存在");
            }

            var rate = new ShippingRate
            {
                Province = createDto.Province,
                Rate = createDto.Rate
            };

            var createdRate = await _shippingRepository.AddAsync(rate);
            var rateDto = MapToDto(createdRate);

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveByPatternAsync($"{CACHE_KEY_PREFIX}province:*");

            _logger.LogInformation("Created shipping rate {ShippingRateId} for province {Province}", createdRate.Id, createdRate.Province);
            return ApiResponse<ShippingRateDto>.SuccessResponse(rateDto, "创建运费配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipping rate for province: {Province}", createDto.Province);
            return ApiResponse<ShippingRateDto>.ErrorResponse("创建运费配置失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 更新运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <param name="updateDto">更新运费配置DTO</param>
    /// <returns>运费配置响应</returns>
    public async Task<ApiResponse<ShippingRateDto>> UpdateShippingRateAsync(int id, UpdateShippingRateDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating shipping rate {ShippingRateId}", id);

            // 验证输入
            var validationErrors = ValidateUpdateShippingRate(updateDto);
            if (validationErrors.Any())
            {
                return ApiResponse<ShippingRateDto>.ErrorResponse("输入验证失败", validationErrors);
            }

            // 检查运费配置是否存在
            var existingRate = await _shippingRepository.GetByIdAsync(id);
            if (existingRate == null)
            {
                _logger.LogWarning("Shipping rate {ShippingRateId} not found for update", id);
                return ApiResponse<ShippingRateDto>.ErrorResponse("运费配置不存在");
            }

            // 如果省份发生变化，检查新省份是否已存在
            if (existingRate.Province != updateDto.Province)
            {
                var duplicateRate = await _shippingRepository.GetByProvinceAsync(updateDto.Province);
                if (duplicateRate != null)
                {
                    return ApiResponse<ShippingRateDto>.ErrorResponse("该省份的运费配置已存在");
                }
            }

            // 更新运费配置信息
            existingRate.Province = updateDto.Province;
            existingRate.Rate = updateDto.Rate;

            var updatedRate = await _shippingRepository.UpdateAsync(existingRate);
            var rateDto = MapToDto(updatedRate);

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveByPatternAsync($"{CACHE_KEY_PREFIX}province:*");
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Updated shipping rate {ShippingRateId} for province {Province}", updatedRate.Id, updatedRate.Province);
            return ApiResponse<ShippingRateDto>.SuccessResponse(rateDto, "更新运费配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipping rate {ShippingRateId}", id);
            return ApiResponse<ShippingRateDto>.ErrorResponse("更新运费配置失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 删除运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> DeleteShippingRateAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting shipping rate {ShippingRateId}", id);

            var result = await _shippingRepository.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("Shipping rate {ShippingRateId} not found for deletion", id);
                return ApiResponse<bool>.ErrorResponse("运费配置不存在");
            }

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveByPatternAsync($"{CACHE_KEY_PREFIX}province:*");
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Deleted shipping rate {ShippingRateId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "删除运费配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shipping rate {ShippingRateId}", id);
            return ApiResponse<bool>.ErrorResponse("删除运费配置失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 计算运费
    /// </summary>
    /// <param name="province">省份</param>
    /// <param name="weight">重量</param>
    /// <returns>运费计算结果响应</returns>
    public async Task<ApiResponse<ShippingCalculationDto>> CalculateShippingAsync(string province, decimal weight)
    {
        try
        {
            _logger.LogInformation("Calculating shipping for province: {Province}, weight: {Weight}", province, weight);

            if (string.IsNullOrWhiteSpace(province))
            {
                return ApiResponse<ShippingCalculationDto>.ErrorResponse("省份不能为空");
            }

            if (weight <= 0)
            {
                return ApiResponse<ShippingCalculationDto>.ErrorResponse("重量必须大于0");
            }

            // 获取运费配置
            var rateResult = await GetShippingRateByProvinceAsync(province);
            if (!rateResult.Success || rateResult.Data == null)
            {
                return ApiResponse<ShippingCalculationDto>.ErrorResponse($"未找到省份 {province} 的运费配置");
            }

            var rate = rateResult.Data;
            var totalShipping = rate.Rate * weight;

            var calculation = new ShippingCalculationDto
            {
                Province = province,
                Weight = weight,
                Rate = rate.Rate,
                TotalShipping = totalShipping
            };

            _logger.LogInformation("Calculated shipping for {Province}: Rate={Rate}, Weight={Weight}, Total={Total}", 
                province, rate.Rate, weight, totalShipping);

            return ApiResponse<ShippingCalculationDto>.SuccessResponse(calculation, "运费计算成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping for province: {Province}, weight: {Weight}", province, weight);
            return ApiResponse<ShippingCalculationDto>.ErrorResponse("运费计算失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 映射实体到DTO
    /// </summary>
    /// <param name="rate">运费配置实体</param>
    /// <returns>运费配置DTO</returns>
    private static ShippingRateDto MapToDto(ShippingRate rate)
    {
        return new ShippingRateDto
        {
            Id = rate.Id,
            Province = rate.Province,
            Rate = rate.Rate,
            CreatedAt = rate.CreatedAt,
            UpdatedAt = rate.UpdatedAt
        };
    }

    /// <summary>
    /// 验证创建运费配置输入
    /// </summary>
    /// <param name="createDto">创建运费配置DTO</param>
    /// <returns>验证错误列表</returns>
    private static List<string> ValidateCreateShippingRate(CreateShippingRateDto createDto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(createDto.Province))
            errors.Add("省份不能为空");

        if (createDto.Rate < 0)
            errors.Add("运费费率不能为负数");

        return errors;
    }

    /// <summary>
    /// 验证更新运费配置输入
    /// </summary>
    /// <param name="updateDto">更新运费配置DTO</param>
    /// <returns>验证错误列表</returns>
    private static List<string> ValidateUpdateShippingRate(UpdateShippingRateDto updateDto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(updateDto.Province))
            errors.Add("省份不能为空");

        if (updateDto.Rate < 0)
            errors.Add("运费费率不能为负数");

        return errors;
    }
}
