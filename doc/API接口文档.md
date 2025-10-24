# 林龍香大米商城 API 接口文档

## 📋 概述

本文档提供了林龍香大米商城后端服务的完整 API 接口文档，包括商品管理、地址管理、订单管理、运费计算和日志测试等功能模块。

## 🌐 基础信息

- **基础 URL**: `http://localhost:8080`
- **API 版本**: v1
- **数据格式**: JSON
- **字符编码**: UTF-8

## 📊 统一响应格式

所有 API 接口都使用统一的响应格式：

```json
{
  "success": true,
  "message": "操作成功",
  "data": {},
  "errors": null,
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 响应字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| success | boolean | 操作是否成功 |
| message | string | 响应消息 |
| data | object/array | 响应数据 |
| errors | array | 错误信息列表（仅在失败时返回） |
| timestamp | string | 响应时间戳（ISO 8601 格式） |

## 🛍️ 商品管理 API

### 1. 获取所有商品

**接口地址**: `GET /api/products`

**接口描述**: 获取系统中所有可用的商品列表

**请求参数**: 无

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "name": "稻花香",
      "price": 40.00,
      "unit": "袋",
      "weight": 10.00,
      "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
      "quantity": 100,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 2. 根据ID获取商品

**接口地址**: `GET /api/products/{id}`

**接口描述**: 根据商品ID获取特定商品的详细信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 商品ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "name": "稻花香",
    "price": 40.00,
    "unit": "袋",
    "weight": 10.00,
    "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
    "quantity": 100,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 3. 搜索商品

**接口地址**: `GET /api/products/search`

**接口描述**: 根据商品名称搜索商品

**查询参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 是 | 商品名称关键词 |

**请求示例**:
```
GET /api/products/search?name=稻花香
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "name": "稻花香",
      "price": 40.00,
      "unit": "袋",
      "weight": 10.00,
      "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
      "quantity": 100,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 4. 分页获取商品

**接口地址**: `GET /api/products/paged`

**接口描述**: 分页获取商品列表，支持排序和搜索

**查询参数**:
| 参数 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| pageNumber | integer | 否 | 1 | 页码 |
| pageSize | integer | 否 | 20 | 每页大小 |
| sortBy | string | 否 | null | 排序字段 |
| sortDescending | boolean | 否 | false | 是否降序 |
| searchTerm | string | 否 | null | 搜索关键词 |

