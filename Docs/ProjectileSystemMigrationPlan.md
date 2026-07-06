# 投射物系统迁移与优化方案

## 一、目标

本方案用于将 `E:\SK2\SK2\Assets\SoulKnight2\Scripts\Game\Fight` 中的投射物系统迁移到当前项目，并在迁移时完成结构优化。

迁移目标不是原样搬运旧系统，而是保留旧系统中成熟的“配置 + 运行时上下文 + 模块 + 规则链”思想，重构为一套核心层不依赖 Unity、具体实现可替换、可测试、可复用的通用投射物系统。

核心要求：

- 放弃旧系统的 Weapon 层级内容。
- 放弃外部表配置、资源 ID 自动加载等内容。
- 保留 ScriptableObject 配置入口。
- 核心层代码与具体实现层代码分开存放。
- 核心层不依赖 UnityEngine、不依赖 MonoBehaviour、不依赖 Physics、不依赖当前项目 CombatEntity。
- 需要依赖其他模块的能力统一通过接口抽象，Unity 或战斗系统只提供接口实现。
- 移除 `IProjectileFrameTerminateRule` 帧级终止规则接口，改为核心内建停止策略。
- 统一检测命中合并逻辑，避免同一目标多个 Collider 导致不同检测模块命中次数不一致。
- 第一版优先保证结构清晰、行为稳定、易验证，不一次性迁移所有旧功能。

## 二、设计原则

### 1. 核心只管规则

核心层只负责投射物运行规则：

- 生成请求。
- 运行时状态。
- 运动计算。
- 命中流程。
- 停止条件。
- 事件输出。

核心层不负责：

- Unity 物理检测。
- GameObject 或 Prefab 生命周期。
- Transform 同步。
- 对象池。
- 伤害、治疗、Buff 结算。
- 编辑器 Gizmos。
- Timeline、技能、AI、网络同步。

### 2. 具体实现只做适配

Unity、战斗系统、技能系统都通过 Adapter 接入核心层。

例如碰撞体检测不写进核心层，而是：

- 核心层定义 `IProjectileQuery`。
- Unity 实现 `UnityProjectilePhysicsQuery`。
- 如果以后有服务器模拟，可以再实现 `ServerProjectileQuery`。

### 3. SO 只做编辑入口

`ProjectileRecipeSO` 只作为 Unity 编辑器配置资产。

运行时不直接读取 SO，而是通过 `BuildSpec()` 或构建器转成纯 C# 的 `ProjectileRecipeSpec`。

这样可以避免运行时依赖 Unity 资产，也能减少反射和 Inspector 数据访问。

### 4. 静态 API 只做薄门面

允许提供静态 API 生成一颗子弹，但静态 API 不应持有复杂业务逻辑。

推荐结构：

- 核心运行实例仍然是 `ProjectileWorld`。
- 静态类 `ProjectileAPI` 只保存当前激活的 `ProjectileWorld` 引用。
- `ProjectileAPI.Spawn(...)` 内部委托给 `ProjectileWorld.Spawn(...)`。

这样既能方便调用，又不会破坏核心层可测试性。

## 三、推荐目录结构

```text
Assets/HotfixScripts/Script/Module/Fight/Projectile/
  Core/
    Config/
    Runtime/
    Spawn/
    Motion/
    Detection/
    Hit/
    Stop/
    Events/

  Authoring/
    ProjectileRecipeSO.cs
    ProjectileRecipeModuleConfigs.cs
    ProjectileRecipeSpecBuilder.cs

  UnityAdapter/
    UnityProjectileRunner.cs
    UnityProjectilePhysicsQuery.cs
    UnityProjectileTransformView.cs
    UnityProjectilePoolAdapter.cs
    UnityProjectileTargetResolver.cs

  CombatAdapter/
    ProjectileDamageResolver.cs
    ProjectileRegenerationResolver.cs
    ProjectileActionPointBridge.cs

  API/
    ProjectileAPI.cs

  Editor/
    ProjectileDetectGizmoDrawer.cs
```

如果后续需要更严格的编译边界，建议拆 asmdef：

```text
Game.Projectile.Core      // 不引用 UnityEngine
Game.Projectile.Authoring // 引用 UnityEngine、Odin、Core
Game.Projectile.Unity     // 引用 UnityEngine、Core
Game.Projectile.Combat    // 引用 Fight、Core、UnityAdapter
```

