using LLX.Server.Models.Entities;

namespace LLX.Server.Repositories;

/// <summary>
/// 运费仓储接口
/// </summary>
public interface IShippingRepository
{
    /// <summary>
    /// 获取所有运费配置
    /// </summary>
    /// <returns>运费配置列表</returns>
    Task<IEnumerable<ShippingRate>> GetAllAsync();

    /// <summary>
    /// 根据ID获取运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>运费配置信息</returns>
    Task<ShippingRate?> GetByIdAsync(int id);

    /// <summary>
    /// 根据省份获取运费配置
    /// </summary>
    /// <param name="province">省份</param>
    /// <returns>运费配置信息</returns>
    Task<ShippingRate?> GetByProvinceAsync(string province);

    /// <summary>
    /// 添加运费配置
    /// </summary>
    /// <param name="shippingRate">运费配置信息</param>
    /// <returns>添加后的运费配置</returns>
    Task<ShippingRate> AddAsync(ShippingRate shippingRate);

    /// <summary>
    /// 更新运费配置
    /// </summary>
    /// <param name="shippingRate">运费配置信息</param>
    /// <returns>更新后的运费配置</returns>
    Task<ShippingRate> UpdateAsync(ShippingRate shippingRate);

    /// <summary>
    /// 删除运费配置
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 检查运费配置是否存在
    /// </summary>
    /// <param name="id">运费配置ID</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 检查省份运费配置是否存在
    /// </summary>
    /// <param name="province">省份</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsByProvinceAsync(string province);
}
