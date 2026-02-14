# InterfaceWeb Redis API 实现计划

## TL;DR

> **Quick Summary**: 在InterfaceWeb项目中创建RedisController，提供全面的Redis操作API，支持String/Hash/Set/ZSet/List数据结构、键管理、缓存管理功能
>
> **Deliverables**:
> - RedisController.cs（包含所有Redis操作API）
> - Startup.cs配置更新（Redis连接注册）
> - 项目文件更新（StackExchange.Redis包引用）
>
> **Estimated Effort**: Medium
> **Parallel Execution**: YES - 2 waves
> **Critical Path**: Startup配置 → RedisController实现

---

## Context

### Original Request
在接口层增加访问redis的多种方法使用，提供全面的Redis操作API

### Interview Summary
**Key Discussions**:
- 功能范围确认：所有Redis数据结构操作（String, Hash, Set, Sorted Set, List）
- 键管理功能：键查询、键遍历、键删除、键重命名/移动
- 缓存管理功能：缓存状态查询、缓存清除、缓存刷新、缓存统计
- 实现方式：直接操作Redis（不通过DataContext封装）
- 安全控制：完全开放，无权限限制

**Research Findings**:
- 项目使用LeoCore框架，底层基于StackExchange.Redis
- 现有接口模式：继承CoreController，使用InvokeResult响应格式
- 特性使用：[ExceptionFilter], [ClientHeaderFilter]
- 需要在Startup.cs中注册Redis连接

### Metis Review
**Identified Gaps** (addressed):
- 安全性：完全开放的API存在风险，添加警告和最佳实践说明
- 键遍历性能：Keys操作在生产环境可能阻塞，推荐使用Scan
- 错误处理：需要统一的Redis异常处理
- 连接字符串：需要从配置读取，避免硬编码
- 事务和批处理：未包含在需求中，但在文档中说明扩展方向

---

## Work Objectives

### Core Objective
在InterfaceWeb项目中创建RedisController，提供全面的Redis操作API，支持五种数据结构和键管理、缓存管理功能。

### Concrete Deliverables
- `Baby.AudioData.InterfaceWeb\Controllers\RedisController.cs` - 主控制器文件
- `Baby.AudioData.InterfaceWeb\Startup.cs` - 添加Redis连接配置
- `Baby.AudioData.InterfaceWeb\Baby.AudioData.InterfaceWeb.csproj` - 添加NuGet包引用

### Definition of Done
- [ ] 所有数据结构操作API实现并测试通过
- [ ] 键管理功能实现并测试通过
- [ ] 缓存管理功能实现并测试通过
- [ ] API响应格式统一使用InvokeResult
- [ ] 错误处理覆盖所有Redis操作
- [ ] 配置正确且可运行

### Must Have
- 支持String/Hash/Set/ZSet/List所有常用操作
- 键管理功能完整
- 缓存管理功能完整
- 完全开放的访问方式（无权限控制）
- 统一的响应格式（InvokeResult）

### Must NOT Have (Guardrails)
- 不使用PowerFilter权限控制
- 不修改.DataContext.Generate.cs文件
- 不破坏现有的缓存同步机制（如果使用Leo.Data.Redis）
- 不在生产环境使用Keys命令（改用Scan）

---

## Verification Strategy (MANDATORY)

> **UNIVERSAL RULE: ZERO HUMAN INTERVENTION**
>
> ALL tasks in this plan MUST be verifiable WITHOUT any human action.
> This is NOT conditional — it applies to EVERY task, regardless of test strategy.
>
> **FORBIDDEN** — acceptance criteria that require:
> - "User manually tests..." / "사용자가 직접 테스트..."
> - "User visually confirms..." / "사용자가 눈으로 확인..."
> - "User interacts with..." / "사용자가 직접 조작..."
> - "Ask user to verify..." / "사용자에게 확인 요청..."
> - ANY step where a human must perform an action
>
> **ALL verification is executed by the agent** using tools (curl, Bash, etc.). No exceptions.

### Test Decision
- **Infrastructure exists**: NO
- **Automated tests**: NO (Tests-after - API level testing with curl)
- **Framework**: None (API level testing only)

### Agent-Executed QA Scenarios (MANDATORY — ALL tasks)

**Whether TDD is enabled or not, EVERY task MUST include Agent-Executed QA Scenarios.**

These describe how the executing agent DIRECTLY verifies the deliverable by running it — sending API requests with curl.

**Each Scenario MUST Follow This Format:**

```
Scenario: [Descriptive name]
  Tool: Bash (curl)
  Preconditions: [What must be true before this scenario runs]
  Steps:
    1. [Exact curl command with endpoint and parameters]
    2. [Next action with expected intermediate state]
    3. [Assertion with exact expected value]
  Expected Result: [Concrete, observable outcome]
  Failure Indicators: [What would indicate failure]
  Evidence: [Response body path / output capture]
```

**Scenario Detail Requirements:**
- **Endpoints**: Specific API paths (`/api/redis/string/set`)
- **Data**: Concrete test data (`{"key":"test","value":"hello"}`)
- **Assertions**: Exact values (`ResultCode equals "Success"`)
- **Evidence**: Specific file paths (`.sisyphus/evidence/task-N-scenario-name.json`)

---

## Execution Strategy

### Parallel Execution Waves

```
Wave 1 (Start Immediately):
├── Task 1: 添加NuGet包引用
├── Task 2: 配置Startup.cs（Redis连接）
└── Task 3: 创建RedisController基础结构

Wave 2 (After Wave 1):
├── Task 4: 实现String操作
├── Task 5: 实现Hash操作
├── Task 6: 实现Set操作
├── Task 7: 实现ZSet操作
└── Task 8: 实现List操作

Wave 3 (After Wave 2):
├── Task 9: 实现键管理
├── Task 10: 实现缓存管理
└── Task 11: API测试验证

Critical Path: Task 1 → Task 2 → Task 3 → Task 4~8 → Task 9~10 → Task 11
Parallel Speedup: ~60% faster than sequential
```

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|------|------------|--------|---------------------|
| 1 | None | 2 | 2 (as parallel setup) |
| 2 | 1 | 3 | None |
| 3 | 2 | 4,5,6,7,8 | None |
| 4 | 3 | 9 | 5,6,7,8 |
| 5 | 3 | 9 | 4,6,7,8 |
| 6 | 3 | 9 | 4,5,7,8 |
| 7 | 3 | 9 | 4,5,6,8 |
| 8 | 3 | 9 | 4,5,6,7 |
| 9 | 4,5,6,7,8 | 10,11 | 10 |
| 10 | 9 | 11 | None |
| 11 | 9,10 | None | None |

---

## TODOs