**请求示例**:
```
GET /api/products/paged?pageNumber=1&pageSize=10&sortBy=name&sortDescending=false&searchTerm=稻花香
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "稻花香",
        "price": 40.00,
        "unit": "袋",
        "weight": 10.00,
        "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
        "quantity": 100,
        "createdAt": "2025-01-22T10:30:00Z",
        "updatedAt": "2025-01-22T10:30:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 5. 创建商品

**接口地址**: `POST /api/products`

**接口描述**: 创建新的商品

**请求体**:
```json
{
  "name": "稻花香",
  "price": 40.00,
  "unit": "袋",
  "weight": 10.00,
  "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
  "quantity": 100
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 是 | 商品名称 |
| price | decimal | 是 | 商品价格 |
| unit | string | 是 | 商品单位 |
| weight | decimal | 是 | 商品重量（kg） |
| image | string | 否 | 商品图片（Base64编码） |
| quantity | integer | 否 | 库存数量（默认0） |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "name": "稻花香",
    "price": 40.00,
    "unit": "袋",
    "weight": 10.00,
    "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
    "quantity": 100,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 6. 更新商品

**接口地址**: `PUT /api/products/{id}`

**接口描述**: 更新指定商品的信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 商品ID |

**请求体**:
```json
{
  "name": "稻花香（更新）",
  "price": 45.00,
  "unit": "袋",
  "weight": 10.00,
  "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
  "quantity": 150
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "name": "稻花香（更新）",
    "price": 45.00,
    "unit": "袋",
    "weight": 10.00,
    "image": "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2ZmZiIvPgogIDx0ZXh0IHg9IjUwIiB5PSI1MCIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjE0IiBmaWxsPSIjMzMzIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBkeT0iLjNlbSI+5rKz5rKz6ImyPC90ZXh0Pgo8L3N2Zz4K",
    "quantity": 150,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:35:00Z"
  },
  "timestamp": "2025-01-22T10:35:00Z"
}
```

### 7. 删除商品

**接口地址**: `DELETE /api/products/{id}`

**接口描述**: 删除指定的商品

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 商品ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:40:00Z"
}
```

### 8. 更新商品库存

**接口地址**: `PATCH /api/products/{id}/quantity`

**接口描述**: 更新指定商品的库存数量

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 商品ID |

**请求体**:
```json
200
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:45:00Z"
}
```

## 📍 地址管理 API

### 1. 获取所有地址

**接口地址**: `GET /api/addresses`

**接口描述**: 获取系统中所有地址列表

**请求参数**: 无

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "name": "张三",
      "phone": "13800138000",
      "province": "广东省",
      "city": "深圳市",
      "district": "南山区",
      "detail": "科技园南路1号",
      "isDefault": true,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 2. 根据ID获取地址

**接口地址**: `GET /api/addresses/{id}`

**接口描述**: 根据地址ID获取特定地址的详细信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 地址ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "name": "张三",
    "phone": "13800138000",
    "province": "广东省",
    "city": "深圳市",
    "district": "南山区",
    "detail": "科技园南路1号",
    "isDefault": true,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 3. 获取默认地址

**接口地址**: `GET /api/addresses/default`

**接口描述**: 获取当前设置的默认地址

**请求参数**: 无

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "name": "张三",
    "phone": "13800138000",
    "province": "广东省",
    "city": "深圳市",
    "district": "南山区",
    "detail": "科技园南路1号",
    "isDefault": true,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 4. 根据手机号获取地址列表

**接口地址**: `GET /api/addresses/phone/{phone}`

**接口描述**: 根据手机号获取相关的地址列表

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| phone | string | 是 | 手机号 |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "name": "张三",
      "phone": "13800138000",
      "province": "广东省",
      "city": "深圳市",
      "district": "南山区",
      "detail": "科技园南路1号",
      "isDefault": true,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 5. 创建地址

**接口地址**: `POST /api/addresses`

**接口描述**: 创建新的收货地址

**请求体**:
```json
{
  "name": "李四",
  "phone": "13900139000",
  "province": "北京市",
  "city": "北京市",
  "district": "朝阳区",
  "detail": "三里屯街道1号",
  "isDefault": false
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | 是 | 收货人姓名 |
| phone | string | 是 | 手机号 |
| province | string | 是 | 省份 |
| city | string | 是 | 城市 |
| district | string | 否 | 区县 |
| detail | string | 是 | 详细地址 |
| isDefault | boolean | 否 | 是否默认地址（默认false） |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 2,
    "name": "李四",
    "phone": "13900139000",
    "province": "北京市",
    "city": "北京市",
    "district": "朝阳区",
    "detail": "三里屯街道1号",
    "isDefault": false,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 6. 更新地址

**接口地址**: `PUT /api/addresses/{id}`

**接口描述**: 更新指定地址的信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 地址ID |

**请求体**:
```json
{
  "name": "李四（更新）",
  "phone": "13900139000",
  "province": "北京市",
  "city": "北京市",
  "district": "朝阳区",
  "detail": "三里屯街道2号",
  "isDefault": false
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 2,
    "name": "李四（更新）",
    "phone": "13900139000",
    "province": "北京市",
    "city": "北京市",
    "district": "朝阳区",
    "detail": "三里屯街道2号",
    "isDefault": false,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:35:00Z"
  },
  "timestamp": "2025-01-22T10:35:00Z"
}
```

### 7. 删除地址

**接口地址**: `DELETE /api/addresses/{id}`

**接口描述**: 删除指定的地址

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 地址ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:40:00Z"
}
```

### 8. 设置默认地址

**接口地址**: `PATCH /api/addresses/{id}/default`

**接口描述**: 将指定地址设置为默认地址

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 地址ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:45:00Z"
}
```

### 9. 智能解析地址

**接口地址**: `POST /api/addresses/parse`

**接口描述**: 智能解析地址文本，提取省市区和详细信息

