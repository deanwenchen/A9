# Redis Hash 操作示例实现计划

## TL;DR

> **快速总结**: 创建一个完整的 Redis Hash 操作示例类，演示如何使用 LeoCore 框架进行音频统计数据的缓存管理
> 
> **交付物**: 
> - `RedisHashExample.cs` - 完整的 Redis Hash 操作类
> - 示例方法文档和使用说明
> 
> **估计工作量**: 短时间任务
> **并行执行**: 否
> **关键路径**: 创建文件 → 实现方法 → 测试验证

---

## Context

### 原始请求
用户需要一个操作 Redis Hash 的示例代码，基于现有的宝宝巴士音频数据管理系统。

### 项目分析
- 项目使用 LeoCore 框架，依赖 `Leo.Data.Redis` 库
- Redis 配置存储在外部文件 `C:\TestConfig\appconfig.json`
- 已有 `AudioVariable` 类定义 Redis 基本配置
- 项目结构采用分层架构，Core 层负责业务逻辑

### 技术栈
- .NET 6.0/10.0
- LeoCore 框架
- Redis 分布式缓存
- MySQL 数据库

---

## Work Objectives

### Core Objective
创建一个完整的 Redis Hash 操作示例，展示如何使用 LeoCore 框架进行音频数据的统计信息缓存管理。

### Concrete Deliverables
- `RedisHashExample.cs` - 包含各种 Redis Hash 操作方法的完整类
- 基本的 CRUD 操作示例
- 实际应用场景的代码示例
- 性能优化的批量操作示例

### Definition of Done
- [ ] 代码文件创建完成
- [ ] 包含基本的 Hash 操作方法
- [ ] 包含实际应用场景示例
- [ ] 包含异步操作示例
- [ ] 代码符合项目编码规范

### Must Have
- 使用 LeoCore 框架的 RedisHelper 类
- 遵循项目的配置规范（AudioVariable）
- 包含错误处理机制
- 包含实际业务场景示例

### Must NOT Have (Guardrails)
- 不直接使用 StackExchange.Redis 原生 API
- 不硬编码 Redis 连接字符串
- 不创建与现有架构冲突的代码
- 不依赖第三方 Redis 客户端库

---

## Verification Strategy (MANDATORY)

> **UNIVERSAL RULE: ZERO HUMAN INTERVENTION**

### Test Decision
- **Infrastructure exists**: YES
- **Automated tests**: Tests-after
- **Framework**: 单元测试使用 xUnit

### Agent-Executed QA Scenarios (MANDATORY)

```
Scenario: 基本Hash操作设置和获取
  Tool: Bash (dotnet run)
  Preconditions: Redis 服务运行，项目构建成功
  Steps:
    1. cd Baby.AudioData.Core
    2. dotnet build
    3. 创建测试程序验证 RedisHashExample 方法
    4. 运行测试程序验证 Hash 操作
  Expected Result: Hash 数据正确设置和获取
  Evidence: 控制台输出显示操作结果

Scenario: 音频统计信息更新功能
  Tool: Bash (dotnet run)
  Preconditions: Redis 连接正常
  Steps:
    1. 实例化 RedisHashExample
    2. 调用 UpdateAudioPlayStats 方法
    3. 验证统计数据是否正确更新
    4. 检查 Redis 中的数据结构
  Expected Result: 播放统计正确更新
  Evidence: Redis 数据验证输出

Scenario: 批量操作性能测试
  Tool: Bash (dotnet run)
  Preconditions: 大量测试数据准备
  Steps:
    1. 准备 1000 个音频ID的测试数据
    2. 调用 BatchUpdateAudioStats 方法
    3. 测量操作耗时
    4. 验证批量操作的正确性
  Expected Result: 批量操作高效完成
  Evidence: 性能指标和正确性验证
```

---

## Execution Strategy

### Parallel Execution Waves

```
Wave 1 (Start Immediately):
├── Task 1: 创建 RedisHashExample.cs 文件

Wave 2 (After Wave 1):
├── Task 2: 实现基本 Hash 操作方法
├── Task 3: 实现实际应用场景方法

Wave 3 (After Wave 2):
├── Task 4: 实现批量操作和异步方法
└── Task 5: 创建示例使用文档

Critical Path: Task 1 → Task 2 → Task 3 → Task 4 → Task 5
```

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|------|------------|--------|---------------------|
| 1 | None | 2, 3 | None |
| 2 | 1 | 4 | 3 |
| 3 | 1 | 4 | 2 |
| 4 | 2, 3 | 5 | None |
| 5 | 4 | None | None |

---

## TODOs

