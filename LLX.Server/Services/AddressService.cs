using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;
using LLX.Server.Repositories;
using LLX.Server.Utils;
using Microsoft.Extensions.Logging;

namespace LLX.Server.Services;

/// <summary>
/// 地址服务实现
/// </summary>
public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AddressService> _logger;

    private const string CACHE_KEY_PREFIX = "address:";
    private const string CACHE_KEY_ALL = "address:all";
    private const string CACHE_KEY_DEFAULT = "address:default";
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

    public AddressService(
        IAddressRepository addressRepository,
        ICacheService cacheService,
        ILogger<AddressService> logger)
    {
        _addressRepository = addressRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有地址
    /// </summary>
    /// <returns>地址列表响应</returns>
    public async Task<ApiResponse<IEnumerable<AddressDto>>> GetAllAddressesAsync()
    {
        try
        {
            _logger.LogInformation("Getting all addresses");

            // 尝试从缓存获取
            var cachedAddresses = await _cacheService.GetAsync<IEnumerable<AddressDto>>(CACHE_KEY_ALL);
            if (cachedAddresses != null)
            {
                _logger.LogInformation("Retrieved {Count} addresses from cache", cachedAddresses.Count());
                return ApiResponse<IEnumerable<AddressDto>>.SuccessResponse(cachedAddresses, "获取地址列表成功");
            }

            // 从数据库获取
            var addresses = await _addressRepository.GetAllAsync();
            var addressDtos = addresses.Select(MapToDto).ToList();

            // 缓存结果
            await _cacheService.SetAsync(CACHE_KEY_ALL, addressDtos, _cacheExpiry);

            _logger.LogInformation("Retrieved {Count} addresses from database", addressDtos.Count);
            return ApiResponse<IEnumerable<AddressDto>>.SuccessResponse(addressDtos, "获取地址列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all addresses");
            return ApiResponse<IEnumerable<AddressDto>>.ErrorResponse("获取地址列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>地址响应</returns>
    public async Task<ApiResponse<AddressDto?>> GetAddressByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting address by ID: {AddressId}", id);

            // 尝试从缓存获取
            var cacheKey = $"{CACHE_KEY_PREFIX}{id}";
            var cachedAddress = await _cacheService.GetAsync<AddressDto>(cacheKey);
            if (cachedAddress != null)
            {
                _logger.LogInformation("Retrieved address {AddressId} from cache", id);
                return ApiResponse<AddressDto?>.SuccessResponse(cachedAddress, "获取地址成功");
            }

            // 从数据库获取
            var address = await _addressRepository.GetByIdAsync(id);
            if (address == null)
            {
                _logger.LogWarning("Address {AddressId} not found", id);
                return ApiResponse<AddressDto?>.ErrorResponse("地址不存在");
            }

            var addressDto = MapToDto(address);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, addressDto, _cacheExpiry);

            _logger.LogInformation("Retrieved address {AddressId} from database", id);
            return ApiResponse<AddressDto?>.SuccessResponse(addressDto, "获取地址成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting address by ID: {AddressId}", id);
            return ApiResponse<AddressDto?>.ErrorResponse("获取地址失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 获取默认地址
    /// </summary>
    /// <returns>地址响应</returns>
    public async Task<ApiResponse<AddressDto?>> GetDefaultAddressAsync()
    {
        try
        {
            _logger.LogInformation("Getting default address");

            // 尝试从缓存获取
            var cachedAddress = await _cacheService.GetAsync<AddressDto>(CACHE_KEY_DEFAULT);
            if (cachedAddress != null)
            {
                _logger.LogInformation("Retrieved default address from cache");
                return ApiResponse<AddressDto?>.SuccessResponse(cachedAddress, "获取默认地址成功");
            }

            // 从数据库获取
            var address = await _addressRepository.GetDefaultAsync();
            if (address == null)
            {
                _logger.LogInformation("No default address found");
                return ApiResponse<AddressDto?>.SuccessResponse(null, "暂无默认地址");
            }

            var addressDto = MapToDto(address);

            // 缓存结果
            await _cacheService.SetAsync(CACHE_KEY_DEFAULT, addressDto, _cacheExpiry);

            _logger.LogInformation("Retrieved default address from database");
            return ApiResponse<AddressDto?>.SuccessResponse(addressDto, "获取默认地址成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default address");
            return ApiResponse<AddressDto?>.ErrorResponse("获取默认地址失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据手机号获取地址列表
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>地址列表响应</returns>
    public async Task<ApiResponse<IEnumerable<AddressDto>>> GetAddressesByPhoneAsync(string phone)
    {
        try
        {
            _logger.LogInformation("Getting addresses by phone: {Phone}", phone);

            if (string.IsNullOrWhiteSpace(phone))
            {
                return ApiResponse<IEnumerable<AddressDto>>.ErrorResponse("手机号不能为空");
            }

            var addresses = await _addressRepository.GetByPhoneAsync(phone);
            var addressDtos = addresses.Select(MapToDto).ToList();

            _logger.LogInformation("Found {Count} addresses for phone {Phone}", addressDtos.Count, phone);
            return ApiResponse<IEnumerable<AddressDto>>.SuccessResponse(addressDtos, "获取地址列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting addresses by phone: {Phone}", phone);
            return ApiResponse<IEnumerable<AddressDto>>.ErrorResponse("获取地址列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 创建地址
    /// </summary>
    /// <param name="createDto">创建地址DTO</param>
    /// <returns>地址响应</returns>
    public async Task<ApiResponse<AddressDto>> CreateAddressAsync(CreateAddressDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating address for: {Name}", createDto.Name);

            // 验证输入
            var validationErrors = ValidateCreateAddress(createDto);
            if (validationErrors.Any())
            {
                return ApiResponse<AddressDto>.ErrorResponse("输入验证失败", validationErrors);
            }

            var address = new Address
            {
                Name = createDto.Name,
                Phone = createDto.Phone,
                Province = createDto.Province,
                City = createDto.City,
                District = createDto.District,
                Detail = createDto.Detail,
                IsDefault = createDto.IsDefault
            };

            // 如果设置为默认地址，先取消其他默认地址
            if (createDto.IsDefault)
            {
                await _addressRepository.SetDefaultAsync(0); // 先清除所有默认
            }

            var createdAddress = await _addressRepository.AddAsync(address);
            var addressDto = MapToDto(createdAddress);

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync(CACHE_KEY_DEFAULT);

            _logger.LogInformation("Created address {AddressId}: {Name}", createdAddress.Id, createdAddress.Name);
            return ApiResponse<AddressDto>.SuccessResponse(addressDto, "创建地址成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address for: {Name}", createDto.Name);
            return ApiResponse<AddressDto>.ErrorResponse("创建地址失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 更新地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <param name="updateDto">更新地址DTO</param>
    /// <returns>地址响应</returns>
    public async Task<ApiResponse<AddressDto>> UpdateAddressAsync(int id, UpdateAddressDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating address {AddressId}", id);

            // 验证输入
            var validationErrors = ValidateUpdateAddress(updateDto);
            if (validationErrors.Any())
            {
                return ApiResponse<AddressDto>.ErrorResponse("输入验证失败", validationErrors);
            }

            // 检查地址是否存在
            var existingAddress = await _addressRepository.GetByIdAsync(id);
            if (existingAddress == null)
            {
                _logger.LogWarning("Address {AddressId} not found for update", id);
                return ApiResponse<AddressDto>.ErrorResponse("地址不存在");
            }

            // 更新地址信息
            existingAddress.Name = updateDto.Name;
            existingAddress.Phone = updateDto.Phone;
            existingAddress.Province = updateDto.Province;
            existingAddress.City = updateDto.City;
            existingAddress.District = updateDto.District;
            existingAddress.Detail = updateDto.Detail;
            existingAddress.IsDefault = updateDto.IsDefault;

            // 如果设置为默认地址，先取消其他默认地址
            if (updateDto.IsDefault && !existingAddress.IsDefault)
            {
                await _addressRepository.SetDefaultAsync(0); // 先清除所有默认
            }

            var updatedAddress = await _addressRepository.UpdateAsync(existingAddress);
            var addressDto = MapToDto(updatedAddress);

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync(CACHE_KEY_DEFAULT);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Updated address {AddressId}: {Name}", updatedAddress.Id, updatedAddress.Name);
            return ApiResponse<AddressDto>.SuccessResponse(addressDto, "更新地址成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", id);
            return ApiResponse<AddressDto>.ErrorResponse("更新地址失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 删除地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> DeleteAddressAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting address {AddressId}", id);

            var result = await _addressRepository.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("Address {AddressId} not found for deletion", id);
                return ApiResponse<bool>.ErrorResponse("地址不存在");
            }

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync(CACHE_KEY_DEFAULT);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Deleted address {AddressId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "删除地址成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", id);
            return ApiResponse<bool>.ErrorResponse("删除地址失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 设置默认地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> SetDefaultAddressAsync(int id)
    {
        try
        {
            _logger.LogInformation("Setting default address {AddressId}", id);

            var result = await _addressRepository.SetDefaultAsync(id);
            if (!result)
            {
                _logger.LogWarning("Address {AddressId} not found for setting default", id);
                return ApiResponse<bool>.ErrorResponse("地址不存在");
            }

            // 清除相关缓存
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync(CACHE_KEY_DEFAULT);

            _logger.LogInformation("Set default address {AddressId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "设置默认地址成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default address {AddressId}", id);
            return ApiResponse<bool>.ErrorResponse("设置默认地址失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 智能解析地址
    /// </summary>
    /// <param name="fullAddress">完整地址字符串</param>
    /// <returns>解析后的地址响应</returns>
    public Task<ApiResponse<ParsedAddressDto>> ParseAddressAsync(string fullAddress)
    {
        try
        {
            _logger.LogInformation("Parsing address: {FullAddress}", fullAddress);

            if (string.IsNullOrWhiteSpace(fullAddress))
            {
                return Task.FromResult(ApiResponse<ParsedAddressDto>.ErrorResponse("地址文本不能为空"));
            }

            var parsedAddress = AddressParser.Parse(fullAddress);

            _logger.LogInformation("Successfully parsed address: {Province} {City} {District}", 
                parsedAddress.Province, parsedAddress.City, parsedAddress.District);

            return Task.FromResult(ApiResponse<ParsedAddressDto>.SuccessResponse(parsedAddress, "地址解析成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing address: {FullAddress}", fullAddress);
            return Task.FromResult(ApiResponse<ParsedAddressDto>.ErrorResponse("地址解析失败", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// 映射实体到DTO
    /// </summary>
    /// <param name="address">地址实体</param>
    /// <returns>地址DTO</returns>
    private static AddressDto MapToDto(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            Name = address.Name,
            Phone = address.Phone,
            Province = address.Province,
            City = address.City,
            District = address.District,
            Detail = address.Detail,
            IsDefault = address.IsDefault,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }

    /// <summary>
    /// 验证创建地址输入
    /// </summary>
    /// <param name="createDto">创建地址DTO</param>
    /// <returns>验证错误列表</returns>
    private static List<string> ValidateCreateAddress(CreateAddressDto createDto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(createDto.Name))
            errors.Add("收货人姓名不能为空");

        if (string.IsNullOrWhiteSpace(createDto.Phone))
            errors.Add("手机号不能为空");

        if (!IsValidPhone(createDto.Phone))
            errors.Add("手机号格式不正确");

        if (string.IsNullOrWhiteSpace(createDto.Province))
            errors.Add("省份不能为空");

        if (string.IsNullOrWhiteSpace(createDto.City))
            errors.Add("城市不能为空");

        if (string.IsNullOrWhiteSpace(createDto.Detail))
            errors.Add("详细地址不能为空");

        return errors;
    }

    /// <summary>
    /// 验证更新地址输入
    /// </summary>
    /// <param name="updateDto">更新地址DTO</param>
    /// <returns>验证错误列表</returns>
    private static List<string> ValidateUpdateAddress(UpdateAddressDto updateDto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(updateDto.Name))
            errors.Add("收货人姓名不能为空");

        if (string.IsNullOrWhiteSpace(updateDto.Phone))
            errors.Add("手机号不能为空");

        if (!IsValidPhone(updateDto.Phone))
            errors.Add("手机号格式不正确");

        if (string.IsNullOrWhiteSpace(updateDto.Province))
            errors.Add("省份不能为空");

        if (string.IsNullOrWhiteSpace(updateDto.City))
            errors.Add("城市不能为空");

        if (string.IsNullOrWhiteSpace(updateDto.Detail))
            errors.Add("详细地址不能为空");

        return errors;
    }

    /// <summary>
    /// 验证手机号格式
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>是否有效</returns>
    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // 简单的手机号验证：11位数字，以1开头
        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1[3-9]\d{9}$");
    }
}