**请求体**:
```json
{
  "fullAddress": "张三 13800138000 广东省深圳市南山区科技园南路1号"
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| fullAddress | string | 是 | 完整地址文本 |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "name": "张三",
    "phone": "13800138000",
    "province": "广东省",
    "city": "深圳市",
    "district": "南山区",
    "detail": "科技园南路1号"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

## 📦 订单管理 API

### 1. 获取所有订单

**接口地址**: `GET /api/orders`

**接口描述**: 获取系统中所有订单列表

**请求参数**: 无

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "orderNo": "ORD20250122001",
      "addressId": 1,
      "totalRicePrice": 80.00,
      "totalWeight": 20.00,
      "shippingRate": 5.00,
      "totalShipping": 5.00,
      "grandTotal": 85.00,
      "paymentStatus": "未支付",
      "status": "待处理",
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z",
      "address": {
        "id": 1,
        "name": "张三",
        "phone": "13800138000",
        "province": "广东省",
        "city": "深圳市",
        "district": "南山区",
        "detail": "科技园南路1号",
        "isDefault": true,
        "createdAt": "2025-01-22T10:30:00Z",
        "updatedAt": "2025-01-22T10:30:00Z"
      },
      "orderItems": [
        {
          "id": 1,
          "orderId": 1,
          "productId": 1,
          "productName": "稻花香",
          "productPrice": 40.00,
          "productUnit": "袋",
          "productWeight": 10.00,
          "quantity": 2,
          "subtotal": 80.00
        }
      ]
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 2. 根据ID获取订单

**接口地址**: `GET /api/orders/{id}`

**接口描述**: 根据订单ID获取特定订单的详细信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 订单ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "orderNo": "ORD20250122001",
    "addressId": 1,
    "totalRicePrice": 80.00,
    "totalWeight": 20.00,
    "shippingRate": 5.00,
    "totalShipping": 5.00,
    "grandTotal": 85.00,
    "paymentStatus": "未支付",
    "status": "待处理",
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z",
    "address": {
      "id": 1,
      "name": "张三",
      "phone": "13800138000",
      "province": "广东省",
      "city": "深圳市",
      "district": "南山区",
      "detail": "科技园南路1号",
      "isDefault": true,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    },
    "orderItems": [
      {
        "id": 1,
        "orderId": 1,
        "productId": 1,
        "productName": "稻花香",
        "productPrice": 40.00,
        "productUnit": "袋",
        "productWeight": 10.00,
        "quantity": 2,
        "subtotal": 80.00
      }
    ]
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 3. 根据订单号获取订单

**接口地址**: `GET /api/orders/order-no/{orderNo}`

**接口描述**: 根据订单号获取特定订单的详细信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| orderNo | string | 是 | 订单号 |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "orderNo": "ORD20250122001",
    "addressId": 1,
    "totalRicePrice": 80.00,
    "totalWeight": 20.00,
    "shippingRate": 5.00,
    "totalShipping": 5.00,
    "grandTotal": 85.00,
    "paymentStatus": "未支付",
    "status": "待处理",
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z",
    "address": {
      "id": 1,
      "name": "张三",
      "phone": "13800138000",
      "province": "广东省",
      "city": "深圳市",
      "district": "南山区",
      "detail": "科技园南路1号",
      "isDefault": true,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    },
    "orderItems": [
      {
        "id": 1,
        "orderId": 1,
        "productId": 1,
        "productName": "稻花香",
        "productPrice": 40.00,
        "productUnit": "袋",
        "productWeight": 10.00,
        "quantity": 2,
        "subtotal": 80.00
      }
    ]
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 4. 根据状态获取订单列表

**接口地址**: `GET /api/orders/status/{status}`

**接口描述**: 根据订单状态获取订单列表

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| status | string | 是 | 订单状态 |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "orderNo": "ORD20250122001",
      "addressId": 1,
      "totalRicePrice": 80.00,
      "totalWeight": 20.00,
      "shippingRate": 5.00,
      "totalShipping": 5.00,
      "grandTotal": 85.00,
      "paymentStatus": "未支付",
      "status": "待处理",
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z",
      "address": {
        "id": 1,
        "name": "张三",
        "phone": "13800138000",
        "province": "广东省",
        "city": "深圳市",
        "district": "南山区",
        "detail": "科技园南路1号",
        "isDefault": true,
        "createdAt": "2025-01-22T10:30:00Z",
        "updatedAt": "2025-01-22T10:30:00Z"
      },
      "orderItems": [
        {
          "id": 1,
          "orderId": 1,
          "productId": 1,
          "productName": "稻花香",
          "productPrice": 40.00,
          "productUnit": "袋",
          "productWeight": 10.00,
          "quantity": 2,
          "subtotal": 80.00
        }
      ]
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 5. 根据地址ID获取订单列表

**接口地址**: `GET /api/orders/address/{addressId}`

**接口描述**: 根据地址ID获取相关订单列表

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| addressId | integer | 是 | 地址ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "orderNo": "ORD20250122001",
      "addressId": 1,
      "totalRicePrice": 80.00,
      "totalWeight": 20.00,
      "shippingRate": 5.00,
      "totalShipping": 5.00,
      "grandTotal": 85.00,
      "paymentStatus": "未支付",
      "status": "待处理",
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z",
      "address": {
        "id": 1,
        "name": "张三",
        "phone": "13800138000",
        "province": "广东省",
        "city": "深圳市",
        "district": "南山区",
        "detail": "科技园南路1号",
        "isDefault": true,
        "createdAt": "2025-01-22T10:30:00Z",
        "updatedAt": "2025-01-22T10:30:00Z"
      },
      "orderItems": [
        {
          "id": 1,
          "orderId": 1,
          "productId": 1,
          "productName": "稻花香",
          "productPrice": 40.00,
          "productUnit": "袋",
          "productWeight": 10.00,
          "quantity": 2,
          "subtotal": 80.00
        }
      ]
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 6. 创建订单

**接口地址**: `POST /api/orders`

**接口描述**: 创建新的订单

**请求体**:
```json
{
  "addressId": 1,
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ],
  "paymentStatus": "未付款",
  "status": "待发货"
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| addressId | integer | 是 | - | 收货地址ID |
| items | array | 是 | - | 订单商品列表 |
| items[].productId | integer | 是 | - | 商品ID |
| items[].quantity | integer | 是 | - | 商品数量 |
| paymentStatus | string | 否 | "未付款" | 支付状态 |
| status | string | 否 | "待发货" | 订单状态 |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "orderNo": "ORD20250122001",
    "addressId": 1,
    "totalRicePrice": 80.00,
    "totalWeight": 20.00,
    "shippingRate": 5.00,
    "totalShipping": 5.00,
    "grandTotal": 85.00,
    "paymentStatus": "未支付",
    "status": "待处理",
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z",
    "address": {
      "id": 1,
      "name": "张三",
      "phone": "13800138000",
      "province": "广东省",
      "city": "深圳市",
      "district": "南山区",
      "detail": "科技园南路1号",
      "isDefault": true,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    },
    "orderItems": [
      {
        "id": 1,
        "orderId": 1,
        "productId": 1,
        "productName": "稻花香",
        "productPrice": 40.00,
        "productUnit": "袋",
        "productWeight": 10.00,
        "quantity": 2,
        "subtotal": 80.00
      }
    ]
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 7. 更新订单状态

