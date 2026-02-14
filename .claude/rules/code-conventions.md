# Code Conventions

## 代码生成约定

项目使用代码生成器自动生成部分代码，许多类包含 `.Generate.cs` 文件。

### 生成文件列表

| 实体 | 主文件 | 生成文件 |
|------|--------|----------|
| `AlbumInfo` | `AlbumInfo.cs` | `AlbumInfo.Generate.cs` |
| `AlbumInfoContext` | `AlbumInfoContext.cs` | `AlbumInfoContext.Generate.cs` |
| `AudioInfo` | `AudioInfo.cs` | `AudioInfo.Generate.cs` |
| `AudioInfoContext` | `AudioInfoContext.cs` | `AudioInfoContext.Generate.cs` |
| `AlbumAudio` | `AlbumAudio.cs` | `AlbumAudio.Generate.cs` |
| `AlbumAudioContext` | `AlbumAudioContext.cs` | `AlbumAudioContext.Generate.cs` |

### 生成文件规则

**重要**：
- **不要手动修改 `.Generate.cs` 文件** - 它们会被代码生成器自动覆盖
- **自定义代码放在主文件中** - 如 `AlbumInfo.cs`，不会被覆盖

### 文件组织

```csharp
// AlbumInfo.Generate.cs（自动生成，不要修改）
public partial class AlbumInfo
{
    public Int32 AlbumID { get; set; }
    public string AlbumName { get; set; }
    // ... 其他自动生成的属性
}

// AlbumInfo.cs（自定义代码）
public partial class AlbumInfo
{
    // 自定义方法
    public string GetDisplayName()
    {
        return $"{AlbumID} - {AlbumName}";
    }

    // 自定义业务逻辑
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(AlbumName);
    }
}
```

## 编码规范

### 命名约定

#### 类命名

- **PascalCase**：`AlbumInfoContext`, `AudioDataProcess`
- **Context 后缀**：数据访问类以 `Context` 结尾
- **Controller 后缀**：控制器以 `Controller` 结尾

#### 方法命名

- **PascalCase**：`GetAlbumInfo`, `SaveAlbum`, `DeleteCache`
- **动词开头**：`Get`, `Add`, `Update`, `Delete`, `Is`, `Has`

#### 变量命名

- **camelCase**：`albumId`, `albumName`, `totalCount`
- **有意义的名称**：避免 `a`, `b`, `tmp` 等

#### 常量命名

- **PascalCase**：`ProviderName`, `DefaultPageSize`

### 文件组织

#### 单一职责

- 每个类只负责一个功能
- 文件大小控制在 200-400 行，最多不超过 800 行

#### 部分类（Partial Classes）

- 使用 `partial class` 分离自动生成和手动编写的代码
- Controller 可以使用 partial 类分离不同功能（如 `AlbumInfoController.List.cs`）

### 注释规范

#### XML 文档注释

```csharp
/// <summary>
/// 获取专辑信息
/// </summary>
/// <param name="albumId">专辑ID</param>
/// <returns>专辑信息，不存在返回 null</returns>
public AlbumInfo GetAlbum(Int32 albumId)
{
    return context.Get(albumId);
}
```

#### 行内注释

```csharp
// 清理 Redis 缓存
context.DeleteCacheEntity(id, providerName, db);

// 请求同步删除（多服务器环境）
context.GetSyncDeleteCacheEntityKey(id, providerName, db)
    .RequestSyncDeleteRedsKey();
```

### 空值检查

使用 LeoCore 的 `IsNull()` 扩展方法：

```csharp
var entity = context.Get(id);

if (entity.IsNull())
{
    return "记录不存在";
}
```

### 字符串处理

使用字符串插值：

```csharp
// 推荐
string message = $"专辑 {album.AlbumName} 已保存";

// 不推荐
string message = string.Format("专辑 {0} 已保存", album.AlbumName);
```

