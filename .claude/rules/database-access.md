# Database Access Pattern

## DataContext 基类

所有数据库上下文继承自 `DataContext<TEntity, TKey>`，提供标准 CRUD 操作和缓存集成。

### 基类特性

- **泛型参数**：`TEntity`（实体类型）、`TKey`（主键类型）
- **自动生成**：大部分代码在 `.Generate.cs` 文件中生成
- **内置缓存**：支持 Redis 分布式缓存
- **依赖键**：支持缓存依赖关系管理

## 定义 Context

### 基本定义

```csharp
namespace Baby.AudioData.Context
{
    public partial class AlbumInfoContext : DataContext<AlbumInfo, Int32>
    {
        // 生成代码在 AlbumInfoContext.Generate.cs 文件中
        // 自定义代码可以在这里添加
    }
}
```

### 常用 Context

| Context | 实体类型 | 主键类型 | 说明 |
|---------|----------|----------|------|
| `AlbumInfoContext` | `AlbumInfo` | `Int32` | 专辑信息 |
| `AlbumAudioContext` | `AlbumAudio` | `Int32` | 专辑音频关联 |
| `AudioInfoContext` | `AudioInfo` | `Int32` | 音频信息 |

## 基本操作

### 实例化 Context

```csharp
AlbumInfoContext context = new AlbumInfoContext();
```

### 查询操作

#### 基本查询

```csharp
// 根据主键查询
AlbumInfo album = context.Get(albumID);

// 检查记录是否存在
bool exists = context.Any("AlbumID = @albumID", new { albumID });

// 查询列表
var albums = context.GetList("Status = 1", null);

// 分页查询
var pageData = context.GetPageData(
    "Status = 1",           // WHERE 条件
    "CreateDate DESC",      // ORDER BY
    1,                      // 页码
    20                      // 每页数量
);
```

#### 带缓存的查询

```csharp
var album = context.GetCacheEntity(
    id,                     // 实体主键
    createCacheIfNotExists, // 不存在时是否创建缓存
    isDependencyKey,        // 是否使用依赖键
    cacheExpiry,            // 缓存过期时间
    entityExpiry,           // 实体过期时间
    AudioVariable.ProviderName, // Redis 提供者名称
    AudioVariable.Db        // Redis 数据库索引
);
```

### 添加操作

```csharp
AlbumInfo newAlbum = new AlbumInfo
{
    AlbumName = "新专辑",
    CreateDate = DateTime.Now
};

context.Add(newAlbum);
// Add 方法会自动设置主键并保存
```

### 更新操作

```csharp
AlbumInfo album = context.Get(albumID);
if (album.IsNull())
{
    // 处理不存在的情况
    return;
}

album.AlbumName = "更新后的名称";
album.ModifyDate = DateTime.Now;

context.Update(album);
```

### 删除操作

```csharp
// 根据主键删除
context.Delete(albumID);

// 删除实体
AlbumInfo album = context.Get(albumID);
context.Delete(album);
```

## 实体校验

### IsNull() 扩展方法

LeoCore 框架提供的扩展方法，用于判断对象是否为 null 或空：

```csharp
var entity = context.Get(id);

if (entity.IsNull())
{
    // 记录不存在或为 null
    return "记录不存在";
}

// 安全访问属性
string name = entity.AlbumName;
```

## 批量操作

### 批量查询

```csharp
// 使用 IN 子句
var ids = new List<int> { 1, 2, 3 };
var albums = context.GetList("AlbumID IN (@ids)", new { ids });
```

### 批量删除

```csharp
// 根据条件批量删除
int count = context.Delete("Status = 0", null);
```

## 缓存集成

### 查询时使用缓存

```csharp
// 从缓存获取，不存在则查询数据库并缓存
var album = context.GetCacheEntity(
    albumID,
    true,                           // createCacheIfNotExists
    false,                          // isDependencyKey
    TimeSpan.FromHours(6),          // cacheExpiry
    TimeSpan.FromHours(6),          // entityExpiry
    AudioVariable.ProviderName,
    AudioVariable.Db
);
```

### 删除缓存

```csharp
// 删除实体缓存
context.DeleteCacheEntity(id, providerName, db);

// 删除依赖键（清除相关所有缓存）
context.DeleteDependencyKey(providerName, db);
```

### 跨服务器缓存同步

多服务器环境下，需要同步删除其他服务器的缓存：

```csharp
// 1. 删除本地缓存
context.DeleteCacheEntity(id, providerName, db);
context.DeleteDependencyKey(providerName, db);

// 2. 请求同步删除（通过 Leo.DevOps.CacheSyncClient）
context.GetSyncDeleteCacheEntityKey(id, providerName, db)
    .RequestSyncDeleteRedsKey();

context.GetSyncDeleteDependencyKey(providerName, db)
    .RequestSyncDeleteRedsKey();
```

## 事务处理

DataContext 支持事务操作：

```csharp
using (var transaction = context.BeginTransaction())
{
    try
    {
        // 执行多个操作
        context.Add(album1);
        context.Add(album2);

        // 提交事务
        transaction.Commit();
    }
    catch
    {
        // 回滚事务
        transaction.Rollback();
        throw;
    }
}
```

## 完整 CRUD 示例

详细的后台管理 CRUD 示例，请参考：[Backend Management](backend-management.md)

## 相关文档

- **[Cache Management](cache-management.md)** - Redis 缓存策略和同步机制
- **[Backend Management](backend-management.md)** - 完整的 CRUD 控制器示例
- **[Code Conventions](code-conventions.md)** - DataContext 代码生成约定