- [ ] 1. 创建 RedisHashExample.cs 文件结构

  **What to do**:
  - 在 Baby.AudioData.Core 项目中创建 RedisHashExample.cs
  - 添加必要的 using 语句
  - 定义类的基本结构

  **Must NOT do**:
  - 不要使用 StackExchange.Redis 原生 API
  - 不要硬编码配置信息

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 简单的代码文件创建任务

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 2, Task 3
  - **Blocked By**: None

  **References**:
  **Pattern References**:
  - `Baby.AudioData.Core/AudioDataProcess.cs` - 现有的业务逻辑类结构
  - `Baby.AudioData/AudioVariable.cs` - Redis 配置变量

  **API/Type References**:
  - `Leo.Data.Redis.RedisHelper` - Redis 操作核心类
  - `Baby.AudioData.Context.AudioInfoContext` - 音频数据上下文
  - `Baby.AudioData.Context.AlbumInfoContext` - 专辑数据上下文

  **Acceptance Criteria**:
  - [ ] 文件创建在正确位置
  - [ ] 包含必要的 using 语句
  - [ ] 类定义正确，继承关系合适

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 文件结构验证
    Tool: Bash (ls)
    Preconditions: 进入项目目录
    Steps:
      1. ls Baby.AudioData.Core/RedisHashExample.cs
      2. grep -n "using Leo.Data.Redis" Baby.AudioData.Core/RedisHashExample.cs
      3. grep -n "public class RedisHashExample" Baby.AudioData.Core/RedisHashExample.cs
    Expected Result: 文件存在并包含基本结构
    Evidence: 文件路径和内容验证
  ```

  **Commit**: NO

- [ ] 2. 实现基本 Hash 操作方法

  **What to do**:
  - 实现设置和获取 Hash 数据的基本方法
  - 包含错误处理机制
  - 使用 AudioVariable 配置

  **Must NOT do**:
  - 不要忽略异常处理
  - 不要使用非 LeoCore 的 Redis 操作方式

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 基本的 Redis Hash 操作实现

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 3)
  - **Blocks**: Task 4
  - **Blocked By**: Task 1

  **References**:
  **Pattern References**:
  - `Baby.AudioData.Core/AudioDataProcess.cs:17-25` - 使用 GetCacheEntity 的模式

  **API/Type References**:
  - `Leo.Data.Redis.RedisHelper.Set` - 设置 Redis 键值
  - `Leo.Data.Redis.RedisHelper.Get` - 获取 Redis 键值
  - `Baby.AudioData.AudioVariable.ProviderName` - Redis 提供者名称
  - `Baby.AudioData.AudioVariable.Db` - Redis 数据库索引

  **Acceptance Criteria**:
  - [ ] 实现设置 Hash 数据方法
  - [ ] 实现获取 Hash 数据方法
  - [ ] 包含适当的错误处理
  - [ ] 使用项目标准配置

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 基本 Hash 操作功能测试
    Tool: Bash (dotnet run)
    Preconditions: Redis 服务运行，Task 1 完成
    Steps:
      1. 编译 Baby.AudioData.Core 项目
      2. 创建简单测试程序调用基本方法
      3. 验证数据设置和获取功能
    Expected Result: 基本 Hash 操作正常工作
    Evidence: 测试程序输出和 Redis 数据验证
  ```

  **Commit**: NO

- [ ] 3. 实现实际应用场景方法

  **What to do**:
  - 实现音频播放统计更新方法
  - 实现用户点赞功能
  - 实现用户播放历史记录

  **Must NOT do**:
  - 不要创建与业务逻辑冲突的场景
  - 不要忽略性能考虑

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 业务逻辑方法的实现

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 2)
  - **Blocks**: Task 4
  - **Blocked By**: Task 1

  **References**:
  **Pattern References**:
  - `Baby.AudioData.Entity/AudioInfo.cs` - 音频实体结构
  - `Baby.AudioData.Entity/AlbumInfo.cs` - 专辑实体结构

  **API/Type References**:
  - `Leo.Data.Redis.RedisHelper.IncrementValueInHash` - 增加 Hash 字段值
  - `Leo.Data.Redis.RedisHelper.GetAllEntriesFromHash` - 获取所有 Hash 条目
  - `Leo.Data.Redis.RedisHelper.GetValueFromHash` - 获取特定 Hash 值

  **Acceptance Criteria**:
  - [ ] 实现音频播放统计更新
  - [ ] 实现用户点赞/取消点赞
  - [ ] 实现用户播放历史记录
  - [ ] 所有方法包含适当注释

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 业务场景功能测试
    Tool: Bash (dotnet run)
    Preconditions: Task 2 完成，测试数据准备
    Steps:
      1. 模拟音频播放场景
      2. 测试用户点赞功能
      3. 验证播放历史记录
    Expected Result: 所有业务场景功能正常
    Evidence: 业务逻辑验证和 Redis 数据检查
  ```

  **Commit**: NO

- [ ] 4. 实现批量操作和异步方法

  **What to do**:
  - 实现批量更新统计信息方法
  - 实现异步操作方法
  - 添加性能优化考虑

  **Must NOT do**:
  - 不要忽略线程安全
  - 不要创建性能瓶颈

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 高级功能的实现

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 5
  - **Blocked By**: Task 2, Task 3

  **References**:
  **Pattern References**:
  - `Baby.AudioData.Core/AudioDataProcess.cs` - 现有方法的性能考虑

  **API/Type References**:
  - `System.Threading.Tasks.Task` - 异步操作支持
  - `System.Collections.Generic.Dictionary<TKey,TValue>` - 批量数据结构

  **Acceptance Criteria**:
  - [ ] 实现批量更新方法
  - [ ] 实现异步操作方法
  - [ ] 包含性能优化
  - [ ] 添加适当注释

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 批量操作和异步功能测试
    Tool: Bash (dotnet run)
    Preconditions: Task 3 完成，大量测试数据
    Steps:
      1. 测试批量更新功能
      2. 测试异步操作性能
      3. 比较同步和异步操作效率
    Expected Result: 批量操作高效，异步功能正常
    Evidence: 性能指标和功能验证
  ```

  **Commit**: NO