### 本次实际落地结构

本次迁移先采用“一套目录 + Core 独立 asmdef”的方式落地，方便当前项目接入，同时保留后续继续拆 asmdef 的空间。

```text
Assets/HotfixScripts/Script/Module/Fight/Projectile/
  Core/
    Game.Projectile.Core.asmdef
    API/ProjectileAPI.cs
    Config/ProjectileSpecs.cs
    Detection/ProjectileDetection.cs
    Hit/ProjectileHitProcessing.cs
    Motion/ProjectileMotionModules.cs
    Runtime/ProjectileCoreTypes.cs
    Runtime/ProjectileMath.cs
    Runtime/ProjectileRuntimeState.cs
    Runtime/ProjectileWorld.cs
    Spawn/ProjectileRequests.cs
    Spawn/ProjectileSpawner.cs
    Stop/ProjectileStopPolicy.cs

  Authoring/
    ProjectileRecipeSO.cs
    ProjectileRecipeModuleConfigs.cs
    ProjectileRecipeSpecBuilder.cs

  UnityAdapter/
    UnityProjectileDetectionShapes.cs
    ProjectileUnityConversion.cs
    UnityProjectileRunner.cs
    UnityProjectileServices.cs

  CombatAdapter/
    ProjectileCombatAdapters.cs
```

当前取舍：

- `Core` 已独立为 `Game.Projectile.Core`，并开启 `noEngineReferences`。
- `Authoring`、`UnityAdapter`、`CombatAdapter` 暂时继续随 `Game.Runtime` 编译，后续稳定后可再拆成独立 asmdef。
- `UnityProjectileDetectionShapes.cs` 存放 Unity 实现层的具体检测 Shape，新增检测方式时不需要修改 Core。
- `UnityProjectileServices.cs` 暂时收纳 Physics Query、TargetProvider、PoseWriter、PrefabFactory，第一版减少文件数量；如果继续扩展检测执行逻辑或对象池策略，应拆成独立文件。
- `ProjectileAPI` 放在 `Core/API` 下，因为它只是 `ProjectileWorld` 的薄门面，不依赖 Unity，也不持有具体实现。
- 暂不实现旧系统里的 Weapon、外部表配置、反弹、可受击 Projectile、帧级终止规则。

## 四、核心层模块

### 1. Config

核心配置数据，运行时只读。

建议类型：

```text
ProjectileRecipeSpec
ProjectileMotionSpec
ProjectileDetectionSpec
ProjectileHitSpec
ProjectileStopSpec
ProjectileSpawnSpec
```

职责：

- 保存运行时需要的纯数据。
- 不包含 Unity 引用。
- 不包含 ScriptableObject。
- 不包含 Prefab。
- 不包含 Collider、LayerMask、Transform。

### 2. Runtime

投射物运行时对象和世界管理。

建议类型：

```text
ProjectileWorld
ProjectileRuntime
ProjectileRuntimeState
ProjectileRuntimeContext
ProjectilePose
ProjectileTrace
ProjectileHandle
ProjectileTickResult
```

职责：

- 管理活跃投射物。
- 处理 `Spawn/Tick/Stop`。
- 驱动运动、检测、命中、停止。
- 输出运行事件。

### 3. Spawn

发射与批量生成。

建议类型：

```text
ProjectileFireRequest
ProjectileSpawnRequest
ProjectileSpawnSettings
ProjectileSpawner
ProjectileSpawnContextBuilder
ProjectileRuntimeContextBuilder
```

职责：

- 根据发射请求创建运行时上下文。
- 处理批量发射。
- 处理散射角。
- 处理随机角。
- 生成多个投射物实例。

### 4. Motion

投射物运动模块。

建议接口：

```csharp
public interface IProjectileMotion
{
    void Evaluate(ref ProjectileRuntimeState state, in ProjectileFrameContext context, ref ProjectilePose pose);
}
```

第一版建议实现：

- `LinearMotion`
- `StaticMotion`
- `RoundMotion`
- `WeakHomingMotion`

暂缓实现：

- 复杂曲线弹道。
- 反弹运动。
- 分裂弹。
- 依赖 Unity Transform 的跟随弹。