- [ ] 1. 添加StackExchange.Redis NuGet包引用

  **What to do**:
  - 在Baby.AudioData.InterfaceWeb.csproj中添加StackExchange.Redis包引用
  - 版本：使用最新稳定版（2.8.x）
  - 验证包引用正确

  **Must NOT do**:
  - 不要修改其他依赖项
  - 不要添加不相关的包

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: Simple package reference addition, straightforward file edit
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: C# project file editing, NuGet package management

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1 (with Tasks 2, 3)
  - **Blocks**: Task 2
  - **Blocked By**: None (can start immediately)

  **References** (CRITICAL - Be Exhaustive):

  **Pattern References** (existing code to follow):
  - `Baby.AudioData.InterfaceWeb\Baby.AudioData.InterfaceWeb.csproj:10-19` - Existing PackageReference examples (Newtonsoft.Json, MySql.Data)

  **API/Type References** (contracts to implement against):
  - None - NuGet package addition

  **Documentation References** (specs and requirements):
  - `https://www.nuget.org/packages/StackExchange.Redis` - Latest version and usage documentation

  **External References** (libraries and frameworks):
  - NuGet official: `https://www.nuget.org/packages/StackExchange.Redis` - Package version and installation instructions

  **WHY Each Reference Matters** (explain the relevance):
  - csproj file: Shows the correct PackageReference syntax and formatting
  - NuGet page: Provides the latest stable version number to use

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] PackageReference added: `<PackageReference Include="StackExchange.Redis" Version="[latest-version]" />`
  - [ ] dotnet restore completes successfully: `cd Baby.AudioData.InterfaceWeb && dotnet restore`
  - [ ] Build succeeds: `dotnet build Baby.AudioData.InterfaceWeb/Baby.AudioData.InterfaceWeb.csproj`

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Verify package reference added successfully
    Tool: Bash (dotnet)
    Preconditions: Working directory is D:\宝宝巴士\A9
    Steps:
      1. dotnet restore Baby.AudioData.InterfaceWeb/Baby.AudioData.InterfaceWeb.csproj
      2. Assert: Command exits with code 0
      3. dotnet build Baby.AudioData.InterfaceWeb/Baby.AudioData.InterfaceWeb.csproj
      4. Assert: Command exits with code 0
      5. Assert: Output contains "Build succeeded" or "Build FAILED" check expected
    Expected Result: Project restores and builds successfully with StackExchange.Redis package
    Evidence: Build output captured to .sisyphus/evidence/task-1-build-output.txt

  **Commit**: YES
  - Message: `feat(deps): add StackExchange.Redis NuGet package`
  - Files: `Baby.AudioData.InterfaceWeb/Baby.AudioData.InterfaceWeb.csproj`
  - Pre-commit: `dotnet restore && dotnet build`

---

