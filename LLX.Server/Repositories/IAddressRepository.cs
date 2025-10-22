using LLX.Server.Models.Entities;

namespace LLX.Server.Repositories;

/// <summary>
/// 地址仓储接口
/// </summary>
public interface IAddressRepository
{
    /// <summary>
    /// 获取所有地址
    /// </summary>
    /// <returns>地址列表</returns>
    Task<IEnumerable<Address>> GetAllAsync();

    /// <summary>
    /// 根据ID获取地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>地址信息</returns>
    Task<Address?> GetByIdAsync(int id);

    /// <summary>
    /// 获取默认地址
    /// </summary>
    /// <returns>默认地址</returns>
    Task<Address?> GetDefaultAsync();

    /// <summary>
    /// 根据手机号获取地址列表
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>地址列表</returns>
    Task<IEnumerable<Address>> GetByPhoneAsync(string phone);

    /// <summary>
    /// 添加地址
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>添加后的地址</returns>
    Task<Address> AddAsync(Address address);

    /// <summary>
    /// 更新地址
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>更新后的地址</returns>
    Task<Address> UpdateAsync(Address address);

    /// <summary>
    /// 删除地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 设置默认地址
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetDefaultAsync(int id);

    /// <summary>
    /// 检查地址是否存在
    /// </summary>
    /// <param name="id">地址ID</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(int id);
}