### 5. Detection

核心层只定义检测抽象，不执行 Unity Physics。

建议接口：

```csharp
public interface IProjectileQuery
{
    int Query(
        in ProjectileQueryRequest request,
        ProjectileRawHit[] results);
}
```

说明：

- `ProjectileQueryRequest` 由核心层生成。
- `ProjectileRawHit` 是外部检测结果的纯数据表达。
- Unity 层把 Collider 检测结果转换成 `ProjectileRawHit`。
- 核心层不需要知道 Collider 是什么。

### 6. Hit

统一命中处理。

建议类型：

```text
ProjectileHitAccumulator
ProjectileHitProcessor
ProjectileHitContext
ProjectileHitResult
IProjectileHitFilter
IProjectileHitResponse
IProjectileHitResolver
```

职责：

- 合并原始命中。
- 按目标去重。
- 保留最近命中。
- 过滤目标。
- 执行命中响应。
- 触发结算接口。
- 输出命中事件。

### 7. Stop

停止策略不再使用 `IProjectileFrameTerminateRule`。

建议类型：

```text
ProjectileStopSpec
ProjectileStopPolicy
ProjectileEndReason
```

内建停止条件：

- 手动停止。
- 生命周期结束。
- 命中目标后销毁。
- 穿透次数耗尽。
- 超出最大距离。
- 锚点失效。
- 目标失效。

反弹相关配置如果第一版不实现，应先不暴露，避免出现旧文档中提到的“配置看起来支持，但运行时不生效”的问题。

## 五、具体实现层模块

### 1. Authoring

负责 Unity 配置资产。

建议文件：

```text
ProjectileRecipeSO.cs
ProjectileRecipeModuleConfigs.cs
ProjectileRecipeSpecBuilder.cs
```

`ProjectileRecipeSO.cs` 职责：

- 作为唯一 ScriptableObject 资产入口。
- 暴露基础配置、运动配置、检测配置、命中配置、停止配置。
- 提供 `BuildSpec()`。

`ProjectileRecipeModuleConfigs.cs` 职责：

- 存放 `[Serializable]` 配置结构。
- 例如运动配置、检测配置、命中配置、停止配置。
- 避免 `ProjectileRecipeSO.cs` 继续膨胀。

所有 Inspector 字段必须：

- 使用 `[SerializeField]`。
- 使用 Odin `LabelText` 添加中文说明。
- 不写运行时获取依赖的保底代码。

### 2. UnityAdapter

负责 Unity 具体能力。

建议类型：

```text
UnityProjectileRunner
UnityProjectilePhysicsQuery
UnityProjectileTransformView
UnityProjectilePoolAdapter
UnityProjectileTargetResolver
```

职责：

- 在 Unity Update、FixedUpdate 或外部手动 Tick 中驱动 `ProjectileWorld.Tick()`。
- 使用 Unity Physics 实现 `IProjectileQuery`。
- 将核心层 `ProjectilePose` 同步到 Transform。
- 将 Prefab 实例接入对象池。
- 将 Collider 转成核心目标 ID。

### 3. CombatAdapter

负责当前项目战斗系统接入。

建议类型：

```text
ProjectileDamageResolver
ProjectileRegenerationResolver
ProjectileActionPointBridge
```

职责：

- 将核心命中事件转换成当前项目的 `DamageAction`。
- 将治疗型投射物转换成 `RegenerationAction`。
- 将投射物发射、命中、停止事件桥接到 `ActionPointManager`。

核心层不直接引用：

- `CombatEntity`
- `DamageAction`
- `RegenerationAction`
- `BuffManager`

## 六、接口适配方案

### 1. 物理检测接口

核心层接口：

```csharp
public interface IProjectileQuery
{
    int Query(
        in ProjectileQueryRequest request,
        ProjectileRawHit[] results);
}
```

Unity 实现：

```csharp
public sealed class UnityProjectilePhysicsQuery : IProjectileQuery
{
    public int Query(
        in ProjectileQueryRequest request,
        ProjectileRawHit[] results)
    {
        // 使用 Physics.RaycastNonAlloc / SphereCastNonAlloc / OverlapSphereNonAlloc
        // 将 Collider 转换为 ProjectileRawHit
    }
}
```

