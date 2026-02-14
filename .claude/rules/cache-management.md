# Cache Management

## Redis 配置

项目使用 Redis 分布式缓存，配置集中管理在 `AudioVariable` 类中。

### AudioVariable 类

```csharp
public class AudioVariable
{
    /// <summary>
    /// Redis 提供者名称
    /// </summary>
    public static string ProviderName => "Baby.Audio";

    /// <summary>
    /// Redis 数据库索引（-1 表示默认数据库）
    /// </summary>
    public static int Db => -1;

    /// <summary>
    /// 缓存默认过期时间
    /// </summary>
    public static TimeSpan ExpiresIn => TimeSpan.FromHours(6);
}
```

### 配置说明

| 参数 | 值 | 说明 |
|------|-----|------|
| `ProviderName` | `"Baby.Audio"` | Redis 提供者名称，用于标识应用 |
| `Db` | `-1` | Redis 数据库索引，-1 表示使用默认数据库 |
| `ExpiresIn` | `6小时` | 默认缓存过期时间 |

## 缓存操作

### 获取缓存

使用 DataContext 的 `GetCacheEntity()` 方法从缓存获取实体：

```csharp
AlbumInfoContext context = new AlbumInfoContext();

var album = context.GetCacheEntity(
    albumID,                        // 实体主键
    true,                           // 不存在时是否创建缓存
    false,                          // 是否使用依赖键
    TimeSpan.FromHours(6),          // 缓存过期时间
    TimeSpan.FromHours(6),          // 实体过期时间
    AudioVariable.ProviderName,     // Redis 提供者名称
    AudioVariable.Db                // Redis 数据库索引
);
```

**参数说明**：

| 参数 | 类型 | 说明 |
|------|------|------|
| `id` | `TKey` | 实体主键 |
| `createCacheIfNotExists` | `bool` | 缓存不存在时是否从数据库加载并创建缓存 |
| `isDependencyKey` | `bool` | 是否使用依赖键管理 |
| `cacheExpiry` | `TimeSpan` | 缓存过期时间 |
| `entityExpiry` | `TimeSpan` | 实体过期时间 |
| `providerName` | `string` | Redis 提供者名称 |
| `db` | `int` | Redis 数据库索引 |

### 删除缓存

#### 删除实体缓存

```csharp
// 删除指定实体的缓存
context.DeleteCacheEntity(
    albumID,                    // 实体主键
    AudioVariable.ProviderName, // Redis 提供者名称
    AudioVariable.Db            // Redis 数据库索引
);
```

#### 删除依赖键

依赖键用于批量清除相关缓存：

```csharp
// 删除依赖键（清除该键下的所有缓存）
context.DeleteDependencyKey(
    AudioVariable.ProviderName, // Redis 提供者名称
    AudioVariable.Db            // Redis 数据库索引
);
```

## 缓存同步机制

在多服务器环境下，删除数据时需要同步清理所有服务器的 Redis 缓存。

项目使用 `Leo.DevOps.CacheSyncClient` 实现跨服务器缓存同步。

### 本地缓存删除

```csharp
// 1. 删除实体缓存
context.DeleteCacheEntity(id, providerName, db);

// 2. 删除依赖键
context.DeleteDependencyKey(providerName, db);
```

### 跨服务器同步删除

```csharp
// 3. 请求同步删除实体缓存键
context.GetSyncDeleteCacheEntityKey(id, providerName, db)
    .RequestSyncDeleteRedsKey();

// 4. 请求同步删除依赖键
context.GetSyncDeleteDependencyKey(providerName, db)
    .RequestSyncDeleteRedsKey();
```

**说明**：`RequestSyncDeleteRedsKey()` 方法会通知所有服务器删除指定的 Redis 键。

## 完整的缓存清理示例

在删除数据操作中，完整的缓存清理流程：

```csharp
public JsonInfoResult Remove(Int32 albumID)
{
    var invokeResult = new InvokeResult();
    AlbumInfoContext context = new AlbumInfoContext();

    var entity = context.Get(albumID);
    if (entity.IsNull())
    {
        invokeResult.ResultCode = "HintMessage";
        invokeResult.ResultMessage = "此记录不存在，因此无法删除";
        return JsonInfo(invokeResult);
    }

    // 1. 执行数据库删除
    context.Delete(albumID);

    // 2. 删除本地缓存
    context.DeleteCacheEntity(entity.AlbumID, AudioVariable.ProviderName, AudioVariable.Db);
    context.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);

    // 3. 请求同步删除（多服务器环境）
    context.GetSyncDeleteCacheEntityKey(entity.AlbumID, AudioVariable.ProviderName, AudioVariable.Db)
        .RequestSyncDeleteRedsKey();
    context.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db)
        .RequestSyncDeleteRedsKey();

    invokeResult.EventAlert("删除成功").EventRefreshGrid();
    return JsonInfo(invokeResult);
}
```

## 缓存键命名规范

### 实体缓存键

格式：`{ProviderName}:Entity:{EntityType}:{Id}`

示例：`Baby.Audio:Entity:AlbumInfo:123`

### 依赖键

格式：`{ProviderName}:Dependency:{EntityType}`

示例：`Baby.Audio:Dependency:AlbumInfo`

## 缓存策略建议

### 何时使用缓存

- **读取频繁的数据**：专辑列表、音频信息
- **计算成本高的数据**：统计数据、聚合查询
- **访问量大但更新少**：配置数据、元数据

### 何时清除缓存

- **数据更新时**：更新实体后清除对应缓存
- **数据删除时**：必须清除缓存和依赖键
- **批量操作时**：使用依赖键批量清除

### 缓存过期时间

根据数据更新频率设置：
- **频繁更新**：1-2 小时
- **一般更新**：6 小时（默认）
- **很少更新**：12-24 小时

## 注意事项

1. **缓存一致性**：更新数据后必须清除缓存
2. **同步删除**：多服务器环境必须调用同步删除方法
3. **依赖键管理**：合理使用依赖键进行批量缓存清理
4. **缓存穿透**：使用 `createCacheIfNotExists` 防止缓存穿透
5. **过期时间**：根据业务特点设置合理的过期时间

## 相关文档

- **[Database Access](database-access.md)** - DataContext 缓存操作
- **[Backend Management](backend-management.md)** - CRUD 中的缓存处理示例
- **[Application Config](application-config.md)** - Redis 连接配置
