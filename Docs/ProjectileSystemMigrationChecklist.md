# 投射物系统迁移执行清单

## 执行约定

- [x] 核心层代码与具体实现层代码分开存放。
- [x] 核心层不引用 `UnityEngine`。
- [x] 依赖其他模块的能力通过接口抽象，具体项目提供实现。
- [x] Unity 物理检测、Transform 同步、对象池、SO 配置、战斗结算都放在适配层。
- [x] 静态 API 只做薄门面，内部委托给 `ProjectileWorld`。
- [x] 完成代码后启动 SubAgent 做 code review。

## 阶段 1：文档与目录

- [x] 创建迁移执行清单文档。
- [x] 创建投射物系统目录结构。
- [x] 创建核心层 asmdef，确保核心层不依赖 Unity。

## 阶段 2：核心层

- [x] 实现核心数学结构。
- [x] 实现投射物配置 Spec。
- [x] 实现运行时状态、姿态、轨迹、Handle。
- [x] 实现 Spawn 请求、Spawn 设置、Spawner。
- [x] 实现 `ProjectileWorld`。
- [x] 实现运动模块。
- [x] 实现检测接口与命中累加器。
- [x] 实现命中过滤、响应、结算接口。
- [x] 实现停止策略。

## 阶段 3：SO 配置层

- [x] 实现 `ProjectileRecipeSO`。
- [x] 实现可序列化配置结构。
- [x] 实现 SO 到 `ProjectileRecipeSpec` 的构建。

## 阶段 4：Unity 适配层

- [x] 实现 Unity 物理检测适配。
- [x] 实现 Unity Transform 表现同步。
- [x] 实现 Unity Runner。
- [x] 实现 Unity 对象池/实例工厂适配。
- [x] 实现 Unity Collider 目标解析。

## 阶段 5：战斗适配与 API

- [x] 实现伤害结算适配。
- [x] 实现治疗结算适配。
- [x] 实现投射物 ActionPoint 桥接预留。
- [x] 实现 `ProjectileAPI` 静态门面。

## 阶段 6：验证

- [x] 检查核心层没有引用 Unity。
- [x] 检查 Unity 暴露字段都有 Odin `LabelText`。
- [x] 检查没有运行时兜底获取序列化依赖。
- [x] 执行可用的编译检查。

验证记录：

- [x] `Core` 层手动 Roslyn 编译通过，输出 `Temp/ProjectileCoreCheck.dll`。
- [x] `Authoring`、`UnityAdapter`、`CombatAdapter` 基于 `ProjectileCoreCheck.dll` 和现有 `Library/ScriptAssemblies` 手动编译通过，输出 `Temp/ProjectileAdapterCheck.dll`。
- [x] `RegenerationAction.cs` 初次补充 `Release()` 后单文件手动编译通过；Review 后治疗事件改动已做源码签名检查，完整程序集验证需等待 Unity/项目构建通过。
- [x] `dotnet build Game.Runtime.csproj -v minimal` 已执行；当前被既有 `UTF8Json` 插件 `IsConstructedGenericType` 二义性错误阻塞，未发现投射物新增代码进入项目级报错。
- [x] `Game.Runtime.csproj` 当前尚未刷新新 asmdef 下的投射物源码，后续需要 Unity Editor 刷新工程文件后再跑一次完整编译。
- [x] Review 修正后重新执行 `Core` 手动编译，通过。
- [x] Review 修正后重新执行 `Authoring`、`UnityAdapter`、`CombatAdapter` 手动编译，通过；仅剩 Unity 序列化字段未赋值警告，符合序列化绑定预期。
- [x] Review 修正后再次执行 `dotnet build Game.Runtime.csproj -v minimal`，仍被既有 `UTF8Json` 插件二义性错误阻塞。

## 阶段 6.5：本轮补充修正

- [x] 去掉弱追踪运动对静态目标服务的依赖，改由 `ProjectileFrameContext.TargetProvider` 传入。
- [x] 命中处理结果携带 `ProjectileHitContext`，避免事件命中索引被 `TotalHitCount++` 偏移。
- [x] 移除命中查询前整数组清零，减少高频 Tick CPU 消耗。
- [x] `ProjectileWorld` 增加 `StopAll`，Runner 关闭时统一停止并回收实例。
- [x] `IProjectileCombatResolver` 在 Unity Runner 中改为可选绑定，允许非战斗用途嵌入。
- [x] 战斗适配层复用单目标列表后主动断开 Action 引用，避免复用列表被已执行 Action 持有。
- [x] 修复现有 `RegenerationAction` 未回收的问题，避免治疗型投射物命中后产生 Action 泄漏。

## 阶段 7：Code Review

- [x] 启动 SubAgent 进行 code review。
- [x] Review 性能与 GC 风险。
- [x] Review 重复逻辑与不合理流程。
- [x] Review 接口抽象与扩展边界。
- [x] 根据 Review 结果修正问题。

Review 修正记录：

- [x] 修复 `ProjectileWorld` 命中缓存不可重入风险，改为 `ProjectileHitWindowBuffer` 租借模型。
- [x] 修复 `ProjectileDetectionSpec.MaxHits` 未生效问题，单次查询缓存容量现在会按配方上限裁剪，并受 Runner 全局缓存上限保护。
- [x] 开放 `ProjectileWorldServices.HitFilters` 与 `HitResponses`，外部可注入命中过滤和响应链。
- [x] `UnityCombatProjectileTargetResolver` 目标缓存增加容量上限，null 结果不再缓存，缓存对象失效后会刷新。
- [x] 将 `CombatAction` 的 `Creator/Target` 清理移动到 `OnRelease()`，投射物战斗适配不再操作已释放 Action。
- [x] `RegenerationAction` 后置事件改为 `PostRestoreHP`，并修正 `ActionPointType.Length = 6`。
- [x] 文档中的 `IProjectileQuery` 示例改为当前实现采用的 `ProjectileRawHit[]`。

Review 后仍需后续实机关注：

- [ ] 运动模块实例当前按配方共享，已在接口上标注“不要在模块字段保存单颗投射物状态”；如果后续需要有状态模块，应引入 per-projectile module state。
- [ ] `ProjectileActionPointBridge` 当前是事件桥接预留，不代表已经接入具体 ActionPoint 行为。
- [ ] 需要 Unity Editor 刷新 asmdef/csproj 后再做一次完整项目编译和 PlayMode 场景验证。

## 阶段 8：检测扩展修正

- [x] 将具体检测类型从 Core 中移出，Core 只保留 `IProjectileDetectionShape` 和 `ProjectileDetectionSpec.Shape` 通道。
- [x] 在 UnityAdapter 中新增具体检测 Shape：`SphereCastProjectileDetectionShape`、`OverlapSphereProjectileDetectionShape`、`OverlapBoxProjectileDetectionShape`、`ConeOverlapProjectileDetectionShape`、`RayFanProjectileDetectionShape`。
- [x] 实现扇形范围检测 `ConeOverlapProjectileDetectionShape`，由 UnityAdapter 使用 `OverlapSphereNonAlloc` 后按夹角过滤。
- [x] 实现射线扇形检测 `RayFanProjectileDetectionShape`，由 UnityAdapter 使用多条 `RaycastNonAlloc` 检测并做同目标去重。
- [x] Authoring 使用 `ProjectileDetectionShapeKind` 作为 Inspector 选项，构建时生成具体 Shape，不再污染 Core。
- [x] 重新执行 Core 手动编译，通过。
- [x] 重新执行 Authoring、UnityAdapter、CombatAdapter 手动编译，通过；仅剩序列化字段未赋值警告。