- [ ] 5. 创建示例使用文档

  **What to do**:
  - 创建使用示例文档
  - 添加方法说明和最佳实践
  - 包含性能建议

  **Must NOT do**:
  - 不要创建过于复杂的示例
  - 不要忽略错误处理示例

  **Recommended Agent Profile**:
  - **Category**: writing
  - **Skills**: []
  - **Reason**: 文档编写任务

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: None
  - **Blocked By**: Task 4

  **References**:
  **Pattern References**:
  - `.claude/rules/cache-management.md` - 现有的缓存管理文档

  **Documentation References**:
  - `Baby.AudioData/AudioVariable.cs` - 配置参考
  - Redis Hash 操作最佳实践

  **Acceptance Criteria**:
  - [ ] 创建完整的使用文档
  - [ ] 包含所有方法的示例
  - [ ] 添加性能建议
  - [ ] 包含错误处理示例

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 文档完整性验证
    Tool: Bash (find)
    Preconditions: Task 4 完成
    Steps:
      1. 检查文档文件存在
      2. 验证文档包含所有方法示例
      3. 验证文档格式正确
    Expected Result: 文档完整且格式正确
    Evidence: 文档文件内容和结构验证
  ```

  **Commit**: NO

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 5 | `feat(core): add Redis Hash operations example` | `Baby.AudioData.Core/RedisHashExample.cs` | `dotnet build` |

---

## Success Criteria

### Verification Commands
```bash
# 编译验证
cd Baby.AudioData.Core && dotnet build

# 运行测试（如果添加了测试）
dotnet test

# 检查 Redis 连接
dotnet run --project TestProject
```

### Final Checklist
- [ ] 所有 Hash 操作方法实现完成
- [ ] 包含实际应用场景示例
- [ ] 包含异步和批量操作
- [ ] 代码符合项目规范
- [ ] 包含适当错误处理
- [ ] 文档完整准确

---

## Redis Hash 操作示例代码大纲

```csharp
using Baby.AudioData.Context;
using Baby.AudioData.Entity;
using Leo.Data.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baby.AudioData.Core
{
    public class RedisHashExample
    {
        // 基本字段
        private readonly AudioInfoContext _audioInfoContext;
        private readonly AlbumInfoContext _albumInfoContext;

        // 构造函数
        public RedisHashExample() { }

        #region 基本 Hash 操作
        public void SetAudioStatsToHash(int audioId, int viewCount, int likeCount, int commentCount);
        public (int ViewCount, int LikeCount, int CommentCount) GetAudioStatsFromHash(int audioId);
        public void IncrementHashField(int audioId, string field, int increment = 1);
        #endregion

        #region 使用 LeoCore 的高级 Hash 功能
        public void SetAudioStatsUsingLeoCoreHash(int audioId, TimeSpan? expireIn = null);
        public Dictionary<string, int> GetAllAudioStats(int audioId, TimeSpan? expireIn = null);
        public int GetSpecificStat(int audioId, string fieldName, TimeSpan? expireIn = null);
        #endregion

        #region 实际应用场景示例
        public void UpdateAudioPlayStats(int audioId, int userId);
        public void ToggleAudioLike(int audioId, int userId, bool isLike);
        public List<int> GetUserPlayHistory(int userId, int limit = 50);
        #endregion

        #region 批量操作和性能优化
        public void BatchUpdateAudioStats(Dictionary<int, (int viewCount, int likeCount, int commentCount)> audioStats);
        public void CleanupExpiredHashData(int audioId);
        #endregion

        #region 异步操作示例
        public Task SetAudioStatsToHashAsync(int audioId, int viewCount, int likeCount, int commentCount);
        public Task<(int ViewCount, int LikeCount, int CommentCount)> GetAudioStatsFromHashAsync(int audioId);
        #endregion
    }
}
```