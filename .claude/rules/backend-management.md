# Backend Management (ManageWeb)

## 项目概述

`Baby.AudioData.ManageWeb` 是后台管理网站，使用 ASP.NET Core MVC + Areas 模式开发。

**目标框架**：.NET 10.0

## Areas 区域结构

后台管理使用 MVC Areas 组织功能模块：

```
Baby.AudioData.ManageWeb/
└── Areas/
    └── AudioDataManage/
        ├── Controllers/         # 控制器（继承 PowerController）
        │   ├── PowerController.cs (基类)
        │   ├── AlbumInfoController.cs
        │   ├── AlbumInfoController.List.cs (部分类)
        │   └── ...
        ├── Views/              # 视图
        │   ├── AlbumInfo/
        │   │   ├── Edit.cshtml
        │   │   └── List.cshtml
        │   ├── _ViewImports.cshtml
        │   └── _ViewStart.cshtml
        ├── Data/               # 数据访问（预留）
        └── Models/             # 视图模型（预留）
```

## 权限管理

后台管理使用 `Leo.PowerData` 框架进行权限控制。

### 控制器继承

所有管理控制器必须继承 `PowerController` 基类：

```csharp
[Area("AudioDataManage")]
[Route("AudioDataManage/[controller]/[action]")]
public partial class PowerController : PowerBaseController
{
    // 基类提供权限管理、操作日志等功能
}

public partial class AlbumInfoController : PowerController
{
    // 具体业务控制器
}
```

### 权限过滤

使用 `PowerFilter` 特性控制操作权限：

```csharp
[PowerFilter(ButtonPlaceType.Top)]
public ActionResult Add()
{
    return View("Edit", new AlbumInfo());
}

[PowerFilter(ButtonPlaceType.Top)]
public ActionResult Modify(Int32 albumID)
{
    var entity = context.Get(albumID);
    if (entity.IsNull()) return Redirect("/AudioDataManage/AlbumInfo/List");
    return View("Edit", entity);
}
```

**ButtonPlaceType 选项**：
- `Top` - 顶部按钮
- `Row` - 行操作按钮
- `Other` - 其他位置

### 操作日志

重要操作需要记录操作日志：

```csharp
WriteOperationLog(OperationType.Insert, id, "新增专辑：" + entity.AlbumName, entity);
WriteOperationLog(OperationType.Update, id, "修改专辑：" + entity.AlbumName, entity);
WriteOperationLog(OperationType.Delete, id, "删除专辑：" + entity.AlbumName, entity);
```

## 典型 CRUD 模式

以下是完整的 CRUD 控制器示例，参考 `AlbumInfoController`：

### 控制器定义

```csharp
[Area("AudioDataManage")]
[Route("AudioDataManage/[controller]/[action]")]
public partial class AlbumInfoController : PowerController
{
    AlbumInfoContext context = new AlbumInfoContext();
    AlbumAudioContext albumAudioContext = new AlbumAudioContext();
}
```

### 新增页面

```csharp
[PowerFilter(ButtonPlaceType.Top)]
public ActionResult Add()
{
    return View("Edit", new AlbumInfo());
}
```

### 修改页面

```csharp
[PowerFilter(ButtonPlaceType.Top)]
public ActionResult Modify(Int32 albumID)
{
    var entity = context.Get(albumID);
    if (entity.IsNull())
    {
        return Redirect("/AudioDataManage/AlbumInfo/List");
    }
    return View("Edit", entity);
}
```

### 保存操作

```csharp
public JsonInfoResult Save(Int32 albumID)
{
    var invokeResult = new InvokeResult();
    var entity = albumID > 0 ? context.Get(albumID) : new AlbumInfo();

    if (entity.IsNull())
    {
        invokeResult.ResultCode = "HintMessage";
        invokeResult.ResultMessage = "此记录不存在，因此无法保存";
        return JsonInfo(invokeResult);
    }

    // 设置属性
    entity.AlbumName = GetString("AlbumName");
    // ... 其他属性

    if (albumID > 0)
    {
        // 更新
        entity.ModifyDate = DateTime.Now;
        context.Update(entity);
        WriteOperationLog(OperationType.Update, entity.AlbumID, "修改专辑：" + entity.AlbumName, entity);
    }
    else
    {
        // 新增
        entity.CreateDate = DateTime.Now;
        context.Add(entity);
        WriteOperationLog(OperationType.Insert, entity.AlbumID, "新增专辑:" + entity.AlbumName, entity);
    }

    invokeResult.EventAlert("保存成功").EventTarget("/AudioDataManage/AlbumInfo/List");
    return JsonInfo(invokeResult);
}
```