- [ ] 2. 配置Startup.cs（注册Redis连接）

  **What to do**:
  - 在Startup.cs的ConfigureServices方法中注册ConnectionMultiplexer
  - 从配置读取Redis连接字符串
  - 添加IDatabase服务注册（可选，便于注入）
  - 错误处理：连接失败时的处理

  **Must NOT do**:
  - 不要修改其他现有服务配置
  - 不要删除健康检查端点
  - 不要破坏现有的中间件顺序

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: Dependency injection configuration, straightforward addition
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: ASP.NET Core dependency injection, configuration management

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 3
  - **Blocked By**: Task 1

  **References** (CRITICAL - Be Exhaustive):

  **Pattern References** (existing code to follow):
  - `Baby.AudioData.InterfaceWeb/Startup.cs:25-46` - Existing ConfigureServices method structure
  - `Baby.AudioData.InterfaceWeb/Startup.cs:28-31` - AddHttpClient() service registration example
  - `Baby.AudioData.InterfaceWeb/Startup.cs:36-37` - AddHttpContextAccessor() example

  **API/Type References** (contracts to implement against):
  - `StackExchange.Redis.ConnectionMultiplexer` - Redis connection class
  - `StackExchange.Redis.IConnectionMultiplexer` - Interface for dependency injection
  - `StackExchange.Redis.IDatabase` - Database interface

  **Documentation References** (specs and requirements):
  - `https://stackexchange.github.io/StackExchange.Redis/Configuration.html` - Configuration options
  - `Baby.AudioData/AudioVariable.cs:12-21` - Existing Redis configuration (ProviderName, Db)

  **External References** (libraries and frameworks):
  - StackExchange.Redis docs: `https://stackexchange.github.io/StackExchange.Redis/Basics.html` - Basic usage and DI patterns

  **WHY Each Reference Matters** (explain the relevance):
  - Startup.cs: Shows the correct location and pattern for service registration
  - AudioVariable.cs: Contains existing Redis configuration that can be reused
  - StackExchange docs: Provides DI registration examples

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] ConnectionMultiplexer registered as singleton in ConfigureServices
  - [ ] Connection string read from Configuration (not hardcoded)
  - [ ] Optional: IDatabase registered as scoped service
  - [ ] Application starts without errors: `dotnet run` (wait for startup)

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Application starts successfully with Redis connection
    Tool: Bash (dotnet)
    Preconditions: Task 1 completed, working directory is D:\宝宝巴士\A9
    Steps:
      1. cd Baby.AudioData.InterfaceWeb
      2. dotnet run --urls "http://localhost:5001" &
      3. Wait 10 seconds for startup
      4. curl -s http://localhost:5001/health
      5. Assert: HTTP status code is 200
      6. Assert: Response contains "Healthy" or similar health check response
      7. Kill the dotnet process
    Expected Result: Application starts without errors, health check returns 200
    Evidence: Startup log and health check response captured

  **Commit**: YES
  - Message: `feat(config): register Redis connection in Startup`
  - Files: `Baby.AudioData.InterfaceWeb/Startup.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 3. 创建RedisController基础结构

  **What to do**:
  - 创建RedisController.cs，继承CoreController
  - 添加[ExceptionFilter]和[ClientHeaderFilter]特性
  - 注入IConnectionMultiplexer和IDatabase
  - 实现GetDatabase()辅助方法
  - 创建统一的响应包装方法

  **Must NOT do**:
  - 不要实现具体的Redis操作方法（由后续任务实现）
  - 不要移除或修改CoreController的现有功能

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: Controller scaffolding, basic structure setup
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: ASP.NET Core controller creation, dependency injection

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Tasks 4, 5, 6, 7, 8
  - **Blocked By**: Task 2

  **References** (CRITICAL - Be Exhaustive):

  **Pattern References** (existing code to follow):
  - `Baby.AudioData.InterfaceWeb/Controllers/CoreController.cs:14` - CoreController base class
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:12-16` - Controller with filters and DI example
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:19-26` - InvokeResult usage pattern

  **API/Type References** (contracts to implement against):
  - `Leo.Mvc.Client.ClientProductController` - Base controller class
  - `Leo.Mvc.Client.InvokeResult` - Response result type
  - `Leo.Mvc.Client.ClientResult` - Return type for async methods

  **Documentation References** (specs and requirements):
  - `.claude/rules/interface-development.md` - InterfaceWeb development guidelines
  - `.claude/rules/backend-management.md` - Controller structure patterns

  **External References** (libraries and frameworks):
  - StackExchange.Redis: `https://stackexchange.github.io/StackExchange.Redis/Basics.html` - IDatabase usage

  **WHY Each Reference Matters** (explain the relevance):
  - CoreController: Shows the correct base class and inheritance pattern
  - AudioInfoController: Demonstrates correct filter usage and DI pattern
  - InvokeResult: Shows the response format to maintain consistency

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] RedisController.cs created in Controllers folder
  - [ ] Inherits from CoreController
  - [ ] Has [ExceptionFilter] and [ClientHeaderFilter] attributes
  - [ ] Constructor injects IConnectionMultiplexer
  - [ ] Has protected IDatabase property for Redis access
  - [ ] Builds successfully: `dotnet build`

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Controller structure is correct
    Tool: Bash (dotnet + grep)
    Preconditions: Tasks 1-2 completed
    Steps:
      1. dotnet build Baby.AudioData.InterfaceWeb/Baby.AudioData.InterfaceWeb.csproj
      2. Assert: Command exits with code 0
      3. grep -n "class RedisController" Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs
      4. Assert: Line found
      5. grep -n "CoreController" Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs
      6. Assert: Line found (inherits from CoreController)
      7. grep -n "IConnectionMultiplexer" Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs
      8. Assert: Line found (constructor parameter)
    Expected Result: Controller file exists with correct structure
    Evidence: Build output and grep results captured

  **Commit**: YES (groups with 4)
  - Message: `feat(redis): add RedisController base structure`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 4. 实现String操作

  **What to do**:
  - 实现GET /api/redis/string/get - 获取字符串值
  - 实现POST /api/redis/string/set - 设置字符串值
  - 实现POST /api/redis/string/delete - 删除字符串
  - 实现POST /api/redis/string/increment - 增加数值
  - 实现POST /api/redis/string/decrement - 减少数值
  - 参数验证：key不能为空
  - 返回统一格式的InvokeResult

  **Must NOT do**:
  - 不要实现其他数据结构操作（由其他任务实现）

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: CRUD operations for single data type, straightforward implementation
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: Redis IDatabase operations, async/await patterns

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 5, 6, 7, 8)
  - **Blocks**: Task 9
  - **Blocked By**: Task 3

  **References** (CRITICAL - Be Exhaustive):

  **Pattern References** (existing code to follow):
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:17-48` - Async action method pattern
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:19-20` - ClientStreamAnonymousTAsync parameter binding example
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:20-26` - InvokeResult error response pattern
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:38-48` - InvokeResult success response pattern

  **API/Type References** (contracts to implement against):
  - `StackExchange.Redis.IDatabase.StringGetAsync` - Get string value
  - `StackExchange.Redis.IDatabase.StringSetAsync` - Set string value
  - `StackExchange.Redis.IDatabase.StringIncrementAsync` - Increment numeric value
  - `StackExchange.Redis.IDatabase.StringDecrementAsync` - Decrement numeric value
  - `StackExchange.Redis.IDatabase.KeyDeleteAsync` - Delete key

  **Documentation References** (specs and requirements):
  - `https://stackexchange.github.io/StackExchange.Redis/Strings.html` - String operations documentation
  - `Baby.AudioData/Context/AudioInfoContext.cs:19-20` - DataContext pattern (for reference only, not following)

  **External References** (libraries and frameworks):
  - StackExchange.Redis Strings: `https://stackexchange.github.io/StackExchange.Redis/Strings.html` - Complete string operation reference

  **WHY Each Reference Matters** (explain the relevance):
  - AudioInfoController: Shows the correct async action method structure and InvokeResult response format
  - IDatabase methods: Provides the exact method signatures for string operations
  - StackExchange docs: Shows all string operations available

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] GET /api/redis/string/get implemented with key parameter
  - [ ] POST /api/redis/string/set implemented with key and value parameters
  - [ ] POST /api/redis/string/delete implemented with key parameter
  - [ ] POST /api/redis/string/increment implemented with key and optional value parameter
  - [ ] POST /api/redis/string/decrement implemented with key and optional value parameter
  - [ ] All methods return InvokeResult with correct format
  - [ ] Key validation: empty or null key returns error

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Set and get string value
    Tool: Bash (curl)
    Preconditions: Tasks 1-3 completed, server running on http://localhost:5001
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"test_key","value":"hello redis","expiry":3600}'
      2. Assert: HTTP status code is 200
      3. Assert: Response contains ResultCode "Success"
      4. curl -s -X GET http://localhost:5001/api/redis/string/get?key=test_key
      5. Assert: HTTP status code is 200
      6. Assert: Response contains ResultCode "Success"
      7. Assert: Response Data contains value "hello redis"
    Expected Result: String set successfully, retrieved correctly
    Evidence: Responses saved to .sisyphus/evidence/task-4-string-operations.json

  Scenario: Increment numeric value
    Tool: Bash (curl)
    Preconditions: Server running, test_key exists with value "10"
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"counter","value":"10"}'
      2. curl -s -X POST http://localhost:5001/api/redis/string/increment -H "Content-Type: application/json" -d '{"key":"counter","value":5}'
      3. Assert: HTTP status code is 200
      4. Assert: Response contains ResultCode "Success"
      5. curl -s -X GET http://localhost:5001/api/redis/string/get?key=counter
      6. Assert: Response Data contains value "15"
    Expected Result: Value incremented from 10 to 15
    Evidence: Responses saved to .sisyphus/evidence/task-4-increment.json

  Scenario: Delete string key
    Tool: Bash (curl)
    Preconditions: Server running, test_key exists
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/delete -H "Content-Type: application/json" -d '{"key":"test_key"}'
      2. Assert: HTTP status code is 200
      3. Assert: Response contains ResultCode "Success"
      4. curl -s -X GET http://localhost:5001/api/redis/string/get?key=test_key
      5. Assert: Response contains ResultCode indicating key not found
    Expected Result: Key deleted, subsequent get returns not found
    Evidence: Responses saved to .sisyphus/evidence/task-4-delete.json

  Scenario: Invalid key parameter returns error
    Tool: Bash (curl)
    Preconditions: Server running
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"","value":"test"}'
      2. Assert: HTTP status code is 200
      3. Assert: Response contains ResultCode indicating error
      4. Assert: Response contains ResultMessage about invalid key
    Expected Result: Error returned for empty key
    Evidence: Response saved to .sisyphus/evidence/task-4-validation.json

  **Commit**: YES
  - Message: `feat(redis): implement string operations`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 5. 实现Hash操作

  **What to do**:
  - 实现GET /api/redis/hash/get - 获取Hash字段值
  - 实现POST /api/redis/hash/set - 设置Hash字段值
  - 实现GET /api/redis/hash/getall - 获取所有字段
  - 实现POST /api/redis/hash/delete - 删除Hash字段
  - 实现GET /api/redis/hash/exists - 检查字段是否存在
  - 实现POST /api/redis/hash/deleteall - 删除整个Hash
  - 参数验证：key和field不能为空

  **Must NOT do**:
  - 不要实现其他数据结构操作

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: CRUD operations for Hash data structure, similar complexity to String
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: Redis IDatabase hash operations

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 4, 6, 7, 8)
  - **Blocks**: Task 9
  - **Blocked By**: Task 3

  **References** (CRITICAL - Be Exhaustive):

  **Pattern References** (existing code to follow):
  - `Baby.AudioData.InterfaceWeb/Controllers/AudioInfoController.cs:51-76` - Parameter validation pattern (page/pageSize)

  **API/Type References** (contracts to implement against):
  - `StackExchange.Redis.IDatabase.HashGetAsync` - Get hash field value
  - `StackExchange.Redis.IDatabase.HashSetAsync` - Set hash field value
  - `StackExchange.Redis.IDatabase.HashGetAllAsync` - Get all fields
  - `StackExchange.Redis.IDatabase.HashDeleteAsync` - Delete hash field(s)
  - `StackExchange.Redis.IDatabase.HashExistsAsync` - Check if field exists
  - `StackExchange.Redis.IDatabase.KeyDeleteAsync` - Delete entire hash

  **Documentation References** (specs and requirements):
  - `https://stackexchange.github.io/StackExchange.Redis/Hashes.html` - Hash operations documentation

  **External References** (libraries and frameworks):
  - StackExchange.Redis Hashes: `https://stackexchange.github.io/StackExchange.Redis/Hashes.html` - Complete hash operation reference

  **WHY Each Reference Matters** (explain the relevance):
  - AudioInfoController validation: Shows parameter validation pattern
  - IDatabase methods: Provides exact method signatures for hash operations

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] GET /api/redis/hash/get implemented with key and field parameters
  - [ ] POST /api/redis/hash/set implemented with key, field, value parameters
  - [ ] GET /api/redis/hash/getall implemented with key parameter
  - [ ] POST /api/redis/hash/delete implemented with key and field(s) parameters
  - [ ] GET /api/redis/hash/exists implemented with key and field parameters
  - [ ] POST /api/redis/hash/deleteall implemented with key parameter
  - [ ] All methods validate key and field parameters

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Set, get, and delete hash field
    Tool: Bash (curl)
    Preconditions: Server running on http://localhost:5001
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/hash/set -H "Content-Type: application/json" -d '{"key":"user:123","field":"name","value":"John Doe"}'
      2. curl -s -X POST http://localhost:5001/api/redis/hash/set -H "Content-Type: application/json" -d '{"key":"user:123","field":"age","value":"30"}'
      3. Assert: Both HTTP status codes are 200
      4. curl -s -X GET http://localhost:5001/api/redis/hash/get?key=user:123&field=name
      5. Assert: Response Data contains value "John Doe"
      6. curl -s -X GET http://localhost:5001/api/redis/hash/getall?key=user:123
      7. Assert: Response Data contains both name and age fields
      8. curl -s -X POST http://localhost:5001/api/redis/hash/delete -H "Content-Type: application/json" -d '{"key":"user:123","field":"age"}'
      9. Assert: Response contains ResultCode "Success"
      10. curl -s -X GET http://localhost:5001/api/redis/hash/get?key=user:123&field=age
      11. Assert: Response indicates field not found
    Expected Result: Hash operations work correctly, fields can be set, retrieved, and deleted
    Evidence: Responses saved to .sisyphus/evidence/task-5-hash-operations.json

  Scenario: Check hash field existence
    Tool: Bash (curl)
    Preconditions: Server running, user:123 hash exists
    Steps:
      1. curl -s -X GET http://localhost:5001/api/redis/hash/exists?key=user:123&field=name
      2. Assert: Response contains ResultCode "Success"
      3. Assert: Response Data contains exists true
      4. curl -s -X GET http://localhost:5001/api/redis/hash/exists?key=user:123&field=nonexistent
      5. Assert: Response contains ResultCode "Success"
      6. Assert: Response Data contains exists false
    Expected Result: Field existence check works correctly
    Evidence: Responses saved to .sisyphus/evidence/task-5-hash-exists.json

  Scenario: Delete entire hash
    Tool: Bash (curl)
    Preconditions: Server running, user:123 hash exists
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/hash/deleteall -H "Content-Type: application/json" -d '{"key":"user:123"}'
      2. Assert: Response contains ResultCode "Success"
      3. curl -s -X GET http://localhost:5001/api/redis/hash/getall?key=user:123
      4. Assert: Response indicates key not found or empty hash
    Expected Result: Entire hash deleted
    Evidence: Responses saved to .sisyphus/evidence/task-5-hash-deleteall.json

  **Commit**: YES
  - Message: `feat(redis): implement hash operations`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 6. 实现Set操作

  **What to do**:
  - 实现POST /api/redis/set/add - 添加成员
  - 实现POST /api/redis/set/remove - 移除成员
  - 实现GET /api/redis/set/members - 获取所有成员
  - 实现GET /api/redis/set/ismember - 检查成员是否存在
  - 实现GET /api/redis/set/count - 获取成员数量
  - 实现POST /api/redis/set/delete - 删除整个Set
  - 参数验证：key和member不能为空

  **Must NOT do**:
  - 不要实现其他数据结构操作

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: CRUD operations for Set data structure, similar complexity to Hash
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: Redis IDatabase set operations

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 4, 5, 7, 8)
  - **Blocks**: Task 9
  - **Blocked By**: Task 3

  **References** (CRITICAL - Be Exhaustive):

  **API/Type References** (contracts to implement against):
  - `StackExchange.Redis.IDatabase.SetAddAsync` - Add member to set
  - `StackExchange.Redis.IDatabase.SetRemoveAsync` - Remove member from set
  - `StackExchange.Redis.IDatabase.SetMembersAsync` - Get all members
  - `StackExchange.Redis.IDatabase.SetContainsAsync` - Check if member exists
  - `StackExchange.Redis.IDatabase.SetLengthAsync` - Get member count
  - `StackExchange.Redis.IDatabase.KeyDeleteAsync` - Delete entire set

  **Documentation References** (specs and requirements):
  - `https://stackexchange.github.io/StackExchange.Redis/Sets.html` - Set operations documentation

  **External References** (libraries and frameworks):
  - StackExchange.Redis Sets: `https://stackexchange.github.io/StackExchange.Redis/Sets.html` - Complete set operation reference

  **WHY Each Reference Matters** (explain the relevance):
  - IDatabase methods: Provides exact method signatures for set operations

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] POST /api/redis/set/add implemented with key and member parameters
  - [ ] POST /api/redis/set/remove implemented with key and member parameters
  - [ ] GET /api/redis/set/members implemented with key parameter
  - [ ] GET /api/redis/set/ismember implemented with key and member parameters
  - [ ] GET /api/redis/set/count implemented with key parameter
  - [ ] POST /api/redis/set/delete implemented with key parameter
  - [ ] All methods validate key and member parameters

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Add, check, and remove set members
    Tool: Bash (curl)
    Preconditions: Server running on http://localhost:5001
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/set/add -H "Content-Type: application/json" -d '{"key":"tags:article1","member":"redis"}'
      2. curl -s -X POST http://localhost:5001/api/redis/set/add -H "Content-Type: application/json" -d '{"key":"tags:article1","member":"database"}'
      3. curl -s -X POST http://localhost:5001/api/redis/set/add -H "Content-Type: application/json" -d '{"key":"tags:article1","member":"cache"}'
      4. Assert: All HTTP status codes are 200
      5. curl -s -X GET http://localhost:5001/api/redis/set/members?key=tags:article1
      6. Assert: Response Data contains array with 3 members
      7. curl -s -X GET http://localhost:5001/api/redis/set/count?key=tags:article1
      8. Assert: Response Data contains count 3
      9. curl -s -X POST http://localhost:5001/api/redis/set/remove -H "Content-Type: application/json" -d '{"key":"tags:article1","member":"cache"}'
      10. Assert: Response contains ResultCode "Success"
      11. curl -s -X GET http://localhost:5001/api/redis/set/count?key=tags:article1
      12. Assert: Response Data contains count 2
    Expected Result: Set operations work correctly, members can be added, checked, and removed
    Evidence: Responses saved to .sisyphus/evidence/task-6-set-operations.json

  Scenario: Check member existence
    Tool: Bash (curl)
    Preconditions: Server running, tags:article1 set exists
    Steps:
      1. curl -s -X GET http://localhost:5001/api/redis/set/ismember?key=tags:article1&member=redis
      2. Assert: Response contains ResultCode "Success"
      3. Assert: Response Data contains isMember true
      4. curl -s -X GET http://localhost:5001/api/redis/set/ismember?key=tags:article1&member=nonexistent
      5. Assert: Response contains ResultCode "Success"
      6. Assert: Response Data contains isMember false
    Expected Result: Member existence check works correctly
    Evidence: Responses saved to .sisyphus/evidence/task-6-set-exists.json

  Scenario: Delete entire set
    Tool: Bash (curl)
    Preconditions: Server running, tags:article1 set exists
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/set/delete -H "Content-Type: application/json" -d '{"key":"tags:article1"}'
      2. Assert: Response contains ResultCode "Success"
      3. curl -s -X GET http://localhost:5001/api/redis/set/count?key=tags:article1
      4. Assert: Response indicates key not found or count 0
    Expected Result: Entire set deleted
    Evidence: Responses saved to .sisyphus/evidence/task-6-set-delete.json

  **Commit**: YES
  - Message: `feat(redis): implement set operations`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 7. 实现Sorted Set操作

  **What to do**:
  - 实现POST /api/redis/zset/add - 添加成员（带分数）
  - 实现POST /api/redis/zset/remove - 移除成员
  - 实现GET /api/redis/zset/range - 获取范围内的成员
  - 实现GET /api/redis/zset/rangebyscore - 按分数范围获取
  - 实现GET /api/redis/zset/rank - 获取成员排名
  - 实现GET /api/redis/zset/score - 获取成员分数
  - 实现GET /api/redis/zset/count - 获取成员数量
  - 实现POST /api/redis/zset/delete - 删除整个ZSet
  - 参数验证：key和member不能为空，score必须是数字

  **Must NOT do**:
  - 不要实现其他数据结构操作

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: CRUD operations for Sorted Set, slightly more complex than regular Set
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: Redis IDatabase sorted set operations

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 4, 5, 6, 8)
  - **Blocks**: Task 9
  - **Blocked By**: Task 3

  **References** (CRITICAL - Be Exhaustive):

  **API/Type References** (contracts to implement against):
  - `StackExchange.Redis.IDatabase.SortedSetAddAsync` - Add member with score
  - `StackExchange.Redis.IDatabase.SortedSetRemoveAsync` - Remove member
  - `StackExchange.Redis.IDatabase.SortedSetRangeByRankAsync` - Get by rank
  - `StackExchange.Redis.IDatabase.SortedSetRangeByScoreAsync` - Get by score range
  - `StackExchange.Redis.IDatabase.SortedSetRankAsync` - Get member rank
  - `StackExchange.Redis.IDatabase.SortedSetScoreAsync` - Get member score
  - `StackExchange.Redis.IDatabase.SortedSetLengthAsync` - Get member count
  - `StackExchange.Redis.IDatabase.KeyDeleteAsync` - Delete entire zset

  **Documentation References** (specs and requirements):
  - `https://stackexchange.github.io/StackExchange.Redis/SortedSets.html` - Sorted Set operations documentation

  **External References** (libraries and frameworks):
  - StackExchange.Redis SortedSets: `https://stackexchange.github.io/StackExchange.Redis/SortedSets.html` - Complete sorted set operation reference

  **WHY Each Reference Matters** (explain the relevance):
  - IDatabase methods: Provides exact method signatures for sorted set operations

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] POST /api/redis/zset/add implemented with key, member, score parameters
  - [ ] POST /api/redis/zset/remove implemented with key and member parameters
  - [ ] GET /api/redis/zset/range implemented with key, start, stop, order parameters
  - [ ] GET /api/redis/zset/rangebyscore implemented with key, min, max parameters
  - [ ] GET /api/redis/zset/rank implemented with key and member parameters
  - [ ] GET /api/redis/zset/score implemented with key and member parameters
  - [ ] GET /api/redis/zset/count implemented with key parameter
  - [ ] POST /api/redis/zset/delete implemented with key parameter
  - [ ] All methods validate key, member, and score parameters

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Add members and retrieve by rank
    Tool: Bash (curl)
    Preconditions: Server running on http://localhost:5001
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/zset/add -H "Content-Type: application/json" -d '{"key":"leaderboard","member":"Alice","score":100}'
      2. curl -s -X POST http://localhost:5001/api/redis/zset/add -H "Content-Type: application/json" -d '{"key":"leaderboard","member":"Bob","score":85}'
      3. curl -s -X POST http://localhost:5001/api/redis/zset/add -H "Content-Type: application/json" -d '{"key":"leaderboard","member":"Charlie","score":95}'
      4. Assert: All HTTP status codes are 200
      5. curl -s -X GET "http://localhost:5001/api/redis/zset/range?key=leaderboard&start=0&stop=-1&order=desc"
      6. Assert: Response Data contains array with 3 members in order: Alice, Charlie, Bob
      7. curl -s -X GET http://localhost:5001/api/redis/zset/rank?key=leaderboard&member=Charlie
      8. Assert: Response Data contains rank 1 (0-indexed)
    Expected Result: Sorted set operations work correctly, members ordered by score
    Evidence: Responses saved to .sisyphus/evidence/task-7-zset-operations.json

  Scenario: Retrieve by score range
    Tool: Bash (curl)
    Preconditions: Server running, leaderboard zset exists
    Steps:
      1. curl -s -X GET "http://localhost:5001/api/redis/zset/rangebyscore?key=leaderboard&min=90&max=100"
      2. Assert: Response Data contains array with Alice (100) and Charlie (95)
      3. curl -s -X GET "http://localhost:5001/api/redis/zset/rangebyscore?key=leaderboard&min=0&max=80"
      4. Assert: Response Data contains empty array or no Bob (85 not in range)
    Expected Result: Score range filtering works correctly
    Evidence: Responses saved to .sisyphus/evidence/task-7-zset-score-range.json

  Scenario: Get member score and remove member
    Tool: Bash (curl)
    Preconditions: Server running, leaderboard zset exists
    Steps:
      1. curl -s -X GET http://localhost:5001/api/redis/zset/score?key=leaderboard&member=Alice
      2. Assert: Response Data contains score 100
      3. curl -s -X POST http://localhost:5001/api/redis/zset/remove -H "Content-Type: application/json" -d '{"key":"leaderboard","member":"Alice"}'
      4. Assert: Response contains ResultCode "Success"
      5. curl -s -X GET "http://localhost:5001/api/redis/zset/range?key=leaderboard&start=0&stop=-1"
      6. Assert: Response Data contains only 2 members (Bob and Charlie)
    Expected Result: Member score retrieved and member removed
    Evidence: Responses saved to .sisyphus/evidence/task-7-zset-score-remove.json

  **Commit**: YES
  - Message: `feat(redis): implement sorted set operations`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 8. 实现List操作

  **What to do**:
  - 实现POST /api/redis/list/lpush - 从左侧推入
  - 实现POST /api/redis/list/rpush - 从右侧推入
  - 实现POST /api/redis/list/lpop - 从左侧弹出
  - 实现POST /api/redis/list/rpop - 从右侧弹出
  - 实现GET /api/redis/list/lrange - 获取范围内的元素
  - 实现GET /api/redis/list/lindex - 获取指定索引的元素
  - 实现GET /api/redis/list/llen - 获取列表长度
  - 实现POST /api/redis/list/delete - 删除整个List
  - 参数验证：key不能为空，index和range参数验证

  **Must NOT do**:
  - 不要实现其他数据结构操作

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: CRUD operations for List data structure, similar complexity to Set
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: Redis IDatabase list operations

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 4, 5, 6, 7)
  - **Blocks**: Task 9
  - **Blocked By**: Task 3

  **References** (CRITICAL - Be Exhaustive):

  **API/Type References** (contracts to implement against):
  - `StackExchange.Redis.IDatabase.ListLeftPushAsync` - Left push
  - `StackExchange.Redis.IDatabase.ListRightPushAsync` - Right push
  - `StackExchange.Redis.IDatabase.ListLeftPopAsync` - Left pop
  - `StackExchange.Redis.IDatabase.ListRightPopAsync` - Right pop
  - `StackExchange.Redis.IDatabase.ListRangeAsync` - Get range
  - `StackExchange.Redis.IDatabase.ListGetByIndexAsync` - Get by index
  - `StackExchange.Redis.IDatabase.ListLengthAsync` - Get length
  - `StackExchange.Redis.IDatabase.KeyDeleteAsync` - Delete entire list

  **Documentation References** (specs and requirements):
  - `https://stackexchange.github.io/StackExchange.Redis/Lists.html` - List operations documentation

  **External References** (libraries and frameworks):
  - StackExchange.Redis Lists: `https://stackexchange.github.io/StackExchange.Redis/Lists.html` - Complete list operation reference

  **WHY Each Reference Matters** (explain the relevance):
  - IDatabase methods: Provides exact method signatures for list operations

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] POST /api/redis/list/lpush implemented with key and value parameters
  - [ ] POST /api/redis/list/rpush implemented with key and value parameters
  - [ ] POST /api/redis/list/lpop implemented with key parameter
  - [ ] POST /api/redis/list/rpop implemented with key parameter
  - [ ] GET /api/redis/list/lrange implemented with key, start, stop parameters
  - [ ] GET /api/redis/list/lindex implemented with key and index parameters
  - [ ] GET /api/redis/list/llen implemented with key parameter
  - [ ] POST /api/redis/list/delete implemented with key parameter
  - [ ] All methods validate key and numeric parameters

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Push, pop, and range operations
    Tool: Bash (curl)
    Preconditions: Server running on http://localhost:5001
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/list/lpush -H "Content-Type: application/json" -d '{"key":"queue","value":"task3"}'
      2. curl -s -X POST http://localhost:5001/api/redis/list/lpush -H "Content-Type: application/json" -d '{"key":"queue","value":"task2"}'
      3. curl -s -X POST http://localhost:5001/api/redis/list/rpush -H "Content-Type: application/json" -d '{"key":"queue","value":"task1"}'
      4. Assert: All HTTP status codes are 200
      5. curl -s -X GET "http://localhost:5001/api/redis/list/lrange?key=queue&start=0&stop=-1"
      6. Assert: Response Data contains array: [task2, task3, task1]
      7. curl -s -X GET http://localhost:5001/api/redis/list/lindex?key=queue&index=0
      8. Assert: Response Data contains value "task2"
      9. curl -s -X POST http://localhost:5001/api/redis/list/lpop -H "Content-Type: application/json" -d '{"key":"queue"}'
      10. Assert: Response Data contains popped value "task2"
      11. curl -s -X GET "http://localhost:5001/api/redis/list/lrange?key=queue&start=0&stop=-1"
      12. Assert: Response Data contains array: [task3, task1]
    Expected Result: List operations work correctly, queue behavior as expected
    Evidence: Responses saved to .sisyphus/evidence/task-8-list-operations.json

  Scenario: Get list length and delete list
    Tool: Bash (curl)
    Preconditions: Server running, queue list exists
    Steps:
      1. curl -s -X GET http://localhost:5001/api/redis/list/llen?key=queue
      2. Assert: Response Data contains length 2
      3. curl -s -X POST http://localhost:5001/api/redis/list/delete -H "Content-Type: application/json" -d '{"key":"queue"}'
      4. Assert: Response contains ResultCode "Success"
      5. curl -s -X GET http://localhost:5001/api/redis/list/llen?key=queue
      6. Assert: Response indicates key not found or length 0
    Expected Result: List length retrieved and list deleted
    Evidence: Responses saved to .sisyphus/evidence/task-8-list-delete.json

  Scenario: RPOP right pop operation
    Tool: Bash (curl)
    Preconditions: Server running, queue list with [task3, task1]
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/list/rpop -H "Content-Type: application/json" -d '{"key":"queue"}'
      2. Assert: Response Data contains popped value "task1"
      3. curl -s -X GET "http://localhost:5001/api/redis/list/lrange?key=queue&start=0&stop=-1"
      4. Assert: Response Data contains array: [task3]
    Expected Result: Right pop works correctly
    Evidence: Responses saved to .sisyphus/evidence/task-8-list-rpop.json

  **Commit**: YES
  - Message: `feat(redis): implement list operations`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 9. 实现键管理功能

  **What to do**:
  - 实现GET /api/redis/key/exists - 检查键是否存在
  - 实现GET /api/redis/key/type - 获取键类型
  - 实现GET /api/redis/key/ttl - 获取过期时间
  - 实现POST /api/redis/key/expire - 设置过期时间
  - 实现GET /api/redis/key/scan - 扫描键（支持模式匹配）
  - 实现POST /api/redis/key/rename - 重命名键
  - 实现POST /api/redis/key/move - 移动键到其他数据库
  - 实现POST /api/redis/key/delete - 删除键
  - 实现POST /api/redis/key/flushdb - 清空当前数据库（危险操作）
  - 参数验证：key不能为空，db参数验证

  **Must NOT do**:
  - 不要实现数据结构操作（已完成）
  - 不要实现Keys命令（不推荐在生产环境使用）

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: Key management operations, straightforward implementations
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: Redis IDatabase key operations

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 3 (with Task 10)
  - **Blocks**: Task 11
  - **Blocked By**: Tasks 4, 5, 6, 7, 8

  **References** (CRITICAL - Be Exhaustive):

  **API/Type References** (contracts to implement against):
  - `StackExchange.Redis.IDatabase.KeyExistsAsync` - Check key existence
  - `StackExchange.Redis.IDatabase.KeyTypeAsync` - Get key type
  - `StackExchange.Redis.IDatabase.KeyTimeToLiveAsync` - Get TTL
  - `StackExchange.Redis.IDatabase.KeyExpireAsync` - Set expiration
  - `StackExchange.Redis.IDatabase.KeyRenameAsync` - Rename key
  - `StackExchange.Redis.IDatabase.KeyMoveAsync` - Move to other DB
  - `StackExchange.Redis.IDatabase.KeyDeleteAsync` - Delete key
  - `StackExchange.Redis.IDatabase.FlushDatabaseAsync` - Flush database
  - `StackExchange.Redis.IDatabase.ScanAsync` - Scan keys (preferred over Keys)

  **Documentation References** (specs and requirements):
  - `https://stackexchange.github.io/StackExchange.Redis/Keys.html` - Key operations documentation

  **External References** (libraries and frameworks):
  - StackExchange.Redis Keys: `https://stackexchange.github.io/StackExchange.Redis/Keys.html` - Complete key operation reference

  **WHY Each Reference Matters** (explain the relevance):
  - IDatabase methods: Provides exact method signatures for key operations

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] GET /api/redis/key/exists implemented with key parameter
  - [ ] GET /api/redis/key/type implemented with key parameter
  - [ ] GET /api/redis/key/ttl implemented with key parameter
  - [ ] POST /api/redis/key/expire implemented with key and expiry parameters
  - [ ] GET /api/redis/key/scan implemented with pattern and cursor parameters
  - [ ] POST /api/redis/key/rename implemented with key and newKey parameters
  - [ ] POST /api/redis/key/move implemented with key, destinationDb parameters
  - [ ] POST /api/redis/key/delete implemented with key parameter
  - [ ] POST /api/redis/key/flushdb implemented (with safety confirmation)
  - [ ] All methods validate key parameter

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Check key existence, type, and TTL
    Tool: Bash (curl)
    Preconditions: Server running, test_key exists as string
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"test_ttl_key","value":"value","expiry":3600}'
      2. curl -s -X GET http://localhost:5001/api/redis/key/exists?key=test_ttl_key
      3. Assert: Response Data contains exists true
      4. curl -s -X GET http://localhost:5001/api/redis/key/type?key=test_ttl_key
      5. Assert: Response Data contains type "String"
      6. curl -s -X GET http://localhost:5001/api/redis/key/ttl?key=test_ttl_key
      7. Assert: Response Data contains ttl (should be > 0 and < 3600)
    Expected Result: Key metadata retrieved correctly
    Evidence: Responses saved to .sisyphus/evidence/task-9-key-metadata.json

  Scenario: Set expiration and verify TTL
    Tool: Bash (curl)
    Preconditions: Server running, test_key exists
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/key/expire -H "Content-Type: application/json" -d '{"key":"test_ttl_key","expiry":60}'
      2. Assert: Response contains ResultCode "Success"
      3. curl -s -X GET http://localhost:5001/api/redis/key/ttl?key=test_ttl_key
      4. Assert: Response Data contains ttl (should be <= 60)
    Expected Result: Expiration set successfully, TTL reflects change
    Evidence: Responses saved to .sisyphus/evidence/task-9-key-expire.json

  Scenario: Scan keys with pattern
    Tool: Bash (curl)
    Preconditions: Server running, multiple test:* keys exist
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"test:scan:1","value":"value1"}'
      2. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"test:scan:2","value":"value2"}'
      3. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"other:key","value":"value3"}'
      4. curl -s -X GET "http://localhost:5001/api/redis/key/scan?pattern=test:*&count=10"
      5. Assert: Response Data contains test:scan:1 and test:scan:2
      6. Assert: Response Data does NOT contain other:key
    Expected Result: Scan returns only matching keys
    Evidence: Responses saved to .sisyphus/evidence/task-9-key-scan.json

  Scenario: Rename key
    Tool: Bash (curl)
    Preconditions: Server running, old_key exists
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"old_key","value":"value"}'
      2. curl -s -X POST http://localhost:5001/api/redis/key/rename -H "Content-Type: application/json" -d '{"key":"old_key","newKey":"new_key"}'
      3. Assert: Response contains ResultCode "Success"
      4. curl -s -X GET http://localhost:5001/api/redis/key/exists?key=old_key
      5. Assert: Response Data contains exists false
      6. curl -s -X GET http://localhost:5001/api/redis/key/exists?key=new_key
      7. Assert: Response Data contains exists true
    Expected Result: Key renamed successfully
    Evidence: Responses saved to .sisyphus/evidence/task-9-key-rename.json

  **Commit**: YES
  - Message: `feat(redis): implement key management operations`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 10. 实现缓存管理功能

  **What to do**:
  - 实现GET /api/redis/cache/info - 获取缓存状态信息（键类型、TTL、大小等）
  - 实现POST /api/redis/cache/clear - 清除指定键的缓存
  - 实现POST /api/redis/cache/batchclear - 批量清除缓存（支持模式匹配）
  - 实现POST /api/redis/cache/refresh - 刷新缓存（删除后重新加载）
  - 实现GET /api/redis/cache/stats - 获取缓存统计信息（可选，需要额外实现统计逻辑）
  - 参数验证：key不能为空

  **Must NOT do**:
  - 不要实现数据结构操作（已完成）
  - 不要实现键管理（已完成）

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `quick`
    - Reason: Cache management operations, combines existing Redis operations
  - **Skills**: [`csharp-code-workflow`]
    - `csharp-code-workflow`: Redis IDatabase operations, caching patterns

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 3 (with Task 9)
  - **Blocks**: Task 11
  - **Blocked By**: Tasks 4, 5, 6, 7, 8

  **References** (CRITICAL - Be Exhaustive):

  **Pattern References** (existing code to follow):
  - `Baby.AudioData.Core/AudioDataProcess.cs:17-25` - Cache access pattern (GetCacheEntity usage) - for reference only, not following

  **API/Type References** (contracts to implement against):
  - Uses combinations of String, Hash, Key operations already implemented
  - No new Redis APIs needed for basic cache management

  **Documentation References** (specs and requirements):
  - `Baby.AudioData/AudioVariable.cs:12-42` - Existing cache configuration (ProviderName, ExpiresIn)

  **External References** (libraries and frameworks):
  - StackExchange.Redis: All operations reuse previously implemented methods

  **WHY Each Reference Matters** (explain the relevance):
  - AudioDataProcess: Shows existing cache pattern (for understanding, not copying)
  - AudioVariable: Shows cache configuration values

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] GET /api/redis/cache/info implemented with key parameter
  - [ ] POST /api/redis/cache/clear implemented with key parameter
  - [ ] POST /api/redis/cache/batchclear implemented with pattern parameter
  - [ ] POST /api/redis/cache/refresh implemented with key parameter
  - [ ] GET /api/redis/cache/stats implemented (optional)
  - [ ] All methods validate key parameter

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: Get cache info and clear cache
    Tool: Bash (curl)
    Preconditions: Server running on http://localhost:5001, test_cache_key exists
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"test_cache_key","value":"cache value","expiry":3600}'
      2. curl -s -X GET http://localhost:5001/api/redis/cache/info?key=test_cache_key
      3. Assert: Response contains ResultCode "Success"
      4. Assert: Response Data contains type, ttl, size information
      5. curl -s -X POST http://localhost:5001/api/redis/cache/clear -H "Content-Type: application/json" -d '{"key":"test_cache_key"}'
      6. Assert: Response contains ResultCode "Success"
      7. curl -s -X GET http://localhost:5001/api/redis/key/exists?key=test_cache_key
      8. Assert: Response Data contains exists false
    Expected Result: Cache info retrieved and cache cleared
    Evidence: Responses saved to .sisyphus/evidence/task-10-cache-clear.json

  Scenario: Batch clear with pattern
    Tool: Bash (curl)
    Preconditions: Server running, cache:* keys exist
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"cache:1","value":"value1"}'
      2. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"cache:2","value":"value2"}'
      3. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"other:key","value":"value3"}'
      4. curl -s -X POST http://localhost:5001/api/redis/cache/batchclear -H "Content-Type: application/json" -d '{"pattern":"cache:*"}'
      5. Assert: Response contains ResultCode "Success"
      6. curl -s -X GET http://localhost:5001/api/redis/key/exists?key=cache:1
      7. Assert: Response Data contains exists false
      8. curl -s -X GET http://localhost:5001/api/redis/key/exists?key=other:key
      9. Assert: Response Data contains exists true
    Expected Result: Only matching keys cleared
    Evidence: Responses saved to .sisyphus/evidence/task-10-cache-batchclear.json

  Scenario: Refresh cache (delete and recreate)
    Tool: Bash (curl)
    Preconditions: Server running, test_refresh_key exists
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"test_refresh_key","value":"old value"}'
      2. curl -s -X POST http://localhost:5001/api/redis/cache/refresh -H "Content-Type: application/json" -d '{"key":"test_refresh_key","newValue":"new value","expiry":3600}'
      3. Assert: Response contains ResultCode "Success"
      4. curl -s -X GET http://localhost:5001/api/redis/string/get?key=test_refresh_key
      5. Assert: Response Data contains value "new value"
      6. curl -s -X GET http://localhost:5001/api/redis/key/ttl?key=test_refresh_key
      7. Assert: Response Data contains ttl (should be near 3600)
    Expected Result: Cache refreshed with new value and expiration
    Evidence: Responses saved to .sisyphus/evidence/task-10-cache-refresh.json

  **Commit**: YES
  - Message: `feat(redis): implement cache management operations`
  - Files: `Baby.AudioData.InterfaceWeb/Controllers/RedisController.cs`
  - Pre-commit: `dotnet build`

