# Dependencies

## LeoCore 框架依赖

项目严重依赖外部 `LeoCore` 框架（通过 DLL 引用），位于相对路径 `../../LeoCore/`。

### .NET 6.0 版本

用于 `Baby.AudioData` 和 `Baby.AudioData.Core` 项目：

| 程序集 | 说明 |
|--------|------|
| `Leo.Core` | 核心功能和扩展方法 |
| `Leo.Data` | 数据访问基础类 |
| `Leo.Log` | 日志记录功能 |
| `Leo.OSSData` | 对象存储数据操作 |
| `Leo.Data.Redis` | Redis 缓存集成 |
| `MySql.Data` | MySQL 数据库驱动 |

### .NET 10.0 版本

用于三个应用项目（`ManageWeb`、`InterfaceWeb`、`AudioDataHosting`）：

**包含 .NET 6.0 所有库的 10.0 版本，以及以下扩展：**

| 程序集 | 说明 | 使用项目 |
|--------|------|----------|
| `Leo.Mvc` | MVC 框架扩展 | ManageWeb, InterfaceWeb |
| `Leo.Mvc.Client` | MVC 客户端功能 | ManageWeb |
| `Leo.PowerData` | 权限数据管理 | ManageWeb |
| `Leo.PowerData.Mvc` | MVC 权限控制 | ManageWeb |
| `Leo.TagControls` | 标签控件库 | ManageWeb |
| `Leo.Tags` | 标签功能 | ManageWeb |
| `Leo.FileData` | 文件操作 | 所有项目 |
| `Leo.TopSdk` | 顶点 SDK 集成 | 所有项目 |

## 第三方云存储 SDK

项目支持多个云存储提供商（在 `AudioDataHosting` 项目中使用）：

| DLL 文件 | 云服务商 | 说明 |
|----------|----------|------|
| `Aliyun.OSS.dll` | 阿里云 | 阿里云对象存储服务 |
| `OBS.dll` | 华为云 | 华为云对象存储服务 |
| `UFile.dll` | UCloud | UCloud 云文件存储 |
| `WcsLib.dll` | 网易云 | 网易云对象存储 |

## NuGet 包依赖

### 通用包（所有项目）

| 包名 | 版本 | 用途 |
|------|------|------|
| `MySql.Data` | - | MySQL 数据库连接 |
| `Newtonsoft.Json` | - | JSON 序列化和反序列化 |

### 缓存和同步

| 包名 | 用途 |
|------|------|
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Redis 分布式缓存 |
| `Leo.DevOps.CacheSyncClient` | 多服务器缓存同步 |

### Web 相关

| 包名 | 使用项目 | 用途 |
|------|----------|------|
| `SixLabors.ImageSharp` | ManageWeb | 图像处理 |

### 后台服务专用

| 包名 | 用途 |
|------|------|
| `Aspose.Cells` | Excel 文件处理 |

### 接口服务专用

| 包名 | 用途 |
|------|------|
| `IdentityModel` | 身份认证客户端 |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT Bearer 认证 |

## DLL 路径配置

LeoCore 框架 DLL 通过相对路径引用：

```
解决方案根目录/
└── ../../LeoCore/          # LeoCore 框架根目录
    ├── net6.0/             # .NET 6.0 版本 DLL
    │   ├── Leo.Core.dll
    │   ├── Leo.Data.dll
    │   └── ...
    └── net10.0/            # .NET 10.0 版本 DLL
        ├── Leo.Mvc.dll
        ├── Leo.PowerData.dll
        └── ...
```

**重要**：确保 LeoCore 目录在正确位置，否则项目无法编译。

## 依赖管理注意事项

1. **版本一致性**：.NET 6.0 和 .NET 10.0 的 LeoCore DLL 必须使用各自对应版本
2. **DLL 位置**：LeoCore 框架必须在 `../../LeoCore/` 相对路径
3. **云存储 SDK**：根据实际使用的云存储提供商，确保对应 DLL 存在
4. **NuGet 还原**：使用 `dotnet restore` 还原所有 NuGet 包

## 相关文档

- **[Project Structure](project-structure.md)** - 项目结构和组织
- **[Hosting Service](hosting-service.md)** - 云存储集成使用
- **[Cache Management](cache-management.md)** - Redis 缓存配置
