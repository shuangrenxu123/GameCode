# BT GraphToolkit 行为树编辑器

本目录基于 `com.unity.graphtoolkit@0.4.x` 实现一套行为树编辑器工作流：

- 编辑阶段：使用 GraphToolkit 编辑行为树（Graph 资产）。
- 导出阶段：导出“仅运行时信息”的 JSON（不包含节点坐标、视口缩放等 Editor 信息）。
- 运行时阶段：使用生成的工厂与 Builder 将 JSON 构建为运行时 `BTNode` 树（运行时不使用反射创建节点）。

## 创建 / 打开

- 创建图资产：`Assets/Create/EditorGraph/BT Tree Graph`
- 双击 `*.DefaultBTTreeGraph` 在 GraphToolkit 窗口中打开
- 新建图后会自动生成一个 `Root` 根节点（用于标记运行时根的入口）

## 节点（Runtime -> Editor）

编辑器里可用的节点类型来自“运行时节点 + 特性标记”，并由生成器自动生成对应的 GraphToolkit Node 类。

- 特性定义（运行时侧，仅标记不改逻辑）：
  - `BT.EditorIntegration.BTEditorNodeAttribute`
  - `BT.EditorIntegration.BTEditorConstructorAttribute`
  - `BT.EditorIntegration.BTEditorExposeAttribute`：标记需要在编辑器里可配置并导出到 JSON 的字段/属性
  - `BT.EditorIntegration.BTEditorNodeAttribute.ChildCapacity`：声明该节点子节点容量（`None/Single/Multi`）
- 生成器入口：`Tools/BT/Regenerate Generated BT Nodes`
- 生成目录：`Assets/Editor/BTWindowEditor/Generated/Nodes`

### 端口连接规则

- `ChildCapacity = Single`：输出端口只能连接 1 个子节点（会通过 `GraphToolkitPortCapacityUtil` 强制限制）。
- `ChildCapacity = Multi`：输出端口允许连接多个子节点。
- `ChildCapacity = None`：不提供输出端口。

## 黑板（Blackboard）

本编辑器使用 GraphToolkit 的变量（Variables/Blackboard）作为黑板数据源，导出到运行时 JSON 后在运行时构建为 `AIBlackboard.Blackboard`。

当前导出支持的变量类型：

- `int`
- `float`
- `bool`
- `string`

导出格式（`BTTreeRuntimeJson.blackboard`）中只保存：

- `key`：变量名
- `type`：变量类型
- `value`：默认值（字符串形式）

## 导出 Runtime JSON

在 Project 窗口中选中一个 `*.DefaultBTTreeGraph` 资产：

- 导出：`Assets/BT Tree Graph/Export Runtime JSON`

### 自动导出（减少手动操作）

为了减少“手动选择图 -> 导出 -> 选择保存路径 -> 再去拖拽 TextAsset”的易错流程，本工具支持在图保存时自动生成/覆盖 Runtime JSON：

- 每个图资产（`*.DefaultBTTreeGraph`）都可以在 Inspector 中配置“Runtime JSON 导出路径（Assets 内）”。
- 在 GraphToolkit 窗口中编辑完图后，执行 `Ctrl+S` 或 `Save Project` 保存时，会自动导出/覆盖到该路径。
- 若导出内容与已有文件完全一致，则不会重复写入与触发导入（避免无意义的刷新与噪音）。
- 若未配置导出路径，会使用默认路径：与图同目录同名，后缀为 `.runtime.json`。

JSON 内容（仅运行时相关）：

- `rootId`：运行时根节点 ID（来自 Root 节点连接的第一个子节点）
- `nodes[]`：每个节点的 `id/typeId/args/children`
- `blackboard[]`：黑板默认值（可选）

## 运行时构建（无反射创建节点）

- 构建行为树：`BT.RuntimeSerialization.BTTreeRuntimeBuilder.BuildFromJson(json)` -> `BTNode root`
- 同时构建黑板：`BT.RuntimeSerialization.BTTreeRuntimeBuilder.BuildFromJson(json, out Blackboard bb)`
- 节点实例化由生成代码 `BTGeneratedNodeFactory` 完成，避免运行时通过反射创建对象

## 推荐流程

1) 给运行时节点加特性（只做标记）：`[BTEditorNode(...)]`、`[BTEditorConstructor]`、`[BTEditorExpose(...)]`
2) 生成编辑器节点：`Tools/BT/Regenerate Generated BT Nodes`
3) 创建/编辑图：`Assets/Create/EditorGraph/BT Tree Graph`
4) 配置（可选）GraphToolkit Blackboard 变量默认值
5) 导出 JSON：`Assets/BT Tree Graph/Export Runtime JSON`
6) 运行时加载 JSON 并调用 `BTTreeRuntimeBuilder.BuildFromJson(...)` 构建树并执行

## 进一步优化建议（可选）

下面是针对“编辑器导出 JSON -> 运行时构建行为树”链路的一些高收益改进点（不影响现有工作流也可逐步落地）。

### 基于当前使用方式的前提

- JSON 通常只加载一次；后续主要操作的是构建出来的运行时行为树。
- JSON 解析统一使用 `Utf8Json`。
- 多子节点顺序以“可视化布局顺序”为准（而不是连接顺序/创建顺序）。

