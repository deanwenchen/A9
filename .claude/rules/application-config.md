# Application Configuration

## 配置路径

应用根据操作系统从不同路径读取配置文件。

### 跨平台配置路径

```json
// appsettings.json
{
  "AppSettings": {
    "LeoLinuxPath": "/etc/app/dataconfig",
    "LeoWindowsPath": "C:\\TestConfig",
    "LeoOSXPath": "",
    "LeoPath": ""
  }
}
```

### 自动路径选择

LeoCore 的 `UseHostingConfig()` 扩展方法会自动选择配置路径：

| 操作系统 | 配置路径 |
|----------|----------|
| **Linux** | `/etc/app/dataconfig` |
| **Windows** | `C:\TestConfig` |
| **macOS** | `C:\TestConfig` (默认) |

### 配置文件示例

**Windows 配置文件**：`C:\TestConfig\appconfig.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BabyAudio;Uid=root;Pwd=password;"
  },
  "Redis": {
    "ConnectionString": "localhost:6379,abortConnect=false"
  },
  "JwtSettings": {
    "Secret": "your-secret-key",
    "Issuer": "Baby.Audio",
    "Audience": "Baby.Audio.Client"
  }
}
```

## 健康检查

所有应用都必须启用健康检查端点，不能移除。

### 配置健康检查

```csharp
// Startup.cs 或 Program.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddMySql(connectionString)           // MySQL 健康检查
        .AddRedis(redisConnectionString);     // Redis 健康检查
}

public void Configure(IApplicationBuilder app)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/health");
    });
}
```

### 访问健康检查

```bash
# HTTP 请求
curl http://localhost:5000/health

# 响应（健康）
HTTP/1.1 200 OK
Content-Type: text/plain

Healthy

# 响应（不健康）
HTTP/1.1 503 Service Unavailable
Content-Type: text/plain

Unhealthy
```

### 健康检查注意事项

- **不要移除** `/health` 端点
- 用于负载均衡器和容器编排的健康探测
- 返回 200 表示健康，503 表示不健康

## 生产环境配置

### 环境判断

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else if (env.IsProduction())
    {
        // 生产环境特定配置
        app.UseAppStoppingSleep(20000);  // 20秒优雅停机
        app.UseExceptionHandler("/Error");
    }
}
```

### 优雅停机

生产环境启用优雅停机，确保正在处理的请求完成：

```csharp
if (env.IsProduction())
{
    // 20 秒优雅停机时间
    app.UseAppStoppingSleep(20000);
}
```

**作用**：
- 收到停止信号后，等待 20 秒让现有请求完成
- 超时后强制关闭
- 避免用户请求中断

### 生产环境建议

1. **使用 HTTPS**：配置 SSL 证书
2. **限制 CORS**：限制允许的域名
3. **启用日志**：详细记录错误和访问日志
4. **监控**：集成 APM 监控
5. **缓存策略**：合理设置缓存过期时间

## 中间件管道

LeoCore 框架要求特定的中间件顺序，必须严格遵守。

### 完整中间件管道

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // 1. 修正双 // 地址
    app.UseCorrectPath();

    // 2. HttpContext 操作
    app.UseHttpContext();

    // 3. 异常处理
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    // 4. 静态文件
    app.UseStaticFiles();

    // 5. 性能监控（必须在 UseStaticFiles 之后）
    app.UseTelemetryUrl();

    // 6. 路由
    app.UseRouting();

    // 7. 跨域
    app.UseCors();

    // 8. Session（仅 ManageWeb）
    app.UsePowerSession();

    // 9. 认证
    app.UseAuthentication();

    // 10. 授权
    app.UseAuthorization();

    // 11. 端点配置
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        endpoints.MapHealthChecks("/health");
    });
}
```

### 中间件顺序说明

| 顺序 | 中间件 | 说明 | 必须 |
|------|--------|------|------|
| 1 | `UseCorrectPath` | 修正 URL 中的双 `//` | 是 |
| 2 | `UseHttpContext` | HttpContext 操作支持 | 是 |
| 3 | 异常处理 | 开发/生产环境不同 | 是 |
| 4 | `UseStaticFiles` | 静态文件服务 | 是 |
| 5 | `UseTelemetryUrl` | 性能监控（必须在 StaticFiles 后） | 是 |
| 6 | `UseRouting` | 路由配置 | 是 |
| 7 | `UseCors` | 跨域支持 | 是 |
| 8 | `UsePowerSession` | Session（仅 ManageWeb） | 条件 |
| 9 | `UseAuthentication` | 认证 | 是 |
| 10 | `UseAuthorization` | 授权 | 是 |

### 中间件重要性

**顺序错误会导致**：
- 静态文件无法访问
- 路由不工作
- 认证失效
- CORS 失败
- 性能监控不工作

## Startup 模板

### Web 应用模板（ManageWeb / InterfaceWeb）

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();

        // 健康检查
        services.AddHealthChecks()
            .AddMySql(Configuration.GetConnectionString("DefaultConnection"))
            .AddRedis(Configuration.GetConnectionString("Redis"));

        // CORS（InterfaceWeb）
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                if (env.IsDevelopment())
                {
                    builder.AllowAnyOrigin();
                }
                else
                {
                    builder.WithOrigins("https://example.com")
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
            });
        });

        // 认证（InterfaceWeb）
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(/* ... */);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCorrectPath();
        app.UseHttpContext();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseAppStoppingSleep(20000);
        }

        app.UseStaticFiles();
        app.UseTelemetryUrl();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapHealthChecks("/health");
        });
    }
}
```

### 后台服务模板（AudioDataHosting）

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .UseMemoryCache()
            .UseDBLogging()
            .UseHttpClient()
            .UseHostingConfig()
            .ConfigureServices((hostContext, services) =>
            {
                // 注册服务
                services.AddHostedService<Worker>();

                // 健康检查
                services.AddHealthChecks();
            })
            .Build()
            .UseServices()
            .RunAsync(async (host, args, cancellationToken) => {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Hosting Service started");

                await Task.Delay(Timeout.Infinite, cancellationToken);
            });
    }
}
```

## 注意事项

1. **配置路径**：确保配置文件在正确的系统路径
2. **健康检查**：不要移除 `/health` 端点
3. **中间件顺序**：严格遵守 LeoCore 框架要求的顺序
4. **优雅停机**：生产环境必须启用
5. **环境区分**：开发和生产环境使用不同配置
6. **敏感信息**：不要在配置文件中硬编码密钥

## 相关文档

- **[Backend Management](backend-management.md)** - ManageWeb 中间件配置
- **[Interface Development](interface-development.md)** - InterfaceWeb CORS 配置
- **[Hosting Service](hosting-service.md)** - 后台服务配置
- **[Dependencies](dependencies.md)** - 外部依赖配置