---

- [ ] 11. API测试验证

  **What to do**:
  - 运行完整的API测试套件
  - 验证所有数据结构操作
  - 验证所有键管理操作
  - 验证所有缓存管理操作
  - 测试错误场景（无效参数、不存在的键等）
  - 验证响应格式统一性
  - 生成测试报告

  **Must NOT do**:
  - 不要修改已实现的功能
  - 不要添加新功能（仅测试验证）

  **Recommended Agent Profile**:
  > Select category + skills based on task domain. Justify each choice.
  - **Category**: `unspecified-low`
    - Reason: Testing and verification task, not implementing new code
  - **Skills**: []
    - No specific skills needed for running curl tests

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential (final task)
  - **Blocks**: None
  - **Blocked By**: Tasks 4, 5, 6, 7, 8, 9, 10

  **References** (CRITICAL - Be Exhaustive):

  **Pattern References** (existing code to follow):
  - Previous tasks' QA scenarios - Test patterns to follow

  **API/Type References** (contracts to implement against):
  - All API endpoints implemented in Tasks 4-10

  **Documentation References** (specs and requirements):
  - All TODO items from Tasks 4-10 - Acceptance criteria to verify

  **External References** (libraries and frameworks):
  - curl documentation: `https://curl.se/docs/` - For advanced curl usage if needed

  **WHY Each Reference Matters** (explain the relevance):
  - Previous QA scenarios: Provide the exact test commands and expected outcomes
  - Acceptance criteria: Define what must be verified

  **Acceptance Criteria**:

  > **AGENT-EXECUTABLE VERIFICATION ONLY** — No human action permitted.
  > Every criterion MUST be verifiable by running a command or using a tool.

  - [ ] All String operations tested and passing
  - [ ] All Hash operations tested and passing
  - [ ] All Set operations tested and passing
  - [ ] All ZSet operations tested and passing
  - [ ] All List operations tested and passing
  - [ ] All Key management operations tested and passing
  - [ ] All Cache management operations tested and passing
  - [ ] Error scenarios tested (invalid parameters, missing keys)
  - [ ] Response format consistent across all endpoints
  - [ ] Test report generated

  **Agent-Executed QA Scenarios (MANDATORY — per-scenario, ultra-detailed):**

  Scenario: End-to-end test of all features
    Tool: Bash (curl + script)
    Preconditions: All previous tasks completed, server running on http://localhost:5001
    Steps:
      1. Create test script with all test cases from Tasks 4-10
      2. Run test script: `bash .sisyphus/evidence/test-all-redis-apis.sh`
      3. Collect all responses: `.sisyphus/evidence/task-11-full-test.log`
      4. Assert: All tests pass (exit code 0)
      5. Generate summary: Count total tests, passed, failed
    Expected Result: All API endpoints tested successfully
    Evidence: Test report saved to .sisyphus/evidence/task-11-test-summary.txt

  Scenario: Error handling verification
    Tool: Bash (curl)
    Preconditions: Server running
    Steps:
      1. curl -s -X POST http://localhost:5001/api/redis/string/set -H "Content-Type: application/json" -d '{"key":"","value":"test"}'
      2. Assert: Response contains error ResultCode
      3. curl -s -X GET http://localhost:5001/api/redis/string/get?key=nonexistent_key_12345
      4. Assert: Response contains appropriate error or null data
      5. curl -s -X POST http://localhost:5001/api/redis/hash/set -H "Content-Type: application/json" -d '{"key":"test","field":"","value":"value"}'
      6. Assert: Response contains error ResultCode
    Expected Result: All error cases handled gracefully
    Evidence: Error responses saved to .sisyphus/evidence/task-11-error-handling.json

  **Commit**: YES (if test script added)
  - Message: `test(redis): add comprehensive API test suite`
  - Files: `.sisyphus/evidence/test-all-redis-apis.sh`
  - Pre-commit: None

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 1 | `feat(deps): add StackExchange.Redis NuGet package` | `Baby.AudioData.InterfaceWeb.csproj` | `dotnet restore && dotnet build` |
| 2 | `feat(config): register Redis connection in Startup` | `Startup.cs` | `dotnet run --urls "http://localhost:5001"` |
| 3+4 | `feat(redis): implement RedisController and string operations` | `RedisController.cs` | `dotnet build && curl tests` |
| 5 | `feat(redis): implement hash operations` | `RedisController.cs` | `curl tests` |
| 6 | `feat(redis): implement set operations` | `RedisController.cs` | `curl tests` |
| 7 | `feat(redis): implement sorted set operations` | `RedisController.cs` | `curl tests` |
| 8 | `feat(redis): implement list operations` | `RedisController.cs` | `curl tests` |
| 9 | `feat(redis): implement key management operations` | `RedisController.cs` | `curl tests` |
| 10 | `feat(redis): implement cache management operations` | `RedisController.cs` | `curl tests` |
| 11 | `test(redis): add comprehensive API test suite` | `test-all-redis-apis.sh` | `bash test-all-redis-apis.sh` |