### 2. 目标解析接口

核心层接口：

```csharp
public interface IProjectileTargetResolver
{
    bool TryResolveTarget(object rawTarget, out ProjectileTargetInfo targetInfo);
}
```

Unity 实现：

```csharp
public sealed class UnityProjectileTargetResolver : IProjectileTargetResolver
{
    public bool TryResolveTarget(object rawTarget, out ProjectileTargetInfo targetInfo)
    {
        // rawTarget 可以是 Collider
        // Unity 层查找 CombatEntity 或其他代理组件
    }
}
```

如果担心 `GetComponentInParent` 成为热点，后续可以在 Unity 层加轻量代理组件缓存目标信息。

### 3. 实例创建接口

核心层接口：

```csharp
public interface IProjectileInstanceFactory
{
    ProjectileInstanceHandle Create(in ProjectileInstanceCreateRequest request);
    void Release(ProjectileInstanceHandle handle);
}
```

Unity 实现：

```csharp
public sealed class UnityProjectilePoolAdapter : IProjectileInstanceFactory
{
    public ProjectileInstanceHandle Create(in ProjectileInstanceCreateRequest request)
    {
        // 从当前项目 PoolManager 获取 prefab 实例
    }

    public void Release(ProjectileInstanceHandle handle)
    {
        // 回收到 PoolManager
    }
}
```

### 4. 战斗结算接口

核心层接口：

```csharp
public interface IProjectileCombatResolver
{
    void ResolveHit(in ProjectileHitContext context);
}
```

当前项目实现：

```csharp
public sealed class ProjectileDamageResolver : IProjectileCombatResolver
{
    public void ResolveHit(in ProjectileHitContext context)
    {
        // 调用 CombatActionFactor.CreateActionAndExecute<DamageAction>
    }
}
```

## 七、静态 API 生成子弹

可以提供静态 API，推荐作为便捷门面。

### 推荐写法

```csharp
public static class ProjectileAPI
{
    static ProjectileWorld world;

    public static bool IsInitialized => world != null;

    public static void Initialize(ProjectileWorld projectileWorld)
    {
        world = projectileWorld;
    }

    public static ProjectileHandle Spawn(in ProjectileFireRequest request)
    {
        if (world == null)
        {
            return ProjectileHandle.Invalid;
        }

        return world.Spawn(request);
    }

    public static void Tick(float deltaTime)
    {
        world?.Tick(deltaTime);
    }

    public static void Stop(ProjectileHandle handle, ProjectileEndReason reason)
    {
        world?.Stop(handle, reason);
    }
}
```

### 使用方式

```csharp
ProjectileAPI.Spawn(new ProjectileFireRequest
{
    Recipe = recipeSpec,
    OwnerId = ownerId,
    SpawnPosition = position,
    Direction = direction,
    CanResolveHit = true,
});
```

### 注意事项

静态 API 可以用，但不要让它变成真正的核心系统。

不建议：

- 在静态类中直接访问 Unity Physics。
- 在静态类中直接 Instantiate Prefab。
- 在静态类中直接调用 DamageAction。
- 在静态类中保存大量运行状态。

推荐：

- 静态类只保存 `ProjectileWorld`。
- 静态类只转发调用。
- 真正逻辑仍在核心实例和 Adapter 中。

这样可以兼顾调用方便和结构清晰。

## 八、第一版功能范围

第一版建议实现：

- 单发直线弹。
- 批量散射。
- 静止范围弹。
- 弱追踪弹。
- 球形检测。
- 球射线检测。
- 盒形检测。
- 命中伤害。
- 命中销毁。
- 生命周期结束。
- 对象池回收。
- SO 配置生成。
- 静态 API 便捷生成。

第一版暂缓：

- Weapon 层。
- 外部表 ID 加载。
- 反弹。
- 子弹生成子弹。
- 可受击 Projectile。
- 网络同步。
- 复杂特效规则链。

## 九、迁移步骤

### 阶段 1：核心骨架

实现：

- `ProjectileWorld`
- `ProjectileRuntime`
- `ProjectileRuntimeState`
- `ProjectileRuntimeContext`
- `ProjectilePose`
- `ProjectileTrace`
- `ProjectileHandle`

验证：