**接口地址**: `PATCH /api/orders/{id}/status`

**接口描述**: 更新指定订单的状态

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 订单ID |

**请求体**:
```json
"已发货"
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:35:00Z"
}
```

### 8. 更新支付状态

**接口地址**: `PATCH /api/orders/{id}/payment-status`

**接口描述**: 更新指定订单的支付状态

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 订单ID |

**请求体**:
```json
"已支付"
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:35:00Z"
}
```

### 9. 删除订单

**接口地址**: `DELETE /api/orders/{id}`

**接口描述**: 删除指定的订单

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 订单ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:40:00Z"
}
```

### 10. 批量删除订单

**接口地址**: `POST /api/orders/batch/delete`

**接口描述**: 批量删除多个订单

**请求体**:
```json
{
  "ids": [1, 2, 3, 4, 5]
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| ids | array | 是 | 订单ID列表 |

**响应示例**:
```json
{
  "success": true,
  "message": "成功删除 5 个订单",
  "data": true,
  "timestamp": "2025-01-22T10:40:00Z"
}
```

**错误响应示例**:
```json
{
  "success": false,
  "message": "订单ID列表不能为空",
  "data": false,
  "timestamp": "2025-01-22T10:40:00Z"
}
```

### 11. 计算订单

**接口地址**: `POST /api/orders/calculate`

**接口描述**: 计算订单的总金额和运费

**请求体**:
```json
{
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ],
  "addressId": 1
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| items | array | 是 | 订单商品列表 |
| items[].productId | integer | 是 | 商品ID |
| items[].quantity | integer | 是 | 商品数量 |
| addressId | integer | 是 | 收货地址ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "totalRicePrice": 80.00,
    "totalWeight": 20.00,
    "shippingRate": 5.00,
    "totalShipping": 5.00,
    "grandTotal": 85.00,
    "province": "广东省"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

## 🚚 运费管理 API

### 1. 获取所有运费配置

**接口地址**: `GET /api/shipping/rates`

**接口描述**: 获取系统中所有运费配置列表

**请求参数**: 无

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": [
    {
      "id": 1,
      "province": "广东省",
      "rate": 5.00,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    },
    {
      "id": 2,
      "province": "北京市",
      "rate": 8.00,
      "createdAt": "2025-01-22T10:30:00Z",
      "updatedAt": "2025-01-22T10:30:00Z"
    }
  ],
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 2. 根据ID获取运费配置

**接口地址**: `GET /api/shipping/rates/{id}`

**接口描述**: 根据运费配置ID获取特定配置的详细信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 运费配置ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "province": "广东省",
    "rate": 5.00,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 3. 根据省份获取运费配置

**接口地址**: `GET /api/shipping/rates/province/{province}`

**接口描述**: 根据省份获取对应的运费配置

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| province | string | 是 | 省份名称 |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 1,
    "province": "广东省",
    "rate": 5.00,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 4. 创建运费配置

**接口地址**: `POST /api/shipping/rates`

**接口描述**: 创建新的运费配置

**请求体**:
```json
{
  "province": "上海市",
  "rate": 6.00
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| province | string | 是 | 省份名称 |
| rate | decimal | 是 | 运费单价（元/kg） |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 3,
    "province": "上海市",
    "rate": 6.00,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:30:00Z"
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 5. 更新运费配置

**接口地址**: `PUT /api/shipping/rates/{id}`

**接口描述**: 更新指定运费配置的信息

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 运费配置ID |

**请求体**:
```json
{
  "province": "上海市",
  "rate": 7.00
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "id": 3,
    "province": "上海市",
    "rate": 7.00,
    "createdAt": "2025-01-22T10:30:00Z",
    "updatedAt": "2025-01-22T10:35:00Z"
  },
  "timestamp": "2025-01-22T10:35:00Z"
}
```

### 6. 删除运费配置

**接口地址**: `DELETE /api/shipping/rates/{id}`

**接口描述**: 删除指定的运费配置

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | integer | 是 | 运费配置ID |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": true,
  "timestamp": "2025-01-22T10:40:00Z"
}
```

### 7. 计算运费

**接口地址**: `POST /api/shipping/calculate`

**接口描述**: 根据省份和重量计算运费

**请求体**:
```json
{
  "province": "广东省",
  "weight": 20.00
}
```

**请求体字段说明**:
| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| province | string | 是 | 省份名称 |
| weight | decimal | 是 | 重量（kg） |

**响应示例**:
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    "province": "广东省",
    "weight": 20.00,
    "rate": 5.00,
    "totalShipping": 100.00
  },
  "timestamp": "2025-01-22T10:30:00Z"
}
```

## 📝 日志测试 API

### 1. 测试所有日志级别

**接口地址**: `GET /api/logging-test/test-levels`

**接口描述**: 生成所有级别的日志记录，用于验证日志系统是否正常工作

**请求参数**: 无

**响应示例**:
```json
{
  "message": "所有日志级别测试完成，请检查日志文件",
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 2. 测试结构化日志

**接口地址**: `GET /api/logging-test/test-structured`

**接口描述**: 生成包含结构化参数的日志记录

**请求参数**: 无

**响应示例**:
```json
{
  "message": "结构化日志测试完成，请检查日志文件",
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 3. 测试异常日志

**接口地址**: `GET /api/logging-test/test-exceptions`

**接口描述**: 生成包含异常信息的日志记录

**请求参数**: 无

**响应示例**:
```json
{
  "message": "异常日志测试完成，请检查错误日志文件",
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 4. 测试不同类别日志

**接口地址**: `GET /api/logging-test/test-categories`

**接口描述**: 生成不同服务类别的日志记录

**请求参数**: 无

**响应示例**:
```json
{
  "message": "不同类别日志测试完成，请检查日志文件",
  "timestamp": "2025-01-22T10:30:00Z"
}
```

### 5. 获取日志文件信息

**接口地址**: `GET /api/logging-test/log-files`

**接口描述**: 列出当前所有日志文件及其信息

**请求参数**: 无

**响应示例**:
```json
{
  "message": "找到 3 个日志文件",
  "logDirectory": "logs",
  "files": [
    {
      "name": "info-2025-01-22.log",
      "size": 1024,
      "lastWriteTime": "2025-01-22T10:30:00Z",
      "fullPath": "/app/logs/info-2025-01-22.log"
    },
    {
      "name": "warning-2025-01-22.log",
      "size": 512,
      "lastWriteTime": "2025-01-22T10:25:00Z",
      "fullPath": "/app/logs/warning-2025-01-22.log"
    },
    {
      "name": "error-2025-01-22.log",
      "size": 256,
      "lastWriteTime": "2025-01-22T10:20:00Z",
      "fullPath": "/app/logs/error-2025-01-22.log"
    }
  ]
}
```

## 🔧 系统健康检查 API

### 1. 健康检查

**接口地址**: `GET /health`

**接口描述**: 检查系统健康状态

**请求参数**: 无

**响应示例**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-22T10:30:00Z",
  "services": {
    "database": "Healthy",
    "redis": "Healthy",
    "api": "Healthy"
  }
}
```

## 📊 订单状态说明

### 支付状态 (PaymentStatus)
| 状态值 | 说明 |
|--------|------|
| 未付款 | 订单创建后默认状态 |
| 已付款 | 支付完成 |
| 部分付款 | 部分支付 |
| 已退款 | 已退款 |
| 退款中 | 退款处理中 |

### 订单状态 (Status)
| 状态值 | 说明 |
|--------|------|
| 待发货 | 订单创建后默认状态 |
| 已发货 | 商品已发出 |
| 运输中 | 商品运输中 |
| 已送达 | 商品已送达 |
| 已完成 | 订单完成 |
| 已取消 | 订单已取消 |
| 退货中 | 退货处理中 |
| 已退货 | 退货完成 |

## 📊 错误码说明

### HTTP 状态码

| 状态码 | 说明 |
|--------|------|
| 200 | 请求成功 |
| 201 | 创建成功 |
| 400 | 请求参数错误 |
| 404 | 资源不存在 |
| 500 | 服务器内部错误 |

### 业务错误码

| 错误码 | 说明 |
|--------|------|
| PRODUCT_NOT_FOUND | 商品不存在 |
| ADDRESS_NOT_FOUND | 地址不存在 |
| ORDER_NOT_FOUND | 订单不存在 |
| SHIPPING_RATE_NOT_FOUND | 运费配置不存在 |
| ~~INSUFFICIENT_STOCK~~ | ~~库存不足~~ (已移除) |
| INVALID_QUANTITY | 数量无效 |
| INVALID_ADDRESS | 地址格式无效 |
| INVALID_PAYMENT_STATUS | 支付状态无效 |
| INVALID_ORDER_STATUS | 订单状态无效 |

## 🧪 测试工具

### Swagger UI

访问 `http://localhost:8080/swagger` 可以查看完整的 API 文档和进行接口测试。

### Postman 集合

可以使用以下 Postman 集合进行接口测试：

```json
{
  "info": {
    "name": "林龍香大米商城 API",
    "description": "林龍香大米商城后端服务 API 接口集合",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "商品管理",
      "item": [
        {
          "name": "获取所有商品",
          "request": {
            "method": "GET",
            "header": [],
            "url": {
              "raw": "{{baseUrl}}/api/products",
              "host": ["{{baseUrl}}"],
              "path": ["api", "products"]
            }
          }
        }
      ]
    }
  ]
}
```

## 📞 技术支持

如有问题，请联系开发团队或查看相关文档：

- [部署指南](./API服务独立部署指南.md)
- [Linux 部署指南](./API服务Linux部署指南.md)
- [多环境配置指南](./多环境配置管理指南.md)
- [脚本使用指南](./linux.md)

---

**文档版本**: v1.0  
**最后更新**: 2025-01-22  
**维护团队**: 林龍香大米商城开发团队