---

## Success Criteria

### Verification Commands
```bash
# Build and restore
cd Baby.AudioData.InterfaceWeb && dotnet restore && dotnet build

# Run server
dotnet run --urls "http://localhost:5001" &

# Health check
curl http://localhost:5001/health

# Full test suite
bash .sisyphus/evidence/test-all-redis-apis.sh
```

### Final Checklist
- [ ] All "Must Have" features implemented
- [ ] All "Must NOT Have" guardrails respected
- [ ] All tests pass (Tasks 4-10 acceptance criteria)
- [ ] Error handling comprehensive
- [ ] Response format consistent (InvokeResult)
- [ ] No security restrictions applied (完全开放)
- [ ] Keys command NOT used (Scan preferred)
- [ ] Health check endpoint preserved
- [ ] Code compiles without errors

---

## Security and Performance Considerations

### Security Considerations
**WARNING**: 完全开放的Redis API存在以下安全风险：
- 任何客户端可以访问所有Redis操作
- 可以删除、修改任意键
- 可以清空整个数据库（FlushDB）
- 没有认证或授权机制

**推荐最佳实践**（虽然未在需求中）：
1. 添加IP白名单限制（仅内网访问）
2. 使用JWT或API Key认证
3. 限制危险操作（如FlushDB）
4. 添加操作审计日志
5. 使用Redis ACL限制操作范围

### Performance Considerations
- **Scan vs Keys**: 优先使用Scan而不是Keys，避免阻塞Redis
- **批量操作**: 支持批量设置、删除以提高效率
- **连接池**: ConnectionMultiplexer已配置为单例，避免重复连接
- **异步操作**: 所有Redis操作使用Async方法
- **超时处理**: 添加Redis操作超时设置

---

## Extension Points (Future Enhancements)

### Optional Future Features
- 事务支持（MULTI/EXEC）
- 管道（Pipeline）批量操作
- 发布/订阅（Pub/Sub）功能
- Lua脚本支持
- 缓存预热接口
- 缓存命中率统计
- 性能监控指标

### Configuration Enhancements
- 支持多个Redis实例
- 可配置的默认过期时间
- 可配置的最大返回值大小
- 速率限制

---

**Plan saved to**: `.sisyphus/plans/interfaceweb-redis-methods.md`

**Estimated implementation time**: 4-6 hours
**Total number of API endpoints**: ~30+
**Files to create/modify**: 4 files
