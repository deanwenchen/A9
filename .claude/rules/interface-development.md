# Interface Development (InterfaceWeb)

## 项目概述

`Baby.AudioData.InterfaceWeb` 是对外 API 服务，提供 RESTful API 接口。

**目标框架**：.NET 10.0

**特点**：
- 使用 ASP.NET Core MVC
- 返回 JSON 格式数据
- 支持 CORS 跨域
- JWT Bearer 认证

## 路由配置

### 传统 MVC 路由

InterfaceWeb 不使用 Areas，使用传统 MVC 路由：

```csharp
// Startup.cs
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
```

### 路由格式

```
http://domain/{controller}/{action}/{id}
```

示例：
- `/Album/Get?id=123` - 获取专辑信息
- `/Audio/List` - 获取音频列表
- `/Album/Search?keyword=test` - 搜索专辑

## 控制器定义

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Baby.AudioData.InterfaceWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AlbumController : ControllerBase
    {
        AlbumInfoContext context = new AlbumInfoContext();

        [HttpGet]
        public IActionResult Get(Int32 id)
        {
            var album = context.Get(id);
            if (album.IsNull())
            {
                return NotFound();
            }
            return Ok(album);
        }
    }
}
```

## 接口响应格式

### AddInterfaceResult() 方法

使用 LeoCore 的 `AddInterfaceResult()` 添加接口响应封装：

```csharp
public JsonInfoResult GetAlbum(Int32 id)
{
    var invokeResult = new InvokeResult();
    var album = context.Get(id);

    if (album.IsNull())
    {
        invokeResult.ResultCode = "Error";
        invokeResult.ResultMessage = "专辑不存在";
        return JsonInfo(invokeResult);
    }

    invokeResult.ResultData = album;
    invokeResult.ResultCode = "Success";
    return JsonInfo(invokeResult);
}
```

### JsonInfoResult 响应结构

```json
{
  "resultCode": "Success",
  "resultMessage": "",
  "data": {
    "albumID": 123,
    "albumName": "专辑名称"
  }
}
```

### 标准响应码

| ResultCode | 说明 | HTTP Status |
|------------|------|-------------|
| `Success` | 成功 | 200 |
| `Error` | 一般错误 | 400 |
| `NotFound` | 资源不存在 | 404 |
| `Unauthorized` | 未授权 | 401 |
| `HintMessage` | 提示消息 | 200 |

## 跨域配置 (CORS)

### 开发环境

允许所有来源：

```csharp
// Startup.cs - Development
services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

app.UseCors();
```

### 生产环境

限制特定域名：

```csharp
// Startup.cs - Production
services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://example.com", "https://*.xxxx.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

app.UseCors();
```

## 身份认证

### JWT Bearer 认证

InterfaceWeb 使用 JWT Bearer 认证：

```csharp
// appsettings.json
{
  "JwtSettings": {
    "Secret": "your-secret-key",
    "Issuer": "Baby.Audio",
    "Audience": "Baby.Audio.Client",
    "ExpiryMinutes": 1440
  }
}

// Startup.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Baby.Audio",
            ValidAudience = "Baby.Audio.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key"))
        };
    });

app.UseAuthentication();
app.UseAuthorization();
```

### IdentityModel 集成

项目使用 `IdentityModel` 进行身份认证：

```bash
# NuGet 包
IdentityModel
Microsoft.AspNetCore.Authentication.JwtBearer
```

### 控制器授权

```csharp
[HttpGet]
[Authorize]  // 需要认证
public IActionResult GetUserData()
{
    var userId = User.FindFirst("sub")?.Value;
    // ...
}