- 可以创建投射物。
- 可以 Tick。
- 可以手动 Stop。
- 生命周期能结束。

### 阶段 2：配置与构建

实现：

- `ProjectileRecipeSO`
- `ProjectileRecipeModuleConfigs`
- `ProjectileRecipeSpec`
- `ProjectileRecipeSpecBuilder`

验证：

- SO 可以生成 Spec。
- 运行时不直接读取 SO。
- Inspector 字段都有中文 `LabelText`。

### 阶段 3：生成系统

实现：

- `ProjectileFireRequest`
- `ProjectileSpawnRequest`
- `ProjectileSpawnSettings`
- `ProjectileSpawner`
- `ProjectileSpawnContextBuilder`

验证：

- 单发生成。
- 多发散射。
- 随机角稳定。

### 阶段 4：运动模块

实现：

- `LinearMotion`
- `StaticMotion`
- `RoundMotion`
- `WeakHomingMotion`

验证：

- 每种运动在纯核心测试中能得到正确 Pose。

### 阶段 5：检测适配

实现：

- 核心 `IProjectileQuery`
- Unity `UnityProjectilePhysicsQuery`
- Unity `UnityProjectileTargetResolver`

验证：

- SphereCast 能命中目标。
- OverlapSphere 能命中目标。
- OverlapBox 能命中目标。
- 同一目标多个 Collider 不重复结算。

### 阶段 6：命中处理

实现：

- `ProjectileHitAccumulator`
- `ProjectileHitProcessor`
- `IProjectileHitFilter`
- `IProjectileHitResponse`
- `IProjectileCombatResolver`

验证：

- 命中过滤生效。
- 命中后能停止。
- 穿透计数生效。
- 命中事件只触发一次。

### 阶段 7：Unity 表现与对象池

实现：

- `UnityProjectileRunner`
- `UnityProjectileTransformView`
- `UnityProjectilePoolAdapter`

验证：

- Prefab 能生成。
- Transform 能跟随核心 Pose。
- 停止后能回池。

### 阶段 8：战斗系统接入

实现：

- `ProjectileDamageResolver`
- `ProjectileRegenerationResolver`
- `ProjectileActionPointBridge`

验证：

- 命中敌人能扣血。
- 治疗型投射物能回血。
- 发射、命中事件能被 ActionPoint 监听。

### 阶段 9：静态 API

实现：

- `ProjectileAPI.Initialize`
- `ProjectileAPI.Spawn`
- `ProjectileAPI.Tick`
- `ProjectileAPI.Stop`

验证：

- 可以通过静态方法生成一颗子弹。
- 静态 API 不直接依赖 Unity 具体实现。
- 替换 `ProjectileWorld` 后测试环境仍可运行。

## 十、验证清单

每次阶段完成后至少验证：

- 普通单发子弹能正常发射、移动、停止。
- 多发散射角度正确。
- 生命周期结束后能回收。
- 命中敌人后能扣血。
- 同一目标多个 Collider 不重复扣血。
- 静止范围弹能按配置检测。
- 弱追踪弹能朝目标转向。
- 核心层不引用 UnityEngine。
- SO 字段均有 Odin `LabelText` 中文说明。
- Unity 依赖都在 Adapter 层。
- 静态 API 只做门面，不承载核心逻辑。

## 十一、后续风险

### 1. 静态 API 的全局状态风险

静态 API 很方便，但容易隐藏依赖。

规避方式：

- 只允许保存一个 `ProjectileWorld` 引用。
- 测试或切场景时必须显式 `Initialize` 或 `Shutdown`。
- 不在静态 API 里直接访问 Unity 或战斗系统。

### 2. Unity 物理查询的性能风险

高频子弹下 `GetComponentInParent`、Collider 解析可能进入热点。

规避方式：

- 第一版先保持简单。
- 后续加 Collider 目标代理缓存。
- 或建立 Collider 到目标信息的注册表。

### 3. SO 字段重命名风险

如果后续从旧资源迁移，需要避免随意改字段名。

当前新项目第一版如果没有旧资源兼容压力，可以优先保持命名清晰。

### 4. 反弹半实现风险

旧文档明确指出反弹配置存在半实现风险。

第一版建议不开放反弹配置。需要反弹时，必须同步实现反弹响应和停止优先级。
