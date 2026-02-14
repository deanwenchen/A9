# InterfaceWeb Redis 播放统计实现计划

## TL;DR

> **快速总结**: 在 InterfaceWeb 项目中为音频播放数实现按日存储的 Redis Hash 操作，添加播放统计 API 和相关的服务类
> 
> **交付物**: 
> - `AudioDataProcess.Stats.cs` - 播放统计服务类
> - 扩展 `AudioInfoController.cs` - 添加播放统计 API
> - 扩展 `AudioDataProcess.cs` - 集成播放统计功能
> 
> **估计工作量**: 短时间任务
> **并行执行**: 否
> **关键路径**: 创建服务类 → 扩展控制器 → 集成现有代码

---

## Context

### 原始请求
在 InterfaceWeb 项目中增加音频的播放数 Redis 操作，需要针对每个音频按日存储，按 Hash 的方式实现。

### 项目分析
- **接口项目**: `Baby.AudioData.InterfaceWeb` (.NET 10.0)
- **控制器模式**: 继承自 `CoreController`，使用 `ClientResult` 返回类型
- **Redis 配置**: 通过 `AudioVariable` 类管理，使用 LeoCore 框架
- **现有 API**: 已有 `GetAudioInfo` 和 `GetAudioList` 方法

### 技术栈
- .NET 10.0
- LeoCore 框架
- Redis 分布式缓存
- MySQL 数据库

---

## Work Objectives

### Core Objective
实现音频播放数的按日存储 Redis Hash 操作，提供 API 接口用于记录和查询播放统计数据。

### Concrete Deliverables
- `AudioDataProcess.Stats.cs` - 播放统计服务类，处理 Redis Hash 操作
- 扩展 `AudioInfoController.cs` - 添加播放统计相关的 API 方法
- 扩展 `AudioDataProcess.cs` - 集成播放统计功能
- 完整的 Redis 键名设计和数据结构

### Definition of Done
- [x] 播放统计服务类创建完成
- [x] Redis Hash 键名设计合理，支持按日存储
- [x] AudioInfoController 添加播放统计 API
- [x] AudioDataProcess 集成播放统计功能
- [x] 包含错误处理和性能优化

### Must Have
- 按日期存储每个音频的播放次数
- 使用 Redis Hash 数据结构
- 集成现有的 LeoCore 框架
- 遵循项目现有的 API 模式
- 包含适当的错误处理

### Must NOT Have (Guardrails)
- 不直接使用 StackExchange.Redis 原生 API
- 不修改现有的 AudioInfo 实体结构
- 不创建与现有架构冲突的代码
- 不忽略性能考虑

---

## Verification Strategy (MANDATORY)

> **UNIVERSAL RULE: ZERO HUMAN INTERVENTION**

### Test Decision
- **Infrastructure exists**: YES
- **Automated tests**: Tests-after
- **Framework**: 单元测试使用 xUnit

### Agent-Executed QA Scenarios (MANDATORY)

```
Scenario: 播放统计 API 功能测试
  Tool: Bash (dotnet run)
  Preconditions: Redis 服务运行，项目构建成功
  Steps:
    1. cd Baby.AudioData.InterfaceWeb
    2. dotnet build
    3. 启动应用程序
    4. 调用播放统计 API 记录播放
    5. 调用查询 API 获取统计数据
    6. 验证 Redis 中的数据结构
  Expected Result: 播放统计正确记录和查询
  Evidence: API 响应和 Redis 数据验证

Scenario: 按日播放统计功能测试
  Tool: Bash (curl)
  Preconditions: 不同日期的播放数据
  Steps:
    1. 模拟不同日期的播放记录
    2. 查询特定日期的播放统计
    3. 查询日期范围的播放统计
    4. 验证数据按日期正确分组
  Expected Result: 按日统计功能正常
  Evidence: 日期分组数据验证

Scenario: 性能和并发测试
  Tool: Bash (ab)
  Preconditions: 大量并发请求
  Steps:
    1. 使用 ab 工具发送并发请求
    2. 监控 Redis 性能指标
    3. 验证数据一致性
    4. 测试高并发场景下的稳定性
  Expected Result: 高并发下系统稳定
  Evidence: 性能指标和数据一致性验证
```

