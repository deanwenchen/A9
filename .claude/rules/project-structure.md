# Project Structure

## 项目概述

宝宝巴士音频数据管理系统，使用 ASP.NET Core 开发，包含多个项目类型的解决方案。

## 项目清单

| 项目 | 类型 | 目标框架 | 说明 |
|------|------|----------|------|
| `Baby.AudioData` | 类库 | .NET 6.0 | 数据实体和数据库上下文 |
| `Baby.AudioData.Core` | 类库 | .NET 6.0 | 核心业务逻辑层 |
| `Baby.AudioData.ManageWeb` | Web应用 | .NET 10.0 | 后台管理网站（MVC + Areas） |
| `Baby.AudioData.InterfaceWeb` | Web应用 | .NET 10.0 | 对外接口服务（API） |
| `Baby.AudioDataHosting` | 控制台应用 | .NET 10.0 | 后台服务/Worker |

## 目标框架说明

- **核心库（.NET 6.0）**：`Baby.AudioData` 和 `Baby.AudioData.Core`
- **应用层（.NET 10.0）**：`ManageWeb`、`InterfaceWeb`、`AudioDataHosting`

## 常用命令

### 构建所有项目

在项目根目录 `D:\宝宝巴士\A9` 执行：

```bash
# 构建核心库
dotnet build Baby.AudioData\Baby.AudioData.csproj
dotnet build Baby.AudioData.Core\Baby.AudioData.Core.csproj

# 构建应用层
dotnet build Baby.AudioData.ManageWeb\Baby.AudioData.ManageWeb.csproj
dotnet build Baby.AudioData.InterfaceWeb\Baby.AudioData.InterfaceWeb.csproj
dotnet build Baby.AudioDataHosting\Baby.AudioDataHosting.csproj
```

### 运行各个项目

```bash
# 运行后台管理网站
cd Baby.AudioData.ManageWeb && dotnet run

# 运行接口服务
cd Baby.AudioData.InterfaceWeb && dotnet run

# 运行后台服务
cd Baby.AudioDataHosting && dotnet run
```

### 发布部署

```bash
# 发布到指定目录
dotnet publish -c Release -o ./publish
```

## 解决方案目录结构

```
D:\宝宝巴士\A9\
├── Baby.AudioData\                    # 数据实体层 (.NET 6.0)
│   ├── Context\                       # 数据库上下文
│   │   ├── AlbumInfoContext.cs
│   │   ├── AlbumAudioContext.cs
│   │   └── AudioInfoContext.cs
│   └── Entities\                      # 实体类
│       ├── AlbumInfo.cs
│       ├── AlbumAudio.cs
│       └── AudioInfo.cs
│
├── Baby.AudioData.Core\               # 业务逻辑层 (.NET 6.0)
│   └── Process\                       # 业务处理类
│       └── AudioDataProcess.cs
│
├── Baby.AudioData.ManageWeb\          # 后台管理 (.NET 10.0)
│   ├── Areas\                         # MVC 区域
│   │   └── AudioDataManage\
│   │       ├── Controllers\           # 控制器
│   │       ├── Views\                 # 视图
│   │       ├── Data\                  # 数据访问（预留）
│   │       └── Models\                # 视图模型（预留）
│   ├── wwwroot\                       # 静态资源
│   └── appsettings.json               # 配置文件
│
├── Baby.AudioData.InterfaceWeb\       # 接口服务 (.NET 10.0)
│   ├── Controllers\                   # API 控制器
│   ├── wwwroot\                       # 静态资源
│   └── appsettings.json               # 配置文件
│
└── Baby.AudioDataHosting\             # 后台服务 (.NET 10.0)
    ├── Services\                      # 后台服务
    └── appsettings.json               # 配置文件
```

## 相关文档

- **[Dependencies](dependencies.md)** - LeoCore 框架和第三方依赖说明
- **[Architecture Overview](architecture.md)** - 分层架构和设计模式
- **[Backend Management](backend-management.md)** - ManageWeb 开发指南
- **[Interface Development](interface-development.md)** - InterfaceWeb API 开发
- **[Hosting Service](hosting-service.md)** - AudioDataHosting 后台服务
