-- =============================================
-- 林龍香大米商城 - PostgreSQL 数据库初始化脚本
-- 基于当前小程序数据生成
-- 创建时间: 2025-10-17
-- 数据库版本: PostgreSQL 16
-- =============================================

-- =============================================
-- 1. 数据库和用户创建（可选，根据需要执行）
-- =============================================

-- 创建数据库（如果不存在）
-- CREATE DATABASE llxrice WITH ENCODING 'UTF8' LC_COLLATE='C' LC_CTYPE='C' TEMPLATE=template0;

-- 创建应用用户（如果不存在）
-- CREATE USER llxrice_user WITH PASSWORD 'your_strong_password';
-- GRANT ALL PRIVILEGES ON DATABASE llxrice TO llxrice_user;

-- 连接到数据库
-- \c llxrice;

-- 授权 schema
-- GRANT ALL ON SCHEMA public TO llxrice_user;
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO llxrice_user;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO llxrice_user;

-- =============================================
-- 2. 删除已存在的表（谨慎使用）
-- =============================================

DROP TABLE IF EXISTS "OrderItems" CASCADE;
DROP TABLE IF EXISTS "Orders" CASCADE;
DROP TABLE IF EXISTS "Addresses" CASCADE;
DROP TABLE IF EXISTS "Products" CASCADE;
DROP TABLE IF EXISTS "ShippingRates" CASCADE;

-- 删除触发器函数（如果存在）
DROP FUNCTION IF EXISTS update_updated_at_column() CASCADE;

-- =============================================
-- 3. 创建表结构
-- =============================================

-- 3.1 商品表
CREATE TABLE "Products" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Price" DECIMAL(10,2) NOT NULL CHECK ("Price" >= 0),
    "Unit" VARCHAR(20) NOT NULL,
    "Weight" DECIMAL(10,2) NOT NULL CHECK ("Weight" > 0),
    "Image" TEXT,
    "Quantity" INT DEFAULT 0 CHECK ("Quantity" >= 0),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE "Products" IS '商品表';
COMMENT ON COLUMN "Products"."Name" IS '商品名称';
COMMENT ON COLUMN "Products"."Price" IS '单价（元）';
COMMENT ON COLUMN "Products"."Unit" IS '单位（袋/箱）';
COMMENT ON COLUMN "Products"."Weight" IS '单位重量（斤）';
COMMENT ON COLUMN "Products"."Image" IS '商品图片（Base64或URL）';
COMMENT ON COLUMN "Products"."Quantity" IS '库存数量';

-- 3.2 收货地址表
CREATE TABLE "Addresses" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL,
    "Phone" VARCHAR(20) NOT NULL,
    "Province" VARCHAR(50) NOT NULL,
    "City" VARCHAR(50) NOT NULL,
    "District" VARCHAR(50),
    "Detail" VARCHAR(200) NOT NULL,
    "IsDefault" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE "Addresses" IS '收货地址表';
COMMENT ON COLUMN "Addresses"."Name" IS '收货人姓名';
COMMENT ON COLUMN "Addresses"."Phone" IS '联系电话';
COMMENT ON COLUMN "Addresses"."Province" IS '省份';
COMMENT ON COLUMN "Addresses"."City" IS '城市';
COMMENT ON COLUMN "Addresses"."District" IS '区县';
COMMENT ON COLUMN "Addresses"."Detail" IS '详细地址';
COMMENT ON COLUMN "Addresses"."IsDefault" IS '是否默认地址';

