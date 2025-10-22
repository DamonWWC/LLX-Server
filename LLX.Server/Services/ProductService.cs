using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;
using LLX.Server.Repositories;
using LLX.Server.Utils;

namespace LLX.Server.Services;

/// <summary>
/// 商品服务实现
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductService> _logger;

    // 使用新的缓存策略
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

    public ProductService(
        IProductRepository productRepository,
        ICacheService cacheService,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有商品
    /// </summary>
    /// <returns>商品列表响应</returns>
    public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all products");

            // 使用GetOrSetAsync防缓存穿透和击穿
            var productDtos = await _cacheService.GetOrSetAsync(
                CacheStrategy.Keys.ProductAll,
                async () =>
                {
                    var products = await _productRepository.GetAllAsync();
                    return products.Select(MapToDto).ToList();
                },
                CacheStrategy.GetRandomExpiry(CacheStrategy.Expiry.ProductAll)
            );

            if (productDtos == null)
            {
                _logger.LogWarning("Failed to retrieve products");
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("获取商品列表失败");
            }

            _logger.LogInformation("Retrieved {Count} products", productDtos.Count());
            return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(productDtos, "获取商品列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("获取商品列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品响应</returns>
    public async Task<ApiResponse<ProductDto?>> GetProductByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);

            // 使用GetOrSetAsync防缓存穿透和击穿
            var cacheKey = CacheStrategy.GenerateKey(CacheStrategy.Keys.Product, id.ToString());
            var productDto = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var product = await _productRepository.GetByIdAsync(id);
                    return product != null ? MapToDto(product) : null;
                },
                CacheStrategy.GetRandomExpiry(CacheStrategy.Expiry.ProductSingle)
            );

            if (productDto == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return ApiResponse<ProductDto?>.ErrorResponse("商品不存在");
            }

            _logger.LogInformation("Retrieved product {ProductId}", id);
            return ApiResponse<ProductDto?>.SuccessResponse(productDto, "获取商品成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
            return ApiResponse<ProductDto?>.ErrorResponse("获取商品失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 根据名称搜索商品
    /// </summary>
    /// <param name="name">商品名称</param>
    /// <returns>商品列表响应</returns>
    public async Task<ApiResponse<IEnumerable<ProductDto>>> SearchProductsAsync(string name)
    {
        try
        {
            _logger.LogInformation("Searching products by name: {ProductName}", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("搜索关键词不能为空");
            }

            // 使用GetOrSetAsync防缓存穿透和击穿
            var cacheKey = CacheStrategy.GenerateSearchKey(name);
            var productDtos = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var products = await _productRepository.SearchByNameAsync(name);
                    return products.Select(MapToDto).ToList();
                },
                CacheStrategy.GetRandomExpiry(CacheStrategy.Expiry.ProductSearch)
            );

            if (productDtos == null)
            {
                productDtos = new List<ProductDto>();
            }

            _logger.LogInformation("Found {Count} products matching '{ProductName}'", productDtos.Count(), name);
            return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(productDtos, "搜索商品成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products by name: {ProductName}", name);
            return ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("搜索商品失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 创建商品
    /// </summary>
    /// <param name="createDto">创建商品DTO</param>
    /// <returns>商品响应</returns>
    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating product: {ProductName}", createDto.Name);

            // 验证输入
            var validationErrors = ValidateCreateProduct(createDto);
            if (validationErrors.Any())
            {
                return ApiResponse<ProductDto>.ErrorResponse("输入验证失败", validationErrors);
            }

            var product = new Product
            {
                Name = createDto.Name,
                Price = createDto.Price,
                Unit = createDto.Unit,
                Weight = createDto.Weight,
                Image = createDto.Image,
                Quantity = createDto.Quantity
            };

            var createdProduct = await _productRepository.AddAsync(product);
            var productDto = MapToDto(createdProduct);

            // 清除相关缓存
            await InvalidateProductCacheAsync();

            _logger.LogInformation("Created product {ProductId}: {ProductName}", createdProduct.Id, createdProduct.Name);
            return ApiResponse<ProductDto>.SuccessResponse(productDto, "创建商品成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", createDto.Name);
            return ApiResponse<ProductDto>.ErrorResponse("创建商品失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="updateDto">更新商品DTO</param>
    /// <returns>商品响应</returns>
    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating product {ProductId}", id);

            // 验证输入
            var validationErrors = ValidateUpdateProduct(updateDto);
            if (validationErrors.Any())
            {
                return ApiResponse<ProductDto>.ErrorResponse("输入验证失败", validationErrors);
            }

            // 检查商品是否存在
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product {ProductId} not found for update", id);
                return ApiResponse<ProductDto>.ErrorResponse("商品不存在");
            }

            // 更新商品信息
            existingProduct.Name = updateDto.Name;
            existingProduct.Price = updateDto.Price;
            existingProduct.Unit = updateDto.Unit;
            existingProduct.Weight = updateDto.Weight;
            existingProduct.Image = updateDto.Image;
            existingProduct.Quantity = updateDto.Quantity;

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            var productDto = MapToDto(updatedProduct);

            // 清除相关缓存
            await InvalidateProductCacheAsync(id);

            _logger.LogInformation("Updated product {ProductId}: {ProductName}", updatedProduct.Id, updatedProduct.Name);
            return ApiResponse<ProductDto>.SuccessResponse(productDto, "更新商品成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return ApiResponse<ProductDto>.ErrorResponse("更新商品失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 删除商品
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product {ProductId}", id);

            var result = await _productRepository.DeleteAsync(id);
            if (!result)
            {
                _logger.LogWarning("Product {ProductId} not found for deletion", id);
                return ApiResponse<bool>.ErrorResponse("商品不存在");
            }

            // 清除相关缓存
            await InvalidateProductCacheAsync(id);

            _logger.LogInformation("Deleted product {ProductId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "删除商品成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return ApiResponse<bool>.ErrorResponse("删除商品失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 更新商品库存
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="quantity">库存数量</param>
    /// <returns>操作响应</returns>
    public async Task<ApiResponse<bool>> UpdateProductQuantityAsync(int id, int quantity)
    {
        try
        {
            _logger.LogInformation("Updating product {ProductId} quantity to {Quantity}", id, quantity);

            if (quantity < 0)
            {
                return ApiResponse<bool>.ErrorResponse("库存数量不能为负数");
            }

            var result = await _productRepository.UpdateQuantityAsync(id, quantity);
            if (!result)
            {
                _logger.LogWarning("Product {ProductId} not found for quantity update", id);
                return ApiResponse<bool>.ErrorResponse("商品不存在");
            }

            // 清除相关缓存
            await InvalidateProductCacheAsync(id);

            _logger.LogInformation("Updated product {ProductId} quantity to {Quantity}", id, quantity);
            return ApiResponse<bool>.SuccessResponse(true, "更新库存成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId} quantity", id);
            return ApiResponse<bool>.ErrorResponse("更新库存失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 分页获取商品列表
    /// </summary>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDescending">是否降序</param>
    /// <param name="searchTerm">搜索词</param>
    /// <returns>分页商品列表响应</returns>
    public async Task<ApiResponse<QueryOptimizer.PagedResult<ProductDto>>> GetProductsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false,
        string? searchTerm = null)
    {
        try
        {
            _logger.LogInformation("Getting paged products: Page={PageNumber}, Size={PageSize}, Sort={SortBy}, Search={SearchTerm}", 
                pageNumber, pageSize, sortBy, searchTerm);

            // 使用GetOrSetAsync防缓存穿透和击穿
            var cacheKey = $"product:paged:{pageNumber}:{pageSize}:{sortBy}:{sortDescending}:{searchTerm}";
            var pagedResult = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var query = _productRepository.GetQueryable();
                    
                    // 应用搜索过滤
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        query = query.ApplySearch(searchTerm, p => p.Name, p => p.Unit);
                    }
                    
                    // 优化查询（只读）
                    query = query.OptimizeForRead(true);
                    
                    // 执行分页查询
                    return await query.ToPagedResultAsync(pageNumber, pageSize, sortBy, sortDescending);
                },
                CacheStrategy.GetRandomExpiry(TimeSpan.FromMinutes(10))
            );

            if (pagedResult == null)
            {
                _logger.LogWarning("Failed to retrieve paged products");
                return ApiResponse<QueryOptimizer.PagedResult<ProductDto>>.ErrorResponse("获取商品列表失败");
            }

            // 转换为DTO
            var productDtos = pagedResult.Items.Select(MapToDto).ToList();
            var result = new QueryOptimizer.PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };

            _logger.LogInformation("Retrieved {Count} products (page {PageNumber} of {TotalPages})", 
                productDtos.Count, result.PageNumber, result.TotalPages);
            
            return ApiResponse<QueryOptimizer.PagedResult<ProductDto>>.SuccessResponse(result, "获取商品列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged products");
            return ApiResponse<QueryOptimizer.PagedResult<ProductDto>>.ErrorResponse("获取商品列表失败", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// 清除商品相关缓存
    /// </summary>
    /// <param name="productId">商品ID（可选）</param>
    /// <returns>任务</returns>
    private async Task InvalidateProductCacheAsync(int? productId = null)
    {
        try
        {
            // 清除所有商品列表缓存
            await _cacheService.RemoveAsync(CacheStrategy.Keys.ProductAll);
            
            // 清除搜索缓存
            await _cacheService.RemoveByPatternAsync(CacheStrategy.Keys.ProductSearch + "*");
            
            // 如果指定了商品ID，清除单个商品缓存
            if (productId.HasValue)
            {
                var productKey = CacheStrategy.GenerateKey(CacheStrategy.Keys.Product, productId.Value.ToString());
                await _cacheService.RemoveAsync(productKey);
            }
            else
            {
                // 清除所有商品缓存
                await _cacheService.RemoveByPatternAsync(CacheStrategy.Patterns.ProductAll);
            }
            
            _logger.LogInformation("Invalidated product cache for product {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate product cache for product {ProductId}", productId);
        }
    }

    /// <summary>
    /// 映射实体到DTO
    /// </summary>
    /// <param name="product">商品实体</param>
    /// <returns>商品DTO</returns>
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Unit = product.Unit,
            Weight = product.Weight,
            Image = product.Image,
            Quantity = product.Quantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    /// <summary>
    /// 验证创建商品输入
    /// </summary>
    /// <param name="createDto">创建商品DTO</param>
    /// <returns>验证错误列表</returns>
    private static List<string> ValidateCreateProduct(CreateProductDto createDto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(createDto.Name))
            errors.Add("商品名称不能为空");

        if (createDto.Price <= 0)
            errors.Add("商品价格必须大于0");

        if (string.IsNullOrWhiteSpace(createDto.Unit))
            errors.Add("商品单位不能为空");

        if (createDto.Weight <= 0)
            errors.Add("商品重量必须大于0");

        if (createDto.Quantity < 0)
            errors.Add("库存数量不能为负数");

        return errors;
    }

    /// <summary>
    /// 验证更新商品输入
    /// </summary>
    /// <param name="updateDto">更新商品DTO</param>
    /// <returns>验证错误列表</returns>
    private static List<string> ValidateUpdateProduct(UpdateProductDto updateDto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(updateDto.Name))
            errors.Add("商品名称不能为空");

        if (updateDto.Price <= 0)
            errors.Add("商品价格必须大于0");

        if (string.IsNullOrWhiteSpace(updateDto.Unit))
            errors.Add("商品单位不能为空");

        if (updateDto.Weight <= 0)
            errors.Add("商品重量必须大于0");

        if (updateDto.Quantity < 0)
            errors.Add("库存数量不能为负数");

        return errors;
    }
}
