using Microsoft.EntityFrameworkCore;
using LLX.Server.Models.Entities;

namespace LLX.Server.Data;

/// <summary>
/// 应用程序数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    private static DatabaseProvider _currentProvider = DatabaseProvider.PostgreSQL;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 设置当前数据库提供程序
    /// </summary>
    /// <param name="provider">数据库提供程序</param>
    public static void SetCurrentProvider(DatabaseProvider provider)
    {
        _currentProvider = provider;
    }

    /// <summary>
    /// 商品表
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// 地址表
    /// </summary>
    public DbSet<Address> Addresses { get; set; }

    /// <summary>
    /// 订单表
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// 订单明细表
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// 运费配置表
    /// </summary>
    public DbSet<ShippingRate> ShippingRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置商品表
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Weight).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(GetCurrentTimestampSql());
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql(GetCurrentTimestampSql());

            entity.HasIndex(e => e.Name).HasDatabaseName("idx_products_name");
        });

        // 配置地址表
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Province).IsRequired().HasMaxLength(50);
            entity.Property(e => e.City).IsRequired().HasMaxLength(50);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.Detail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(GetCurrentTimestampSql());
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql(GetCurrentTimestampSql());

            entity.HasIndex(e => e.IsDefault).HasDatabaseName("idx_addresses_default");
            entity.HasIndex(e => e.Phone).HasDatabaseName("idx_addresses_phone");
        });

        // 配置订单表
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalRicePrice).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.TotalWeight).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.ShippingRate).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.TotalShipping).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.GrandTotal).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.PaymentStatus).HasMaxLength(20).HasDefaultValue("未付款");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("待发货");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(GetCurrentTimestampSql());
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql(GetCurrentTimestampSql());

            entity.HasIndex(e => e.OrderNo).IsUnique().HasDatabaseName("idx_orders_orderno");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_orders_status");
            entity.HasIndex(e => e.PaymentStatus).HasDatabaseName("idx_orders_payment_status");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("idx_orders_createdat");
            entity.HasIndex(e => e.AddressId).HasDatabaseName("idx_orders_address");

            entity.HasOne(e => e.Address)
                  .WithMany()
                  .HasForeignKey(e => e.AddressId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 配置订单明细表
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProductPrice).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.ProductUnit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ProductWeight).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.Subtotal).HasColumnType(GetDecimalType(10, 2));

            entity.HasIndex(e => e.OrderId).HasDatabaseName("idx_orderitems_orderid");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("idx_orderitems_productid");

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 配置运费配置表
        modelBuilder.Entity<ShippingRate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Province).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Rate).HasColumnType(GetDecimalType(10, 2));
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(GetCurrentTimestampSql());
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql(GetCurrentTimestampSql());

            entity.HasIndex(e => e.Province).IsUnique().HasDatabaseName("idx_shippingrates_province");
        });
    }

    /// <summary>
    /// 获取数据库特定的 decimal 类型
    /// </summary>
    /// <param name="precision">精度</param>
    /// <param name="scale">小数位数</param>
    /// <returns>数据库特定的 decimal 类型字符串</returns>
    private string GetDecimalType(int precision, int scale)
    {
        return _currentProvider switch
        {
            DatabaseProvider.PostgreSQL => $"decimal({precision},{scale})",
            DatabaseProvider.SqlServer => $"decimal({precision},{scale})",
            DatabaseProvider.MySql => $"decimal({precision},{scale})",
            DatabaseProvider.Sqlite => "decimal",
            _ => $"decimal({precision},{scale})"
        };
    }

    /// <summary>
    /// 获取数据库特定的当前时间戳 SQL
    /// </summary>
    /// <returns>当前时间戳 SQL 语句</returns>
    private string GetCurrentTimestampSql()
    {
        return _currentProvider switch
        {
            DatabaseProvider.PostgreSQL => "CURRENT_TIMESTAMP",
            DatabaseProvider.SqlServer => "GETDATE()",
            DatabaseProvider.MySql => "CURRENT_TIMESTAMP",
            DatabaseProvider.Sqlite => "CURRENT_TIMESTAMP",
            _ => "CURRENT_TIMESTAMP"
        };
    }
}