### 易用性 / 便利性

- 导出前做结构校验并提供可定位报错：Root 未连接/多根、存在环、端口超容量、节点缺失 `IBTRuntimeJsonNode`、重复/空 `NodeId`、未知 `typeId` 等；最好能在窗口里列出问题并支持一键选中定位节点。
- 明确并固化“多子节点顺序”规则：当前多子节点顺序更接近“枚举顺序/连接顺序”，建议改为可预期的规则（例如按可视化布局从上到下/从左到右排序），或提供手动排序 UI，并将顺序信息固化到导出的数据中。
- Root 语义更严格：当 Root 未连接时，当前导出/构建存在“自动找根”的回退路径，容易掩盖配置错误；更推荐直接阻止导出或强提示。
- 参数编辑更友好：可考虑为 `BTEditorExposeAttribute` 增加 `DisplayName/Tooltip/Order/Range/Group` 等元信息，用于更好的 Inspector/Graph 选项展示与排序。

### 运行时转化性能：可能的卡点与优化方向

> 在“每个 JSON 通常只构建一次树”的前提下，性能重点通常不是平均耗时，而是是否在关键帧出现一次性卡顿/GC 峰值。

- 主要成本来源：
  - `Utf8Json` 反序列化产生的对象分配（`string/List/class` 等）。
  - 构建树时的节点实例化、`children/args` 遍历与字符串解析。
- 低风险、高收益的改善：
  - 构建挪出关键帧：在加载界面/预加载阶段构建，或用协程分帧分批构建，避免切场景/开战首帧卡顿。
  - 解析路径收敛：仅保留 `Utf8Json` 单一路径，避免 fallback/异常分支带来的不确定成本。
  - 构建期减少重复扫描：每节点把 `args` 预转换成 `Dictionary<string,string>`（或轻量索引），再给工厂读取，避免多次线性查找。
- 可选更激进的方案：把运行时数据从 JSON 升级为二进制/自定义格式（体积更小、解析更快、分配更少），但通常属于后续阶段（P2/P3）。

### “编辑图 -> 生成 JSON -> 拖拽使用”的体验优化方向

- 把“拖拽对象”从 `*.json` 升级为“运行时行为树资源”：例如 `BTTreeRuntimeAsset`（`ScriptableObject`）或自定义 Importer，让运行时组件直接引用该资产，减少手动管理 json 文件与路径。
- 自动化导出：图保存/生成节点后自动刷新运行时资产（或提供一键 Build），减少“导出-找文件-拖拽”的手工步骤。
- 引用时即校验：在 Inspector 中显示校验结果（Root/环/未知 `typeId`/子节点排序规则等），并支持一键打开图并定位到问题节点。
- 子节点顺序按视觉固化：导出阶段按节点可视化坐标排序，并在导出面板提供规则说明/预览，确保运行时顺序与编辑器观感一致。

### 性能 / 稳定性风险点

- `args` 参数读取的线性扫描：运行时工厂解析参数通常会对 `List<BTArgJson>` 做多次按名查找（每次 O(n)），节点参数多时会放大。可在构建阶段把 `args` 预转换为 `Dictionary<string, string>` 或轻量索引结构（每节点一次），降低总复杂度。
- JSON 解析的双路径与异常开销：当前解析逻辑存在多种解析方式的 fallback；如果某些情况下经常触发异常分支，会有额外开销。建议统一使用一种解析器/格式，避免用 try/catch 作为常规控制流。
- 工厂返回 null 的静默失败：如果某个 `typeId` 未被生成工厂覆盖，可能出现节点创建为 null、子树被悄悄丢弃的情况。建议运行时构建阶段 fail-fast（明确抛错并带 `nodeId/typeId`），同时在导出阶段也校验“所有 typeId 都可被工厂创建”。
- 递归构建深度：极深树可能带来栈深风险；可选方案是改为显式栈的迭代构建或加入最大深度保护。
- `BTTreeRuntimeJson.version` 未参与兼容：建议加版本迁移/兼容策略（至少在不支持的版本时明确报错）。

### 推荐落地顺序（计划）

- P0（稳定性优先）：导出前结构校验 + 运行时 fail-fast（Root/环/未知 typeId/重复 id/容量不匹配等）；同时明确并固化多子节点排序规则。
- P1（体验提效）：把 JSON 的“拖拽引用”升级为运行时资产（`ScriptableObject`/Importer）+ 自动化导出闭环 + 引用时即校验。
- P2（明显性能点）：为 `args` 引入每节点一次性的索引/字典化访问；构建放到预加载或分帧执行；解析路径保持 `Utf8Json` 单一路径。
- P3（演进扩展）：落地 `version` 迁移；扩展 blackboard/args 对更多类型的支持并完善校验（例如 enum、Vector 等）；必要时评估二进制格式。

### 需要提前确认的设计点

- JSON 引用方式：当前“拖拽 JSON”是拖 `TextAsset` 到某个组件字段吗？字段类型是 `TextAsset/string/path`？
- 加载通道：JSON/资产是放在 `Resources`、`StreamingAssets`、Addressables，还是纯文件路径读取？
- 目标平台：是否包含 IL2CPP？（影响 `Utf8Json` 的 AOT/Resolver 配置策略）