### 删除操作

```csharp
public JsonInfoResult Remove(Int32 albumID)
{
    var invokeResult = new InvokeResult();
    var entity = context.Get(albumID);

    if (entity.IsNull())
    {
        invokeResult.ResultCode = "HintMessage";
        invokeResult.ResultMessage = "此记录不存在，因此无法删除";
        return JsonInfo(invokeResult);
    }

    // 业务验证
    if (albumAudioContext.Any("AlbumID=" + albumID, null))
    {
        invokeResult.ResultCode = "HintMessage";
        invokeResult.ResultMessage = "专辑存在音频，因此无法删除";
        return JsonInfo(invokeResult);
    }

    // 执行删除
    context.Delete(albumID);

    // 清理缓存
    context.DeleteCacheEntity(entity.AlbumID, AudioVariable.ProviderName, AudioVariable.Db);
    context.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
    context.GetSyncDeleteCacheEntityKey(entity.AlbumID, AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();
    context.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();

    // 记录日志
    WriteOperationLog(OperationType.Delete, entity.AlbumID, "删除专辑:" + entity.AlbumName, entity);

    invokeResult.EventAlert("删除成功").EventRefreshGrid();
    return JsonInfo(invokeResult);
}
```

### 列表查询

```csharp
public ActionResult List()
{
    // 可以使用 partial 类分离列表功能
    // AlbumInfoController.List.cs
    return View();
}
```

## JsonInfoResult 响应

`JsonInfoResult` 是 LeoCore 框架提供的统一响应格式：

### 成功响应

```csharp
invokeResult.EventAlert("操作成功");
invokeResult.EventTarget("/AudioDataManage/AlbumInfo/List");  // 跳转
invokeResult.EventRefreshGrid();  // 刷新列表
return JsonInfo(invokeResult);
```

### 错误响应

```csharp
invokeResult.ResultCode = "HintMessage";
invokeResult.ResultMessage = "此记录不存在，因此无法保存";
return JsonInfo(invokeResult);
```

## 获取请求参数

LeoCore 提供的扩展方法：

```csharp
// 获取字符串参数
string albumName = GetString("AlbumName");

// 获取整数参数
Int32 albumID = GetInt("AlbumID");

// 获取布尔参数
bool isActive = GetBoolean("IsActive");
```

## 视图约定

### Edit.cshtml

新增和修改共用同一个视图：

```csharp
@model Baby.AudioData.AlbumInfo

<form method="post" action="/AudioDataManage/AlbumInfo/Save">
    <input type="hidden" name="AlbumID" value="@Model.AlbumID" />

    <div class="form-group">
        <label>专辑名称</label>
        <input type="text" name="AlbumName" value="@Model.AlbumName" />
    </div>

    <!-- 其他表单字段 -->

    <button type="submit">保存</button>
</form>
```

### List.cshtml

列表视图通常包含：
- 搜索表单
- 数据表格
- 操作按钮

## 部分类（Partial Classes）

大型控制器可以使用 partial 类分离功能：

```csharp
// AlbumInfoController.cs
public partial class AlbumInfoController : PowerController
{
    // 主要功能：Add, Modify, Save, Remove
}

// AlbumInfoController.List.cs
public partial class AlbumInfoController
{
    // 列表相关功能：List, Search, Export
}
```

## 注意事项

1. **权限控制**：所有操作方法必须添加 `PowerFilter` 特性
2. **缓存清理**：删除数据时必须清理 Redis 缓存
3. **操作日志**：重要操作必须记录日志
4. **业务验证**：删除前检查业务规则
5. **空值检查**：使用 `IsNull()` 检查实体是否存在

## 相关文档

- **[Database Access](database-access.md)** - DataContext 数据访问
- **[Cache Management](cache-management.md)** - 缓存清理机制
- **[Application Config](application-config.md)** - 中间件配置
- **[Code Conventions](code-conventions.md)** - 编码规范
