# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

宝宝巴士音频数据管理系统，基于 ASP.NET Core 开发，包含 5 个项目：

- **AudioData** (.NET 6.0) - 数据实体和 DataContext
- **AudioData.Core** (.NET 6.0) - 业务逻辑层
- **ManageWeb** (.NET 10.0) - 后台管理 (MVC + Areas + PowerData)
- **InterfaceWeb** (.NET 10.0) - 对外 API 服务
- **AudioDataHosting** (.NET 10.0) - 后台服务

## 常用命令

```bash
# 构建
dotnet build Baby.AudioData\Baby.AudioData.csproj
dotnet build Baby.AudioData.Core\Baby.AudioData.Core.csproj
dotnet build Baby.AudioData.ManageWeb\Baby.AudioData.ManageWeb.csproj
dotnet build Baby.AudioData.InterfaceWeb\Baby.AudioData.InterfaceWeb.csproj
dotnet build Baby.AudioDataHosting\Baby.AudioDataHosting.csproj

# 运行
cd Baby.AudioData.ManageWeb && dotnet run
cd Baby.AudioData.InterfaceWeb && dotnet run
cd Baby.AudioDataHosting && dotnet run
```

## 关键注意事项

1. **不要修改 `.Generate.cs` 文件** - 它们会被代码生成器自动覆盖，自定义代码写在主文件中

2. **删除数据时必须清理缓存** - 先删除本地缓存，再请求跨服务器同步删除：
   ```csharp
   context.DeleteCacheEntity(id, providerName, db);
   context.DeleteDependencyKey(providerName, db);
   context.GetSyncDeleteCacheEntityKey(id, providerName, db).RequestSyncDeleteRedsKey();
   context.GetSyncDeleteDependencyKey(providerName, db).RequestSyncDeleteRedsKey();
   ```

3. **不要移除 `/health` 端点** - 所有应用都必须保留健康检查端点

4. **严格遵守中间件顺序** - LeoCore 框架要求特定的中间件顺序，详见 `application-config.md`

5. **后台管理必须使用 PowerFilter** - ManageWeb 控制器继承 PowerController，操作方法添加 `[PowerFilter]` 特性

6. **使用 IsNull() 检查实体** - 使用 LeoCore 扩展方法而非直接 null 判断

## 详细文档

详细主题文档位于 `.claude/rules/` 目录：

| 文件 | 说明 |
|------|------|
| `project-structure.md` | 项目结构和目录组织 |
| `dependencies.md` | LeoCore 框架和第三方依赖 |
| `architecture.md` | 分层架构和设计模式 |
| `database-access.md` | DataContext 模式和 CRUD 操作 |
| `cache-management.md` | Redis 缓存策略和同步机制 |
| `backend-management.md` | ManageWeb 后台管理开发 |
| `interface-development.md` | InterfaceWeb API 开发 |
| `hosting-service.md` | AudioDataHosting 后台服务 |
| `application-config.md` | 应用配置和中间件管道 |
| `code-conventions.md` | 编码规范和代码生成约定 |
