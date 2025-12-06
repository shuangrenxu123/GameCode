Utility AI 决策系统设计
1. 背景与目标
现有 AI 框架包含行为树（Behavior Tree）、分层有限状态机（HFSM）、GOAP、HTN，以及统一的 AIBlackboard.Blackboard 数据通道，但缺乏可动态调节优先级的效用评估机制。
引入 Utility AI 的目的：在保持低耦合的前提下，利用 Blackboard 统一读写状态，为行为树节点、HFSM 状态或其他系统提供“哪一个动作当前最有价值”的实时参考。
核心需求：
低耦合 —— Utility 层不依赖具体 BT/HFSM/GOAP 实现，只与 Blackboard 与接口通信。
双配置入口 —— 支持代码手动注册（便于在初始化阶段构建）与 ScriptableObject 资产（便于设计师配置）。
最小反射/高性能 —— 除配置加载阶段外禁止频繁反射；评分过程可缓存与增量更新。
运行期无可视化 —— 仅提供必要的调试日志/事件。
2. 核心接口与管理类
名称	位置	主要职责
UtilityContext	decision/UtilityAI/Core	封装 Blackboard、DeltaTime、AI 实例 Id 等运行参数，作为所有考量/动作的上下文入口。
UtilityScore	decision/UtilityAI/Core	结构体，记录单个动作的综合分值、权重、时间戳；便于排序与缓存。
IUtilityConsideration	decision/UtilityAI/Core	定义 float Evaluate(UtilityContext ctx)，从 Blackboard 读取数据并返回 0~1 的评分，可包含权重与曲线策略。
IUtilityAction	decision/UtilityAI/Core	描述可执行动作：string Name { get; }、float Evaluate(UtilityContext ctx)、void Execute(UtilityContext ctx)；内部可关联 BT 子树或 HFSM 状态。
IUtilityConfigurator	decision/UtilityAI/Core	抽象“配置来源”（代码/ScriptableObject）；负责将配置转化为运行期 UtilityAction 集合。
UtilityAgent	decision/UtilityAI/Core	Utility 系统的调度中心：注册/注销动作、Tick 评分、选择最佳动作、写入 Blackboard，并向外暴露评分结果。
UtilityConfigLoader	decision/UtilityAI/Config	解析 ScriptableObject UtilityProfile，一次性构建 UtilityAction/Consideration 实例后交给 UtilityAgent，保证运行时零反射。
BTUtilitySelector	decision/Behavior Tree/Core/Actions	行为树节点，先从 UtilityAgent 获取排序后的动作列表，再按推荐顺序执行子节点。
UtilityDrivenTransition	decision/HFSM/Base	HFSM 条件，读取 Blackboard 中的“推荐状态/动作”键，以 Utility 输出驱动状态切换。
IUtilityExecutor（可选）	decision/UtilityAI/Core	桥接不同执行域（BT/HFSM/GOAP）；UtilityAgent 只需调用 executor.Execute(actionName, ctx) 即可解耦执行逻辑。
3. 配置与数据流
3.1 手动注册（代码方式）
适用于在行为树/HFSM 初始化时通过代码注入；所有考量读取统一的 Blackboard 键，避免字符串散落。
UtilityAgent.Tick(deltaTime) 后可通过 agent.CurrentBestAction 获取推荐动作。
3.2 ScriptableObject 配置
UtilityProfile : ScriptableObject
List<UtilityActionAsset>（名称、默认权重、执行器标签、可选冷却参数）
List<ConsiderationAsset>（Blackboard Key、参数、曲线、聚合方式）
加载流程：
UtilityProfile.Build() 生成只包含运行时所需数据的 UtilitySpec（纯 POCO）。
UtilityConfigLoader.Apply(spec, agent) 遍历 spec 构造 UtilityAction 与 Consideration，完成注册。
配置更改只需重新 Build；运行中不依赖反射。
3.3 Blackboard 约定
建议新增 BlackboardKeys.cs 集中声明常用键，例如：
UtilityAgent 将推荐动作写入如 BlackboardKeys.UtilityAction，BT/HFSM 适配器据此读取。
对关键值使用 Blackboard.Subscribe 来标记对应 Consideration 脏状态，减少重复计算。
4. 运行流程
感知更新：传感器/BT Service/HFSM State 写入 Blackboard。
Utility Tick：UtilityAgent.Tick(deltaTime)
遍历注册动作 → 调用其 Consideration 集合 → 得到 0~1 分数 → 与动作优先级/冷却叠加 → 输出 UtilityScore。
动作选择：可配置策略（Max、TopK、WeightedRandom），默认取最大值。
结果发布：
写入 Blackboard（如 Utility.CurrentAction、Utility.LastScore）。
通知执行端（BT 适配器/HFSM 条件/自定义系统）。
执行：由具体执行器决定触发 BT 子树、HFSM 状态、GOAP 目标或直接发命令。
5. 行为树 & HFSM 集成示例
行为树：在 BTTree 里注入 UtilityAgent，BTUtilitySelector 调用：
HFSM：在 StateMachine.Update() 前调用 agent.Tick() 并存储结果；UtilityDrivenTransition 通过 Blackboard 读取推荐状态 ID 作为 Transition 条件。
GOAP（可选）：Utility 分数可写入 Goal 优先级或作为 Planner 约束，以便在策略层动态调整。
6. 性能与扩展建议
缓存策略：UtilityAction 维护上次得分与脏标记，只有相关 Blackboard 值变化才重新评估。
对象池化：对 UtilityScore、ConsiderationResult 等临时对象使用 Struct 或 ArrayPool 减少 GC。
批量调度：提供 UtilitySystem.UpdateAllAgents() 将多个 Agent 的 Tick 分帧或集中执行，便于多 AI 同步。
扩展接口：IUtilityScoringCurve 支持自定义曲线（线性、S 型、阶梯、动画曲线等），IUtilityExecutor 可桥接任意系统。
7. 实施阶段建议
核心实现：UtilityContext、UtilityAgent、UtilityAction、Consideration、UtilityScore。
配置支持：UtilityProfile ScriptableObject、UtilityConfigLoader、示例资产。
集成桥接：行为树/HFSM/GOAP 适配器 + 示例场景。
测试与文档：单元测试涵盖评分逻辑、配置解析；补充 README 或 wiki 指引。