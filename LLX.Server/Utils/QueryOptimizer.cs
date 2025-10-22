using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LLX.Server.Utils;

/// <summary>
/// 数据库查询优化工具类
/// </summary>
public static class QueryOptimizer
{
    /// <summary>
    /// 分页查询结果
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// 分页查询参数
    /// </summary>
    public class PagedQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// 执行分页查询
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询</param>
    /// <param name="pageNumber">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDescending">是否降序</param>
    /// <returns>分页结果</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false)
    {
        // 参数验证
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize)); // 限制最大页大小为100

        // 获取总数
        var totalCount = await query.CountAsync();

        // 应用排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = ApplySorting(query, sortBy, sortDescending);
        }

        // 应用分页
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 执行分页查询（使用参数对象）
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询</param>
    /// <param name="params">分页参数</param>
    /// <returns>分页结果</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedQueryParams @params)
    {
        return await query.ToPagedResultAsync(
            @params.PageNumber,
            @params.PageSize,
            @params.SortBy,
            @params.SortDescending);
    }

    /// <summary>
    /// 应用排序
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询</param>
    /// <param name="sortBy">排序字段</param>
    /// <param name="sortDescending">是否降序</param>
    /// <returns>排序后的查询</returns>
    private static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, bool sortDescending)
    {
        try
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, sortBy);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
        catch
        {
            // 如果排序失败，返回原查询
            return query;
        }
    }

    /// <summary>
    /// 应用搜索过滤
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询</param>
    /// <param name="searchTerm">搜索词</param>
    /// <param name="searchProperties">搜索属性</param>
    /// <returns>过滤后的查询</returns>
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? searchTerm,
        params Expression<Func<T, string>>[] searchProperties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || !searchProperties.Any())
        {
            return query;
        }

        var searchTermLower = searchTerm.ToLowerInvariant();
        Expression? combinedExpression = null;

        foreach (var property in searchProperties)
        {
            // 创建属性访问表达式
            var propertyAccess = property.Body;
            
            // 创建 ToLower() 调用
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var toLowerCall = Expression.Call(propertyAccess, toLowerMethod!);
            
            // 创建 Contains 调用
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsCall = Expression.Call(toLowerCall, containsMethod!, Expression.Constant(searchTermLower));
            
            // 组合表达式
            if (combinedExpression == null)
            {
                combinedExpression = containsCall;
            }
            else
            {
                combinedExpression = Expression.OrElse(combinedExpression, containsCall);
            }
        }

        if (combinedExpression != null)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// 优化查询（添加AsNoTracking）
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询</param>
    /// <param name="isReadOnly">是否只读</param>
    /// <returns>优化后的查询</returns>
    public static IQueryable<T> OptimizeForRead<T>(this IQueryable<T> query, bool isReadOnly = true) where T : class
    {
        if (isReadOnly)
        {
            return query.AsNoTracking();
        }
        return query;
    }

    /// <summary>
    /// 批量更新实体
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="context">数据库上下文</param>
    /// <param name="entities">实体列表</param>
    /// <param name="updateAction">更新操作</param>
    /// <returns>任务</returns>
    public static async Task BatchUpdateAsync<T>(
        this DbContext context,
        IEnumerable<T> entities,
        Action<T> updateAction) where T : class
    {
        foreach (var entity in entities)
        {
            updateAction(entity);
            context.Entry(entity).State = EntityState.Modified;
        }
        
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 批量删除实体
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="context">数据库上下文</param>
    /// <param name="entities">实体列表</param>
    /// <returns>任务</returns>
    public static async Task BatchDeleteAsync<T>(
        this DbContext context,
        IEnumerable<T> entities) where T : class
    {
        context.RemoveRange(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 检查查询性能
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">查询</param>
    /// <param name="operationName">操作名称</param>
    /// <returns>查询结果和性能信息</returns>
    public static async Task<(List<T> Result, TimeSpan Elapsed)> MeasureQueryPerformanceAsync<T>(
        this IQueryable<T> query,
        string operationName = "Query")
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await query.ToListAsync();
        stopwatch.Stop();
        
        // 这里可以添加日志记录
        Console.WriteLine($"{operationName} executed in {stopwatch.ElapsedMilliseconds}ms, returned {result.Count} items");
        
        return (result, stopwatch.Elapsed);
    }
}
