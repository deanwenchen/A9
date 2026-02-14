# Hosting Service (AudioDataHosting)

## 项目概述

`Baby.AudioDataHosting` 是后台服务，使用 ASP.NET Core Worker/后台服务模式。

**目标框架**：.NET 10.0

**用途**：
- 定时任务处理
- 后台数据处理
- 云存储集成
- Excel 文件处理

## 扩展方法链

LeoCore 框架提供了扩展方法链用于应用程序初始化。

### Web 应用初始化

```csharp
// Program.cs (ManageWeb / InterfaceWeb)
CreateHostBuilder(args)
    .UseDBLogging()        // 数据库日志
    .UseHostingConfig()    // 托管配置（从 /etc/app/dataconfig 或 C:\TestConfig 读取）
    .Build()
    .UseServices()         // 注册服务
    .Run();
```

### 后台服务初始化

```csharp
// Program.cs (AudioDataHosting)
Host.CreateDefaultBuilder(args)
    .UseMemoryCache()               // 内存缓存
    .UseDBLogging()                 // 数据库日志
    .UseHttpClient()                // HttpClient
    .UseHostingConfig()             // 托管配置
    .Build()
    .UseServices()                  // 注册服务
    .RunAsync(async (host, args, cancellationToken) => {
        // 后台服务启动逻辑
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Hosting Service started");

        // 保持服务运行
        await Task.Delay(Timeout.Infinite, cancellationToken);
    });
```

## 扩展方法说明

### UseDBLogging()

启用数据库日志记录：

```csharp
// 将日志写入数据库
builder.UseDBLogging();
```

**功能**：
- 自动记录应用程序日志到数据库
- 支持日志级别过滤
- 异步写入，不阻塞主线程

### UseHostingConfig()

从外部路径读取配置：

```csharp
builder.UseHostingConfig();
```

**配置路径**（自动根据操作系统选择）：
- **Linux**：`/etc/app/dataconfig`
- **Windows**：`C:\TestConfig`

详见：[Application Config](application-config.md)

### UseMemoryCache()

启用内存缓存：

```csharp
builder.UseMemoryCache();
```

**功能**：
- 注册 IMemoryCache 服务
- 适用于短期缓存
- 不持久化，重启后丢失

### UseHttpClient()

注册 HttpClient：

```csharp
builder.UseHttpClient();
```

**功能**：
- 注册 IHttpClientFactory
- 支持命名 HttpClient
- 自动处理连接池

### UseServices()

注册应用服务：

```csharp
host.UseServices();
```

**功能**：
- 扫描并注册自定义服务
- 依赖注入配置
- 生命周期管理

## 云存储集成

项目支持多个云存储提供商，在 `AudioDataHosting` 项目中集成。

### 支持的云存储

| 云服务商 | DLL 文件 | 说明 |
|----------|----------|------|
| 阿里云 | `Aliyun.OSS.dll` | 阿里云对象存储服务 |
| 华为云 | `OBS.dll` | 华为云对象存储服务 |
| UCloud | `UFile.dll` | UCloud 云文件存储 |
| 网易云 | `WcsLib.dll` | 网易云对象存储 |

### OSS 初始化

```csharp
// 使用 LeoCore 的 Leo.OSSData
public class OssService
{
    private readonly IOssClient _ossClient;

    public OssService()
    {
        // 初始化 OSS 客户端
        _ossClient = OssClientFactory.Create(
            provider: "Aliyun",  // 云服务商
            accessKeyId: "your-access-key",
            accessKeySecret: "your-secret-key",
            endpoint: "oss-cn-hangzhou.aliyuncs.com",
            bucketName: "your-bucket"
        );
    }

    public async Task<string> UploadFileAsync(string localPath, string remotePath)
    {
        await _ossClient.UploadAsync(localPath, remotePath);
        return $"https://your-bucket.oss-cn-hangzhou.aliyuncs.com/{remotePath}";
    }

    public async Task DeleteFileAsync(string remotePath)
    {
        await _ossClient.DeleteAsync(remotePath);
    }
}
```

