using LLX.Server.Models.DTOs;
using LLX.Server.Models.Entities;
using LLX.Server.Repositories;

namespace LLX.Server.Services;

/// <summary>
/// 商品服务实现
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductService> _logger;

    private const string CACHE_KEY_PREFIX = "product:";
    private const string CACHE_KEY_ALL = "product:all";
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

            // 尝试从缓存获取
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(CACHE_KEY_ALL);
            if (cachedProducts != null)
            {
                _logger.LogInformation("Retrieved {Count} products from cache", cachedProducts.Count());
                return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(cachedProducts, "获取商品列表成功");
            }

            // 从数据库获取
            var products = await _productRepository.GetAllAsync();
            var productDtos = products.Select(MapToDto).ToList();

            // 缓存结果
            await _cacheService.SetAsync(CACHE_KEY_ALL, productDtos, _cacheExpiry);

            _logger.LogInformation("Retrieved {Count} products from database", productDtos.Count);
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

            // 尝试从缓存获取
            var cacheKey = $"{CACHE_KEY_PREFIX}{id}";
            var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
            if (cachedProduct != null)
            {
                _logger.LogInformation("Retrieved product {ProductId} from cache", id);
                return ApiResponse<ProductDto?>.SuccessResponse(cachedProduct, "获取商品成功");
            }

            // 从数据库获取
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return ApiResponse<ProductDto?>.ErrorResponse("商品不存在");
            }

            var productDto = MapToDto(product);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, productDto, _cacheExpiry);

            _logger.LogInformation("Retrieved product {ProductId} from database", id);
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

            var products = await _productRepository.SearchByNameAsync(name);
            var productDtos = products.Select(MapToDto).ToList();

            _logger.LogInformation("Found {Count} products matching '{ProductName}'", productDtos.Count, name);
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
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);

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
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

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
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

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
            await _cacheService.RemoveAsync(CACHE_KEY_ALL);
            await _cacheService.RemoveAsync($"{CACHE_KEY_PREFIX}{id}");

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
