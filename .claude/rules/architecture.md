# Architecture Overview

## 分层架构

项目采用经典的多层架构设计：

```
┌─────────────────────────────────────────┐
│   Controllers (控制器层)                 │
│   - ManageWeb: Areas + PowerController   │
│   - InterfaceWeb: API Controllers        │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Core (业务逻辑层)                      │
│   - AudioDataProcess                     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Context (数据访问层)                   │
│   - AlbumInfoContext                     │
│   - AlbumAudioContext                    │
│   - AudioInfoContext                     │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   Entity (实体层)                        │
│   - AlbumInfo, AlbumAudio, AudioInfo     │
└─────────────────────────────────────────┘
```

## 各层职责

### Controllers 层（控制器层）

**位置**：`Baby.AudioData.ManageWeb` 和 `Baby.AudioData.InterfaceWeb`

**职责**：
- 处理 HTTP 请求和响应
- 路由和参数解析
- 调用业务逻辑层
- 返回视图（ManageWeb）或 JSON（InterfaceWeb）
- 权限验证和操作日志记录

**特点**：
- ManageWeb：使用 MVC Areas 模式，继承 `PowerController`
- InterfaceWeb：传统 MVC 路由，返回 JSON API

### Core 层（业务逻辑层）

**位置**：`Baby.AudioData.Core`

**职责**：
- 实现业务规则和逻辑
- 协调多个 Context 操作
- 处理复杂的数据转换
- 提供 Service 接口

**示例**：`AudioDataProcess` 类处理音频相关业务逻辑

### Context 层（数据访问层）

**位置**：`Baby.AudioData`

**职责**：
- 封装数据库操作
- 提供 CRUD 接口
- 缓存管理
- 事务处理

**基类**：继承自 `DataContext<TEntity, TKey>`，提供标准数据操作

**主要 Context**：
- `AlbumInfoContext` - 专辑信息
- `AlbumAudioContext` - 专辑音频关联
- `AudioInfoContext` - 音频信息

### Entity 层（实体层）

**位置**：`Baby.AudioData`

**职责**：
- 定义数据模型
- 对应数据库表结构
- 数据注解和验证规则

**主要实体**：
- `AlbumInfo` - 专辑信息
- `AlbumAudio` - 专辑音频关联
- `AudioInfo` - 音频信息

## 项目间依赖关系

```
┌──────────────────────────────────────────┐
│  AudioDataHosting (后台服务)              │
│  - 定时任务                              │
│  - 云存储处理                            │
└──────────────┬───────────────────────────┘
               │
┌──────────────┴───────────────────────────┐
│  ManageWeb / InterfaceWeb (应用层)       │
│  - HTTP 请求处理                         │
│  - 权限控制                              │
└──────────────┬───────────────────────────┘
               │
┌──────────────┴───────────────────────────┐
│  AudioData.Core (业务逻辑层)             │
│  - 业务规则                              │
│  - 数据处理                              │
└──────────────┬───────────────────────────┘
               │
┌──────────────┴───────────────────────────┐
│  AudioData (数据访问层)                  │
│  - DataContext                          │
│  - Entity 定义                          │
└──────────────────────────────────────────┘
```

**依赖方向**：从上到下，上层依赖下层

## 设计模式和约定

### DataContext 模式

所有数据库上下文继承 `DataContext<TEntity, TKey>`，提供统一的 CRUD 操作。

详见：[Database Access](database-access.md)

### Areas 模式（ManageWeb）

后台管理使用 MVC Areas 组织功能模块，每个业务模块独立 Area。

详见：[Backend Management](backend-management.md)

### 缓存策略

使用 Redis 分布式缓存，支持依赖键和跨服务器同步。

详见：[Cache Management](cache-management.md)

### 代码生成

部分类使用代码生成器生成 `.Generate.cs` 文件。

详见：[Code Conventions](code-conventions.md)

## 关键概念

### 目标框架混用

- **核心库 (.NET 6.0)**：`AudioData`、`AudioData.Core`
- **应用层 (.NET 10.0)**：`ManageWeb`、`InterfaceWeb`、`AudioDataHosting`

### 外部框架依赖

严重依赖 LeoCore 框架，通过 DLL 引用（`../../LeoCore/`）。

详见：[Dependencies](dependencies.md)

### 分层职责

- **Controller**：薄层，仅处理 HTTP 和调用 Core
- **Core**：业务逻辑核心，可复用
- **Context**：数据访问封装，不包含业务逻辑
- **Entity**：纯数据模型

## 相关文档

- **[Database Access](database-access.md)** - DataContext 模式和数据操作
- **[Cache Management](cache-management.md)** - Redis 缓存策略
- **[Backend Management](backend-management.md)** - ManageWeb 开发模式
- **[Interface Development](interface-development.md)** - InterfaceWeb API 开发
- **[Code Conventions](code-conventions.md)** - 代码生成和规范