## 注意事项清单

开发过程中必须注意以下事项：

### 1. 目标框架混用

- **核心库**：`.NET 6.0`（`Baby.AudioData`, `Baby.AudioData.Core`）
- **应用层**：`.NET 10.0`（`ManageWeb`, `InterfaceWeb`, `AudioDataHosting`）

**注意**：不要在 .NET 6.0 项目中引用 .NET 10.0 特性。

### 2. 外部 DLL 依赖

- 确保 LeoCore DLL 在正确位置：`../../LeoCore/`
- .NET 6.0 项目引用 `net6.0` 版本 DLL
- .NET 10.0 项目引用 `net10.0` 版本 DLL

### 3. 生成代码

- **不要修改** `.Generate.cs` 文件
- **自定义代码**放在主文件（如 `AlbumInfo.cs`）

### 4. 缓存清理

删除数据时必须同步清理 Redis 缓存：

```csharp
// 1. 删除本地缓存
context.DeleteCacheEntity(id, providerName, db);
context.DeleteDependencyKey(providerName, db);

// 2. 请求同步删除（多服务器环境）
context.GetSyncDeleteCacheEntityKey(id, providerName, db)
    .RequestSyncDeleteRedsKey();
context.GetSyncDeleteDependencyKey(providerName, db)
    .RequestSyncDeleteRedsKey();
```

### 5. 健康检查

所有应用都必须启用健康检查端点（不能移除）：

```csharp
// Startup.cs
services.AddHealthChecks();

// Configure
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
});
```

### 6. 日志记录

重要操作应记录操作日志（后台管理）：

```csharp
WriteOperationLog(OperationType.Insert, id, "新增专辑：" + entity.AlbumName, entity);
WriteOperationLog(OperationType.Update, id, "修改专辑：" + entity.AlbumName, entity);
WriteOperationLog(OperationType.Delete, id, "删除专辑：" + entity.AlbumName, entity);
```

### 7. 中间件顺序

严格遵守 LeoCore 框架的中间件顺序：

```csharp
app.UseCorrectPath();          // 1. 修正双 // 地址
app.UseHttpContext();          // 2. HttpContext 操作

// 开发/生产环境异常处理

app.UseStaticFiles();          // 3. 静态文件
app.UseTelemetryUrl();         // 4. 性能监控（必须在 UseStaticFiles 之后）
app.UseRouting();              // 5. 路由
app.UseCors();                 // 6. 跨域
app.UsePowerSession();         // 7. 仅 ManageWeb
app.UseAuthentication();       // 8. 认证
app.UseAuthorization();        // 9. 授权
```

### 8. 权限控制

后台管理控制器必须使用 `PowerFilter` 特性：

```csharp
[Area("AudioDataManage")]
[Route("AudioDataManage/[controller]/[action]")]
public partial class AlbumInfoController : PowerController
{
    [PowerFilter(ButtonPlaceType.Top)]
    public ActionResult Add()
    {
        return View("Edit", new AlbumInfo());
    }
}
```

## 最佳实践

### 错误处理

```csharp
try
{
    context.Update(entity);
}
catch (Exception ex)
{
    Log.Error($"更新专辑失败：{ex.Message}");
    throw;
}
```

### 资源释放

```csharp
// 使用 using 自动释放资源
using (var context = new AlbumInfoContext())
{
    var album = context.Get(albumID);
    return album;
}
```

### 异步操作

对于 I/O 密集型操作，使用异步方法：

```csharp
public async Task<AlbumInfo> GetAlbumAsync(Int32 albumId)
{
    return await context.GetAsync(albumId);
}
```

## 相关文档

- **[Database Access](database-access.md)** - DataContext 使用规范
- **[Cache Management](cache-management.md)** - 缓存操作规范
- **[Backend Management](backend-management.md)** - Controller 编码规范
- **[Application Config](application-config.md)** - 中间件配置规范