[HttpGet]
[AllowAnonymous]  // 允许匿名访问
public IActionResult PublicData()
{
    // ...
}
```

## 接口开发示例

### GET 请求 - 获取单个实体

```csharp
[HttpGet]
public JsonInfoResult GetAlbum(Int32 id)
{
    var invokeResult = new InvokeResult();
    var album = context.Get(id);

    if (album.IsNull())
    {
        invokeResult.ResultCode = "NotFound";
        invokeResult.ResultMessage = "专辑不存在";
        return JsonInfo(invokeResult);
    }

    invokeResult.ResultData = album;
    invokeResult.ResultCode = "Success";
    return JsonInfo(invokeResult);
}
```

### GET 请求 - 获取列表

```csharp
[HttpGet]
public JsonInfoResult GetAlbumList(Int32 page = 1, Int32 pageSize = 20)
{
    var invokeResult = new InvokeResult();

    var pageData = context.GetPageData(
        "Status = 1",              // WHERE 条件
        "CreateDate DESC",         // ORDER BY
        page,
        pageSize
    );

    invokeResult.ResultData = new
    {
        list = pageData.Items,
        total = pageData.TotalCount,
        page = page,
        pageSize = pageSize
    };
    invokeResult.ResultCode = "Success";
    return JsonInfo(invokeResult);
}
```

### POST 请求 - 创建实体

```csharp
[HttpPost]
public JsonInfoResult CreateAlbum([FromBody] AlbumInfoDto dto)
{
    var invokeResult = new InvokeResult();

    // 验证
    if (string.IsNullOrEmpty(dto.AlbumName))
    {
        invokeResult.ResultCode = "Error";
        invokeResult.ResultMessage = "专辑名称不能为空";
        return JsonInfo(invokeResult);
    }

    // 创建实体
    var album = new AlbumInfo
    {
        AlbumName = dto.AlbumName,
        CreateDate = DateTime.Now
    };

    context.Add(album);

    invokeResult.ResultData = new { albumId = album.AlbumID };
    invokeResult.ResultCode = "Success";
    invokeResult.ResultMessage = "创建成功";
    return JsonInfo(invokeResult);
}
```

### PUT 请求 - 更新实体

```csharp
[HttpPut]
public JsonInfoResult UpdateAlbum(Int32 id, [FromBody] AlbumInfoDto dto)
{
    var invokeResult = new InvokeResult();
    var album = context.Get(id);

    if (album.IsNull())
    {
        invokeResult.ResultCode = "NotFound";
        invokeResult.ResultMessage = "专辑不存在";
        return JsonInfo(invokeResult);
    }

    // 更新
    album.AlbumName = dto.AlbumName;
    album.ModifyDate = DateTime.Now;
    context.Update(album);

    // 清理缓存
    context.DeleteCacheEntity(album.AlbumID, AudioVariable.ProviderName, AudioVariable.Db);
    context.GetSyncDeleteCacheEntityKey(album.AlbumID, AudioVariable.ProviderName, AudioVariable.Db)
        .RequestSyncDeleteRedsKey();

    invokeResult.ResultCode = "Success";
    invokeResult.ResultMessage = "更新成功";
    return JsonInfo(invokeResult);
}
```

### DELETE 请求 - 删除实体

```csharp
[HttpDelete]
public JsonInfoResult DeleteAlbum(Int32 id)
{
    var invokeResult = new InvokeResult();
    var album = context.Get(id);

    if (album.IsNull())
    {
        invokeResult.ResultCode = "NotFound";
        invokeResult.ResultMessage = "专辑不存在";
        return JsonInfo(invokeResult);
    }

    // 删除
    context.Delete(id);

    // 清理缓存
    context.DeleteCacheEntity(id, AudioVariable.ProviderName, AudioVariable.Db);
    context.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
    context.GetSyncDeleteCacheEntityKey(id, AudioVariable.ProviderName, AudioVariable.Db)
        .RequestSyncDeleteRedsKey();
    context.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db)
        .RequestSyncDeleteRedsKey();

    invokeResult.ResultCode = "Success";
    invokeResult.ResultMessage = "删除成功";
    return JsonInfo(invokeResult);
}
```

## 健康检查

InterfaceWeb 必须保留健康检查端点：

```csharp
// Startup.cs
services.AddHealthChecks();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
    endpoints.MapControllers();
});
```

访问 `http://domain/health` 检查服务状态。

## 注意事项

1. **CORS 配置**：生产环境必须限制允许的域名
2. **认证授权**：公开接口使用 `[AllowAnonymous]`，私有接口使用 `[Authorize]`
3. **缓存管理**：更新和删除操作必须清理缓存
4. **错误处理**：使用统一的 `JsonInfoResult` 响应格式
5. **参数验证**：验证所有输入参数
6. **健康检查**：不要移除 `/health` 端点

## 相关文档

- **[Database Access](database-access.md)** - DataContext 数据访问
- **[Cache Management](cache-management.md)** - 缓存清理机制
- **[Application Config](application-config.md)** - 应用配置和中间件