---

## Execution Strategy

### Redis Hash 键名设计

基于 LeoCore 框架的模式和项目要求，设计如下键名结构：

```
# 每日播放统计 Hash
Baby.Audio:Hash:DailyPlay:20240204

# Hash 字段结构
AudioID_123 -> 45  (音频ID 123，播放45次)
AudioID_456 -> 32  (音频ID 456，播放32次)

# 月度统计 Hash（可选）
Baby.Audio:Hash:MonthlyPlay:202402

# 年度统计 Hash（可选）
Baby.Audio:Hash:YearlyPlay:2024
```

### Parallel Execution Waves

```
Wave 1 (Start Immediately):
├── Task 1: 创建 AudioDataProcess.Stats 播放统计服务

Wave 2 (After Wave 1):
├── Task 2: 扩展 AudioInfoController 添加播放统计 API
└── Task 3: 扩展 AudioDataProcess 集成播放统计

Wave 3 (After Wave 2):
├── Task 4: 添加批量操作和性能优化
└── Task 5: 创建使用文档和示例

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

- [x] 1. 创建 AudioDataProcess.Stats 播放统计服务

  **What to do**:
  - 在 Baby.AudioData.Core 项目中创建 AudioDataProcess.Stats 类
  - 实现按日播放统计的 Redis Hash 操作
  - 包含记录播放、查询统计、批量操作等方法
  - 使用 AudioVariable 配置和 LeoCore 框架

  **Must NOT do**:
  - 不要直接使用 StackExchange.Redis 原生 API
  - 不要忽略错误处理和性能考虑

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 业务服务类实现

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 2, Task 3
  - **Blocked By**: None

  **References**:
  **Pattern References**:
  - `Baby.AudioData.Core/AudioDataProcess.cs` - 现有业务逻辑类结构
  - `Baby.AudioData/AudioVariable.cs` - Redis 配置管理

  **API/Type References**:
  - `Leo.Data.Redis.RedisHelper.IncrementValueInHash` - 增加 Hash 字段值
  - `Leo.Data.Redis.RedisHelper.GetValueFromHash` - 获取 Hash 字段值
  - `Leo.Data.Redis.RedisHelper.GetAllEntriesFromHash` - 获取所有 Hash 条目

  **Acceptance Criteria**:
  - [x] 服务类创建在正确位置
  - [x] 实现记录播放方法
  - [x] 实现查询统计方法
  - [x] 实现批量操作方法
  - [x] 包含适当错误处理

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 播放统计服务基本功能测试
    Tool: Bash (dotnet build)
    Preconditions: Task 1 完成
    Steps:
      1. cd Baby.AudioData.Core
      2. dotnet build
      3. 创建简单测试程序验证服务方法
    Expected Result: 服务类编译通过，基本方法可用
    Evidence: 编译成功和测试输出
  ```

  **Commit**: NO

- [x] 2. 扩展 AudioInfoController 添加播放统计 API

  **What to do**:
  - 在 AudioInfoController 中添加播放统计相关 API
  - 包含记录播放和查询统计的接口
  - 遵循现有的 API 模式和返回格式
  - 集成 AudioDataProcess.Stats

  **Must NOT do**:
  - 不要偏离现有的 API 设计模式
  - 不要忽略参数验证和错误处理

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 控制器扩展实现

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 3)
  - **Blocks**: Task 4
  - **Blocked By**: Task 1

  **References**:
  **Pattern References**:
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:16-48` - 现有 API 模式
  - `Baby.AudioData.InterfaceWeb/Controllers/CoreController.cs` - 基类结构

  **API/Type References**:
  - `Leo.Mvc.Client.ClientResult` - API 返回类型
  - `Leo.Core.ClientStreamAnonymousTAsync` - 参数获取方法

  **Acceptance Criteria**:
  - [x] 添加 RecordPlay API 方法
  - [x] 添加 GetPlayStats API 方法
  - [x] 添加 GetDailyPlayStats API 方法
  - [x] 遵循现有 API 模式
  - [x] 包含参数验证

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 播放统计 API 功能测试
    Tool: Bash (curl)
    Preconditions: Task 1 和 Task 2 完成，应用运行
    Steps:
      1. 调用 RecordPlay API 记录播放
      2. 调用 GetPlayStats API 查询统计
      3. 验证返回数据格式正确
    Expected Result: API 正常工作，数据格式符合预期
    Evidence: API 响应验证
  ```

  **Commit**: NO

