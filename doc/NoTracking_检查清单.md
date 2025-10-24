# NoTracking 检查清单

## 快速识别指南

在全局配置了 `NoTracking` 的项目中，以下代码模式需要修复：

### ❌ 需要修复的模式

#### 模式1：查询后修改属性
```csharp
// 问题代码
var entity = await _context.Entities.FirstOrDefaultAsync(e => e.Id == id);
entity.Property = value;
await _context.SaveChangesAsync();

// 修复方案
var entity = await _context.Entities
    .AsTracking()  // 添加这行
    .FirstOrDefaultAsync(e => e.Id == id);
entity.Property = value;
await _context.SaveChangesAsync();
```

#### 模式2：使用 FindAsync 后修改
```csharp
// 问题代码
var entity = await _context.Entities.FindAsync(id);
entity.Property = value;
await _context.SaveChangesAsync();

// 修复方案
var entity = await _context.Entities
    .AsTracking()
    .FirstOrDefaultAsync(e => e.Id == id);
entity.Property = value;
await _context.SaveChangesAsync();
```

#### 模式3：批量查询后修改
```csharp
// 问题代码
var entities = await _context.Entities.Where(e => condition).ToListAsync();
foreach (var entity in entities)
{
    entity.Property = value;
}
await _context.SaveChangesAsync();

// 修复方案
var entities = await _context.Entities
    .AsTracking()  // 添加这行
    .Where(e => condition)
    .ToListAsync();
foreach (var entity in entities)
{
    entity.Property = value;
}
await _context.SaveChangesAsync();
```

### ✅ 不需要修复的模式

#### 模式1：使用 Update() 方法
```csharp
// 正确 - 不需要修复
_context.Entities.Update(entity);
await _context.SaveChangesAsync();
```

#### 模式2：使用 Remove() 方法
```csharp
// 正确 - 不需要修复
var entity = await _context.Entities.FindAsync(id);
_context.Entities.Remove(entity);
await _context.SaveChangesAsync();
```

#### 模式3：只读查询
```csharp
// 正确 - 应该继续使用 NoTracking
var entities = await _context.Entities.ToListAsync();
return entities; // 只是返回数据，不修改
```

## 检查流程

### 步骤1：查找所有更新方法
```bash
# 搜索包含 "Update" 的方法
grep -r "public async Task.*Update" Repositories/

# 搜索包含 "Set" 的方法
grep -r "public async Task.*Set" Repositories/
```

### 步骤2：识别问题代码
查找以下关键词组合：
1. `FirstOrDefaultAsync` + 后面有属性赋值
2. `FindAsync` + 后面有属性赋值
3. `Where().ToListAsync()` + 循环中有属性赋值

### 步骤3：验证修复
- [ ] 构建项目无错误
- [ ] 运行更新操作测试
- [ ] 检查数据库确认更新生效

## 快速检查列表

### Repository 方法检查

| Repository | 方法 | 模式 | 需要修复 | 状态 |
|-----------|------|------|---------|------|
| OrderRepository | UpdateStatusAsync | 查询后修改 | ✅ 是 | ✅ 已修复 |
| OrderRepository | UpdatePaymentStatusAsync | 查询后修改 | ✅ 是 | ✅ 已修复 |
| OrderRepository | UpdateAsync | 使用Update() | ❌ 否 | ✅ 正常 |
| ProductRepository | UpdateQuantityAsync | 查询后修改 | ✅ 是 | ✅ 已修复 |
| ProductRepository | UpdateAsync | 使用Update() | ❌ 否 | ✅ 正常 |
| AddressRepository | SetDefaultAsync | 查询后修改 | ✅ 是 | ✅ 已修复 |
| AddressRepository | UpdateAsync | 使用Update() | ❌ 否 | ✅ 正常 |
| ShippingRepository | UpdateAsync | 使用Update() | ❌ 否 | ✅ 正常 |

## 常见问题 FAQ

### Q: 如何判断一个方法是否需要修复？
**A:** 如果方法执行以下流程，则需要修复：
1. 查询实体（FirstOrDefaultAsync、FindAsync、ToListAsync）
2. 修改实体属性
3. 调用 SaveChangesAsync

### Q: Update() 和 AsTracking() 有什么区别？
**A:** 
- `AsTracking()` - 查询时启用跟踪，适合查询后修改属性
- `Update()` - 直接标记整个实体为Modified，不需要先查询

### Q: 为什么删除操作不需要修复？
**A:** `Remove()` 方法直接标记实体为Deleted状态，不依赖于跟踪机制。

### Q: 如何测试修复是否成功？
**A:** 
1. 调用更新API
2. 重新查询数据
3. 检查数据库表确认更新

### Q: 性能会受到影响吗？
**A:** 
- 只读查询：无影响（继续使用NoTracking）
- 更新操作：轻微增加（可接受，因为更新操作本身就慢）

## 代码审查要点

### 新增代码审查

在添加新的更新方法时，确保：
- [ ] 如果使用查询后修改模式，必须使用 `.AsTracking()`
- [ ] 添加注释说明为什么使用 AsTracking
- [ ] 考虑是否可以使用 `Update()` 或 `ExecuteUpdate`

### 示例代码模板

```csharp
/// <summary>
/// 更新实体的某个属性
/// </summary>
public async Task<bool> UpdatePropertyAsync(int id, TValue value)
{
    // 使用 AsTracking 确保实体被跟踪（全局配置了 NoTracking）
    var entity = await _context.Entities
        .AsTracking()
        .FirstOrDefaultAsync(e => e.Id == id);
        
    if (entity == null)
        return false;

    entity.Property = value;
    entity.UpdatedAt = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
    return true;
}
```

## 工具和命令

### 查找可能需要修复的代码
```bash
# 查找所有使用 FirstOrDefaultAsync 的地方
grep -n "FirstOrDefaultAsync" Repositories/*.cs

# 查找所有使用 FindAsync 的地方
grep -n "FindAsync" Repositories/*.cs

# 查找所有使用 SaveChangesAsync 的地方
grep -n "SaveChangesAsync" Repositories/*.cs
```

### 验证修复
```bash
# 构建项目
dotnet build

# 运行测试
dotnet test

# 检查linter错误
dotnet format --verify-no-changes
```

## 总结

**记住这个原则：**
> 在全局配置了 NoTracking 的项目中，所有 "查询后修改属性" 的操作都必须显式使用 `.AsTracking()`

**三个关键步骤：**
1. 🔍 识别：查找 "查询 + 修改 + Save" 的模式
2. ✏️ 修复：添加 `.AsTracking()` 到查询中
3. ✅ 验证：测试确认更新操作生效
