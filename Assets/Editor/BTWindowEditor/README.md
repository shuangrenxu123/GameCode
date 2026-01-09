# BT GraphToolkit 行为树（编辑器）

本目录基于 `com.unity.graphtoolkit@0.4.0-exp.2` 实现一套行为树编辑器工作流：

- 编辑阶段：用 GraphToolkit 的 Graph 编辑行为树。
- 导出阶段：导出“仅运行时信息”的 JSON（不包含节点坐标、视口缩放等 Editor 信息）。
- 运行时阶段：使用生成的工厂（switch）+ Builder 将 JSON 构建为运行时 `BTNode` 树，全程不使用反射。

## Create / Open

- 创建图资源：`Assets/Create/EditorGraph/BT Tree Graph`
- 双击 `*.DefaultBTTreeGraph` 在 GraphToolkit 窗口中打开

## Nodes

编辑器里可用的节点类型来源于“运行时节点 + 特性标记”，并由生成器自动生成对应的 GraphToolkit Node：

- 运行时特性定义：
  - `BT.EditorIntegration.BTEditorNodeAttribute`
  - `BT.EditorIntegration.BTEditorConstructorAttribute`
  - `BT.EditorIntegration.BTEditorExposeAttribute`：标记运行时节点上需要在编辑器里可配置并导出到 JSON 的字段/属性（要求 public 或 internal 且可写）
- 生成器入口：`Tools/BT/Regenerate Generated BT Nodes`
- 生成后的编辑器节点目录：`Assets/Editor/BTWindowEditor/Generated/Nodes`

约束：

- 所有输出端口都强制为 `Single`（一个输出端口只能连接一个子节点）。

## JSON Export / Import

Select a `*.DefaultBTTreeGraph` asset in the Project window:

- 导出运行时 JSON：选中图资源 → `Assets/BT Tree Graph/Export Runtime JSON`

## Runtime

导出的 JSON 只包含运行时相关信息：

- `rootId`：根节点 ID
- 节点列表：节点 `typeId`（运行时类型标识）、构造参数 `args`、子节点 `children`

运行时构建（无反射）：

- `BT.RuntimeSerialization.BTTreeRuntimeBuilder.BuildFromJson(json)` → 返回 `BTNode root`
- 实例化由 `BTGeneratedNodeFactory`（生成代码）完成，避免运行时通过反射创建对象

## 常用流程（推荐）

1) 给运行时节点加特性（只加标记不改逻辑）：
   - `[BTEditorNode("Composite/Sequence", BTEditorNodeKind.Composite)]`
   - 在用于实例化的构造函数上加 `[BTEditorConstructor]`
   - 给需要暴露为编辑器可配置参数的成员加 `[BTEditorExpose]`，例如：`[BTEditorExpose("probability")] public int Probability { get; set; }`
2) 执行生成：`Tools/BT/Regenerate Generated BT Nodes`
3) 创建/编辑图：`Assets/Create/EditorGraph/BT Tree Graph`
4) 导出 JSON：`Assets/BT Tree Graph/Export Runtime JSON`
5) 运行时加载 JSON 后调用 `BTTreeRuntimeBuilder.BuildFromJson` 得到 `root` 并执行 `root.Tick()`