- [x] 3. 扩展 AudioDataProcess 集成播放统计

  **What to do**:
  - 在 AudioDataProcess 类中添加播放统计相关方法
  - 提供便捷的播放统计接口
  - 与现有的音频信息查询方法集成
  - 使用 AudioDataProcess.Stats 实现

  **Must NOT do**:
  - 不要修改现有方法的签名和行为
  - 不要创建重复的功能

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 业务逻辑扩展

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 2)
  - **Blocks**: Task 4
  - **Blocked By**: Task 1

  **References**:
  **Pattern References**:
  - `Baby.AudioData.Core/AudioDataProcess.cs:17-25` - 现有方法模式

  **API/Type References**:
  - `Baby.AudioData.Context.AudioInfoContext` - 音频数据上下文
  - `Baby.AudioData.Entity.AudioInfo` - 音频实体

  **Acceptance Criteria**:
  - [x] 添加 RecordAudioPlay 方法
  - [x] 添加 GetAudioPlayStats 方法
  - [x] 与现有方法保持一致的风格
  - [x] 包含适当注释

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: AudioDataProcess 扩展功能测试
    Tool: Bash (dotnet run)
    Preconditions: Task 1 和 Task 3 完成
    Steps:
      1. 创建测试程序调用扩展方法
      2. 验证播放统计功能正常
      3. 测试与现有方法的集成
    Expected Result: 扩展功能正常工作
    Evidence: 测试程序输出验证
  ```

  **Commit**: NO

- [x] 4. 添加批量操作和性能优化

  **What to do**:
  - 在 AudioDataProcess.Stats 中添加批量操作方法
  - 实现批量记录播放和批量查询统计
  - 添加缓存策略和性能优化
  - 包含数据聚合和清理功能

  **Must NOT do**:
  - 不要忽略线程安全考虑
  - 不要创建性能瓶颈

  **Recommended Agent Profile**:
  - **Category**: unspecified-low
  - **Skills**: []
  - **Reason**: 性能优化和高级功能

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 5
  - **Blocked By**: Task 2, Task 3

  **References**:
  **Pattern References**:
  - `Baby.AudioData.Core/AudioDataProcess.cs` - 现有性能考虑

  **API/Type References**:
  - `System.Collections.Generic.Dictionary<TKey,TValue>` - 批量数据结构
  - `System.Threading.Tasks.Task` - 异步操作支持

  **Acceptance Criteria**:
  - [x] 实现批量记录播放方法
  - [x] 实现批量查询统计方法
  - [x] 添加数据聚合功能
  - [x] 包含性能优化策略

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 批量操作和性能测试
    Tool: Bash (dotnet run)
    Preconditions: Task 4 完成
    Steps:
      1. 测试批量记录播放功能
      2. 测试批量查询统计功能
      3. 测试数据聚合功能
      4. 验证性能优化效果
    Expected Result: 批量操作高效，性能优化有效
    Evidence: 性能指标和功能验证
  ```

  **Commit**: NO