-- 3.3 订单表
CREATE TABLE "Orders" (
    "Id" SERIAL PRIMARY KEY,
    "OrderNo" VARCHAR(50) UNIQUE NOT NULL,
    "AddressId" INT NOT NULL,
    "TotalRicePrice" DECIMAL(10,2) NOT NULL CHECK ("TotalRicePrice" >= 0),
    "TotalWeight" DECIMAL(10,2) NOT NULL CHECK ("TotalWeight" > 0),
    "ShippingRate" DECIMAL(10,2) NOT NULL CHECK ("ShippingRate" >= 0),
    "TotalShipping" DECIMAL(10,2) NOT NULL CHECK ("TotalShipping" >= 0),
    "GrandTotal" DECIMAL(10,2) NOT NULL CHECK ("GrandTotal" >= 0),
    "PaymentStatus" VARCHAR(20) DEFAULT '未付款',
    "Status" VARCHAR(20) NOT NULL DEFAULT '待发货',
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT "fk_orders_address" FOREIGN KEY ("AddressId") 
        REFERENCES "Addresses"("Id") ON DELETE RESTRICT
);

COMMENT ON TABLE "Orders" IS '订单表';
COMMENT ON COLUMN "Orders"."OrderNo" IS '订单号';
COMMENT ON COLUMN "Orders"."TotalRicePrice" IS '商品总价';
COMMENT ON COLUMN "Orders"."TotalWeight" IS '总重量（斤）';
COMMENT ON COLUMN "Orders"."ShippingRate" IS '运费单价（元/斤）';
COMMENT ON COLUMN "Orders"."TotalShipping" IS '运费总计';
COMMENT ON COLUMN "Orders"."GrandTotal" IS '订单总金额';
COMMENT ON COLUMN "Orders"."PaymentStatus" IS '付款状态：已付款/未付款';
COMMENT ON COLUMN "Orders"."Status" IS '订单状态：待付款/待发货/已发货/已完成/已取消';

-- 3.4 订单明细表
CREATE TABLE "OrderItems" (
    "Id" SERIAL PRIMARY KEY,
    "OrderId" INT NOT NULL,
    "ProductId" INT NOT NULL,
    "ProductName" VARCHAR(100) NOT NULL,
    "ProductPrice" DECIMAL(10,2) NOT NULL CHECK ("ProductPrice" >= 0),
    "ProductUnit" VARCHAR(20) NOT NULL,
    "ProductWeight" DECIMAL(10,2) NOT NULL CHECK ("ProductWeight" > 0),
    "Quantity" INT NOT NULL CHECK ("Quantity" > 0),
    "Subtotal" DECIMAL(10,2) NOT NULL CHECK ("Subtotal" >= 0),
    
    CONSTRAINT "fk_orderitems_order" FOREIGN KEY ("OrderId") 
        REFERENCES "Orders"("Id") ON DELETE CASCADE,
    CONSTRAINT "fk_orderitems_product" FOREIGN KEY ("ProductId") 
        REFERENCES "Products"("Id") ON DELETE RESTRICT
);

COMMENT ON TABLE "OrderItems" IS '订单明细表';
COMMENT ON COLUMN "OrderItems"."ProductName" IS '商品名称（冗余存储）';
COMMENT ON COLUMN "OrderItems"."ProductPrice" IS '商品单价（冗余存储）';
COMMENT ON COLUMN "OrderItems"."ProductUnit" IS '商品单位（冗余存储）';
COMMENT ON COLUMN "OrderItems"."ProductWeight" IS '商品重量（冗余存储）';
COMMENT ON COLUMN "OrderItems"."Quantity" IS '购买数量';
COMMENT ON COLUMN "OrderItems"."Subtotal" IS '小计';