### 文件上传示例

```csharp
public async Task ProcessAudioFileAsync(Int32 audioId)
{
    var audio = audioInfoContext.Get(audioId);
    if (audio.IsNull()) return;

    // 上传到 OSS
    var ossService = new OssService();
    string remotePath = $"audio/{audio.AudioID}/{audio.FileName}";

    await ossService.UploadFileAsync(audio.LocalPath, remotePath);

    // 更新数据库
    audio.OssUrl = ossService.GetFileUrl(remotePath);
    audioInfoContext.Update(audio);
}
```

## Excel 处理

项目使用 `Aspose.Cells` 库处理 Excel 文件。

### NuGet 包

```bash
dotnet add package Aspose.Cells
```

### 读取 Excel

```csharp
using Aspose.Cells;

public class ExcelService
{
    public List<AlbumInfo> ImportAlbums(string filePath)
    {
        Workbook workbook = new Workbook(filePath);
        Worksheet worksheet = workbook.Worksheets[0];

        var albums = new List<AlbumInfo>();

        // 从第二行开始（第一行是标题）
        for (int row = 1; row <= worksheet.Cells.MaxRow; row++)
        {
            var album = new AlbumInfo
            {
                AlbumName = worksheet.Cells[row, 0].StringValue,
                CreateDate = DateTime.Now
            };

            albums.Add(album);
        }

        return albums;
    }
}
```

### 导出 Excel

```csharp
public void ExportAlbums(string filePath, List<AlbumInfo> albums)
{
    Workbook workbook = new Workbook();
    Worksheet worksheet = workbook.Worksheets[0];

    // 标题行
    worksheet.Cells[0, 0].Value = "专辑ID";
    worksheet.Cells[0, 1].Value = "专辑名称";
    worksheet.Cells[0, 2].Value = "创建时间";

    // 数据行
    for (int i = 0; i < albums.Count; i++)
    {
        worksheet.Cells[i + 1, 0].Value = albums[i].AlbumID;
        worksheet.Cells[i + 1, 1].Value = albums[i].AlbumName;
        worksheet.Cells[i + 1, 2].Value = albums[i].CreateDate;
    }

    workbook.Save(filePath);
}
```

## 后台任务模式

### 定时任务

```csharp
public class TimedWorker : BackgroundService
{
    private readonly ILogger<TimedWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TimedWorker(
        ILogger<TimedWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TimedWorker started");

        // 每 5 分钟执行一次
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // 执行任务
                    await ProcessTaskAsync(scope.ServiceProvider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing task");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessTaskAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AlbumInfoContext>();

        // 处理逻辑
        var albums = context.GetList("Status = 0", null);
        foreach (var album in albums)
        {
            // 处理每个专辑
            await ProcessAlbumAsync(album);
        }
    }
}
```

### 注册后台服务

```csharp
// Program.cs
builder.Services.AddHostedService<TimedWorker>();
```

## 健康检查

后台服务也应该提供健康检查：

```csharp
// Startup.cs 或 Program.cs
services.AddHealthChecks()
    .AddCheck<HostingHealthCheck>("hosting_service");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
});
```

## 配置示例

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BabyAudio;Uid=root;Pwd=password;"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "OssSettings": {
    "Provider": "Aliyun",
    "AccessKeyId": "your-access-key",
    "AccessKeySecret": "your-secret-key",
    "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
    "BucketName": "your-bucket"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## 注意事项

1. **云存储配置**：根据实际使用的云服务商配置对应的 SDK
2. **Excel 处理**：Aspose.Cells 是商业库，需要授权
3. **后台任务**：使用依赖注入获取服务，使用 CreateScope 创建作用域
4. **异常处理**：后台任务必须有完善的异常处理
5. **日志记录**：重要操作必须记录日志
6. **资源释放**：及时释放资源，使用 `using` 语句

## 相关文档

- **[Application Config](application-config.md)** - 应用配置和中间件
- **[Dependencies](dependencies.md)** - 云存储 SDK 依赖说明
- **[Code Conventions](code-conventions.md)** - 编码规范