- [x] 5. 创建使用文档和示例

  **What to do**:
  - 创建播放统计功能的使用文档
  - 包含 API 接口说明和示例
  - 添加最佳实践和性能建议
  - 创建集成示例代码

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
  - `.claude/rules/cache-management.md` - 现有缓存文档

  **Documentation References**:
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs` - API 示例
  - 播放统计功能实现

  **Acceptance Criteria**:
  - [x] 创建完整的使用文档
  - [x] 包含所有 API 的示例
  - [x] 添加性能建议
  - [x] 包含集成示例

  **Agent-Executed QA Scenarios**:

  ```
  Scenario: 文档完整性验证
    Tool: Bash (find)
    Preconditions: Task 5 完成
    Steps:
      1. 检查文档文件存在
      2. 验证文档包含所有 API 示例
      3. 验证文档格式正确
    Expected Result: 文档完整且格式正确
    Evidence: 文档文件内容和结构验证
  ```

  **Commit**: NO

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 5 | `feat(interface): add audio play statistics with Redis Hash daily storage` | 多个文件 | `dotnet build` |

---

## Success Criteria

### Verification Commands
```bash
# 编译 InterfaceWeb 项目
cd Baby.AudioData.InterfaceWeb && dotnet build

# 编译 Core 项目
cd Baby.AudioData.Core && dotnet build

# 运行 API 测试
curl -X POST "http://localhost:5000/AudioInfo/RecordPlay" -d "AudioID=123"
curl -X GET "http://localhost:5000/AudioInfo/GetPlayStats?AudioID=123&Date=20240204"
```

### Final Checklist
- [x] AudioDataProcess.Stats 创建完成
- [x] AudioInfoController 扩展完成
- [x] AudioDataProcess 集成完成
- [x] 批量操作和性能优化完成
- [x] 文档创建完成
- [x] 所有功能测试通过

---

## 技术实现详细设计

### 1. AudioDataProcess.Stats 类结构

```csharp
namespace Baby.AudioData.Core
{
    public class AudioPlayStatsService
    {
        private readonly AudioInfoContext _audioInfoContext;
        
        // 构造函数
        public AudioPlayStatsService()
        {
            _audioInfoContext = new AudioInfoContext();
        }
        
        // 记录播放（按日）
        public void RecordDailyPlay(int audioId, DateTime? date = null)
        
        // 获取每日播放统计
        public int GetDailyPlayCount(int audioId, DateTime date)
        
        // 获取日期范围播放统计
        public Dictionary<DateTime, int> GetDateRangePlayStats(int audioId, DateTime startDate, DateTime endDate)
        
        // 批量记录播放
        public void BatchRecordDailyPlay(Dictionary<int, int> audioPlayCounts, DateTime? date = null)
        
        // 清理过期数据
        public void CleanupExpiredData(int daysToKeep = 90)
    }
}
```

### 2. AudioInfoController API 扩展

```csharp
// 记录播放 API
public async Task<ClientResult> RecordPlay()
{
    // 参数验证
    // 验证音频存在
    // 调用 AudioPlayStatsService.RecordDailyPlay
    // 返回结果
}

// 获取播放统计 API
public async Task<ClientResult> GetPlayStats()
{
    // 参数验证
    // 调用 AudioPlayStatsService 方法
    // 返回统计数据
}

// 获取每日播放统计 API
public async Task<ClientResult> GetDailyPlayStats()
{
    // 参数验证
    // 调用 AudioPlayStatsService 方法
    // 返回日期范围统计
}
```

### 3. Redis Hash 键名设计

```csharp
// 日期格式：yyyyMMdd
string dateKey = date.ToString("yyyyMMdd");

// Hash 键名：Baby.Audio:Hash:DailyPlay:20240204
string hashKey = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}";

// Hash 字段：AudioID_123
string fieldKey = $"AudioID_{audioId}";

// 使用 LeoCore 的 RedisHelper.IncrementValueInHash
RedisHelper.IncrementValueInHash(
    null,                   // 不使用 DataContext
    0,                      // 不需要实体ID
    fieldKey,               // 字段名
    fieldKey,               // 统计字段列表
    1,                      // 增量
    TimeSpan.FromDays(30),  // 过期时间
    AudioVariable.ProviderName,
    AudioVariable.Db
);
```

### 4. 性能优化策略

1. **批量操作**: 支持一次记录多个音频的播放数据
2. **缓存策略**: 热点数据使用内存缓存
3. **数据聚合**: 定期将日统计聚合为月度统计
4. **数据清理**: 自动清理过期的日统计数据
5. **异步处理**: 使用异步方法提高并发性能