-- 3.5 运费配置表
CREATE TABLE "ShippingRates" (
    "Id" SERIAL PRIMARY KEY,
    "Province" VARCHAR(50) NOT NULL UNIQUE,
    "Rate" DECIMAL(10,2) NOT NULL CHECK ("Rate" >= 0),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE "ShippingRates" IS '运费配置表';
COMMENT ON COLUMN "ShippingRates"."Province" IS '省份名称';
COMMENT ON COLUMN "ShippingRates"."Rate" IS '运费单价（元/斤）';

-- =============================================
-- 4. 创建索引
-- =============================================

-- 商品表索引
CREATE INDEX "idx_products_name" ON "Products"("Name");

-- 地址表索引
CREATE INDEX "idx_addresses_default" ON "Addresses"("IsDefault");
CREATE INDEX "idx_addresses_phone" ON "Addresses"("Phone");

-- 订单表索引
CREATE INDEX "idx_orders_orderno" ON "Orders"("OrderNo");
CREATE INDEX "idx_orders_status" ON "Orders"("Status");
CREATE INDEX "idx_orders_payment_status" ON "Orders"("PaymentStatus");
CREATE INDEX "idx_orders_createdat" ON "Orders"("CreatedAt" DESC);
CREATE INDEX "idx_orders_address" ON "Orders"("AddressId");

-- 订单明细表索引
CREATE INDEX "idx_orderitems_orderid" ON "OrderItems"("OrderId");
CREATE INDEX "idx_orderitems_productid" ON "OrderItems"("ProductId");

-- 运费配置表索引（已通过 UNIQUE 约束创建）

-- =============================================
-- 5. 创建触发器（自动更新UpdatedAt字段）
-- =============================================

CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- 为所有表创建触发器
CREATE TRIGGER update_products_updated_at 
    BEFORE UPDATE ON "Products" 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_addresses_updated_at 
    BEFORE UPDATE ON "Addresses" 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_orders_updated_at 
    BEFORE UPDATE ON "Orders" 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_shippingrates_updated_at 
    BEFORE UPDATE ON "ShippingRates" 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- 6. 插入初始数据
-- =============================================

-- 6.1 插入默认商品数据（基于小程序getDefaultProducts()）
INSERT INTO "Products" ("Name", "Price", "Unit", "Weight", "Image", "Quantity") VALUES
('稻花香', 40.00, '袋', 10.00, 'data:image/svg+xml,%3Csvg width="300" height="300" xmlns="http://www.w3.org/2000/svg"%3E%3Crect width="300" height="300" fill="%23FCE4EC"/%3E%3Ctext x="50%25" y="50%25" font-size="80" fill="%23E91E63" text-anchor="middle" dy=".3em"%3E🌾%3C/text%3E%3C/svg%3E', 0),
('稻花香', 50.00, '箱', 10.00, 'data:image/svg+xml,%3Csvg width="300" height="300" xmlns="http://www.w3.org/2000/svg"%3E%3Crect width="300" height="300" fill="%23FCE4EC"/%3E%3Ctext x="50%25" y="50%25" font-size="80" fill="%23E91E63" text-anchor="middle" dy=".3em"%3E📦%3C/text%3E%3C/svg%3E', 0),
('长粒香', 30.00, '袋', 10.00, 'data:image/svg+xml,%3Csvg width="300" height="300" xmlns="http://www.w3.org/2000/svg"%3E%3Crect width="300" height="300" fill="%23E8F5E9"/%3E%3Ctext x="50%25" y="50%25" font-size="80" fill="%234CAF50" text-anchor="middle" dy=".3em"%3E🌾%3C/text%3E%3C/svg%3E', 0),
('长粒香', 40.00, '箱', 10.00, 'data:image/svg+xml,%3Csvg width="300" height="300" xmlns="http://www.w3.org/2000/svg"%3E%3Crect width="300" height="300" fill="%23E8F5E9"/%3E%3Ctext x="50%25" y="50%25" font-size="80" fill="%234CAF50" text-anchor="middle" dy=".3em"%3E📦%3C/text%3E%3C/svg%3E', 0);

-- 6.2 插入运费配置数据（基于小程序shippingConfig.js）
INSERT INTO "ShippingRates" ("Province", "Rate") VALUES
-- 东北地区 - 1.0元/斤
('黑龙江省', 1.0),
('吉林省', 1.0),
('辽宁省', 1.0),

-- 华北地区 - 1.2元/斤
('北京市', 1.2),
('天津市', 1.2),
('河北省', 1.2),
('山西省', 1.2),
('内蒙古自治区', 1.2),
('山东省', 1.2),
('河南省', 1.2),

-- 华东地区 - 1.4元/斤
('上海市', 1.4),
('江苏省', 1.4),
('浙江省', 1.4),
('安徽省', 1.4),
('福建省', 1.4),
('江西省', 1.4),

-- 华中地区 - 1.4元/斤
('湖北省', 1.4),
('湖南省', 1.4),

-- 华南地区 - 1.4元/斤
('广东省', 1.4),
('广西壮族自治区', 1.4),
('海南省', 1.4),

-- 西南地区 - 1.4元/斤
('重庆市', 1.4),
('四川省', 1.4),
('贵州省', 1.4),
('云南省', 1.4),

-- 西北地区 - 1.4元/斤
('陕西省', 1.4),
('甘肃省', 1.4),
('宁夏回族自治区', 1.4),

-- 偏远地区 - 5.4元/斤
('西藏自治区', 5.4),
('青海省', 5.4),
('新疆维吾尔自治区', 5.4),

-- 特别行政区 - 1.4元/斤
('香港特别行政区', 1.4),
('澳门特别行政区', 1.4),
('台湾省', 1.4),

-- 默认运费 - 1.4元/斤
('default', 1.4);

-- 6.3 插入示例地址数据（可选，用于测试）
INSERT INTO "Addresses" ("Name", "Phone", "Province", "City", "District", "Detail", "IsDefault") VALUES
('张三', '13800138000', '广东省', '深圳市', '南山区', '科技园南路1号', true),
('李四', '13900139000', '北京市', '北京市', '朝阳区', '建国路88号SOHO现代城', false),
('王五', '13700137000', '上海市', '上海市', '浦东新区', '陆家嘴环路1000号', false);

-- =============================================
-- 7. 创建视图（便于查询）
-- =============================================

-- 7.1 订单详情视图
CREATE OR REPLACE VIEW "OrderDetails" AS
SELECT 
    o."Id" as "OrderId",
    o."OrderNo",
    o."Status",
    o."PaymentStatus",
    o."TotalRicePrice",
    o."TotalWeight",
    o."ShippingRate",
    o."TotalShipping",
    o."GrandTotal",
    o."CreatedAt",
    a."Name" as "AddressName",
    a."Phone" as "AddressPhone",
    a."Province" as "AddressProvince",
    a."City" as "AddressCity",
    a."District" as "AddressDistrict",
    a."Detail" as "AddressDetail"
FROM "Orders" o
LEFT JOIN "Addresses" a ON o."AddressId" = a."Id";

COMMENT ON VIEW "OrderDetails" IS '订单详情视图（含地址信息）';

-- 7.2 商品统计视图
CREATE OR REPLACE VIEW "ProductStats" AS
SELECT 
    p."Id",
    p."Name",
    p."Price",
    p."Unit",
    p."Weight",
    COUNT(oi."Id") as "OrderCount",
    COALESCE(SUM(oi."Quantity"), 0) as "TotalSold",
    COALESCE(SUM(oi."Subtotal"), 0) as "TotalRevenue"
FROM "Products" p
LEFT JOIN "OrderItems" oi ON p."Id" = oi."ProductId"
GROUP BY p."Id", p."Name", p."Price", p."Unit", p."Weight";

COMMENT ON VIEW "ProductStats" IS '商品统计视图（销售数据）';

-- 7.3 订单统计视图
CREATE OR REPLACE VIEW "OrderStats" AS
SELECT 
    DATE(o."CreatedAt") as "OrderDate",
    COUNT(o."Id") as "OrderCount",
    SUM(o."TotalRicePrice") as "TotalRevenue",
    SUM(o."TotalShipping") as "TotalShipping",
    SUM(o."GrandTotal") as "GrandTotal",
    AVG(o."GrandTotal") as "AvgOrderAmount"
FROM "Orders" o
GROUP BY DATE(o."CreatedAt")
ORDER BY "OrderDate" DESC;

COMMENT ON VIEW "OrderStats" IS '订单统计视图（按日期汇总）';

-- =============================================
-- 8. 创建存储过程/函数
-- =============================================

-- 8.1 生成订单号
CREATE OR REPLACE FUNCTION generate_order_number()
RETURNS TEXT AS $$
DECLARE
    order_no TEXT;
    counter INT := 0;
BEGIN
    LOOP
        -- 格式: ORD + 时间戳毫秒 + 3位计数器
        order_no := 'ORD' || EXTRACT(EPOCH FROM NOW())::BIGINT || LPAD(counter::TEXT, 3, '0');
        
        -- 检查订单号是否已存在
        IF NOT EXISTS (SELECT 1 FROM "Orders" WHERE "OrderNo" = order_no) THEN
            RETURN order_no;
        END IF;
        
        counter := counter + 1;
        
        -- 防止无限循环
        IF counter > 999 THEN
            RAISE EXCEPTION 'Unable to generate unique order number';
        END IF;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION generate_order_number() IS '生成唯一订单号';

-- 8.2 计算运费
CREATE OR REPLACE FUNCTION calculate_shipping_fee(
    p_province TEXT,
    p_weight DECIMAL
)
RETURNS TABLE(
    rate DECIMAL,
    shipping DECIMAL
) AS $$
DECLARE
    shipping_rate DECIMAL;
BEGIN
    -- 获取运费单价
    SELECT "Rate" INTO shipping_rate
    FROM "ShippingRates"
    WHERE "Province" = p_province;
    
    -- 如果没找到，使用默认运费
    IF shipping_rate IS NULL THEN
        SELECT "Rate" INTO shipping_rate
        FROM "ShippingRates"
        WHERE "Province" = 'default';
    END IF;
    
    -- 计算运费（保留2位小数）
    RETURN QUERY SELECT 
        shipping_rate as rate,
        ROUND((p_weight * shipping_rate)::NUMERIC, 2) as shipping;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION calculate_shipping_fee(TEXT, DECIMAL) IS '计算运费';

-- 8.3 设置默认地址（确保只有一个默认地址）
CREATE OR REPLACE FUNCTION set_default_address(p_address_id INT)
RETURNS VOID AS $$
BEGIN
    -- 先将所有地址设为非默认
    UPDATE "Addresses" SET "IsDefault" = FALSE;
    
    -- 设置指定地址为默认
    UPDATE "Addresses" 
    SET "IsDefault" = TRUE 
    WHERE "Id" = p_address_id;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION set_default_address(INT) IS '设置默认地址';

-- 8.4 获取订单统计（按状态）
CREATE OR REPLACE FUNCTION get_order_statistics()
RETURNS TABLE(
    status TEXT,
    order_count BIGINT,
    total_amount DECIMAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        o."Status" as status,
        COUNT(o."Id") as order_count,
        COALESCE(SUM(o."GrandTotal"), 0) as total_amount
    FROM "Orders" o
    GROUP BY o."Status"
    ORDER BY order_count DESC;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION get_order_statistics() IS '获取订单统计（按状态）';

-- =============================================
-- 9. 数据验证和测试
-- =============================================

-- 9.1 验证数据插入
SELECT 'Products' as table_name, COUNT(*) as record_count FROM "Products"
UNION ALL
SELECT 'Addresses', COUNT(*) FROM "Addresses"
UNION ALL
SELECT 'ShippingRates', COUNT(*) FROM "ShippingRates"
UNION ALL
SELECT 'Orders', COUNT(*) FROM "Orders"
UNION ALL
SELECT 'OrderItems', COUNT(*) FROM "OrderItems";

-- 9.2 测试运费计算
SELECT * FROM calculate_shipping_fee('广东省', 20.0);
SELECT * FROM calculate_shipping_fee('北京市', 15.0);
SELECT * FROM calculate_shipping_fee('西藏自治区', 10.0);
SELECT * FROM calculate_shipping_fee('不存在的省份', 10.0);  -- 应该使用默认运费

-- 9.3 测试订单号生成
SELECT generate_order_number() as new_order_no;
SELECT generate_order_number() as another_order_no;

-- 9.4 显示所有表
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_type = 'BASE TABLE'
ORDER BY table_name;

-- 9.5 显示所有视图
SELECT table_name 
FROM information_schema.views 
WHERE table_schema = 'public'
ORDER BY table_name;

-- 9.6 显示所有函数
SELECT 
    routine_name,
    routine_type,
    data_type as return_type
FROM information_schema.routines 
WHERE routine_schema = 'public'
ORDER BY routine_name;

-- =============================================
-- 10. 性能优化建议
-- =============================================

-- 10.1 启用统计信息收集
ANALYZE "Products";
ANALYZE "Addresses";
ANALYZE "Orders";
ANALYZE "OrderItems";
ANALYZE "ShippingRates";

-- 10.2 设置自动 VACUUM（PostgreSQL 通常默认开启）
-- 可以通过以下命令检查配置
-- SHOW autovacuum;

-- =============================================
-- 11. 安全性建议
-- =============================================

-- 11.1 创建只读用户（用于报表查询）
-- CREATE USER llxrice_readonly WITH PASSWORD 'readonly_password';
-- GRANT CONNECT ON DATABASE llxrice TO llxrice_readonly;
-- GRANT USAGE ON SCHEMA public TO llxrice_readonly;
-- GRANT SELECT ON ALL TABLES IN SCHEMA public TO llxrice_readonly;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO llxrice_readonly;

-- 11.2 启用行级安全（如果需要多租户）
-- ALTER TABLE "Orders" ENABLE ROW LEVEL SECURITY;

-- =============================================
-- 12. 备份和恢复命令（参考）
-- =============================================

-- 备份数据库
-- pg_dump -U llxrice_user -d llxrice -F c -b -v -f llxrice_backup_$(date +%Y%m%d).dump

-- 恢复数据库
-- pg_restore -U llxrice_user -d llxrice -v llxrice_backup_20251017.dump

-- 导出 SQL 文件
-- pg_dump -U llxrice_user -d llxrice -f llxrice_backup_$(date +%Y%m%d).sql

-- 导入 SQL 文件
-- psql -U llxrice_user -d llxrice -f llxrice_backup_20251017.sql

-- =============================================
-- 初始化完成
-- =============================================

-- 显示初始化结果
DO $$
DECLARE
    products_count INT;
    addresses_count INT;
    shipping_rates_count INT;
BEGIN
    SELECT COUNT(*) INTO products_count FROM "Products";
    SELECT COUNT(*) INTO addresses_count FROM "Addresses";
    SELECT COUNT(*) INTO shipping_rates_count FROM "ShippingRates";
    
    RAISE NOTICE '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    RAISE NOTICE '数据库初始化完成！';
    RAISE NOTICE '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    RAISE NOTICE 'Products: % 条记录', products_count;
    RAISE NOTICE 'Addresses: % 条记录', addresses_count;
    RAISE NOTICE 'ShippingRates: % 条记录', shipping_rates_count;
    RAISE NOTICE '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
END $$;

-- 显示商品列表
SELECT "Id", "Name", "Price", "Unit", "Weight" FROM "Products" ORDER BY "Id";

-- 显示运费配置（部分）
SELECT "Province", "Rate" 
FROM "ShippingRates" 
WHERE "Province" IN ('广东省', '北京市', '上海市', '西藏自治区', 'default')
ORDER BY "Rate", "Province";

-- =============================================
-- 脚本执行说明
-- =============================================

/*
使用方法：

1. 命令行执行：
   psql -U postgres -f 数据库初始化脚本.sql

2. Docker 容器内执行：
   docker exec -i llxrice_db psql -U llxrice_user -d llxrice < 数据库初始化脚本.sql

3. pgAdmin 中执行：
   打开 Query Tool，粘贴脚本内容并执行

4. 应用启动时自动执行：
   将脚本放在 docker-entrypoint-initdb.d 目录中

注意事项：
- 首次执行时会创建所有表和初始数据
- 重复执行会删除现有数据（DROP TABLE），请谨慎使用
- 生产环境建议先备份再执行
- 建议修改默认密码和敏感配置
*/

-- =============================================
-- END OF SCRIPT
-- =============================================
