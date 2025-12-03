using AIBlackboard;
using UnityEngine;
using UtilityAI.Integration;

namespace UtilityAI.Examples
{
    /// <summary>
    /// NPC效用AI示例 - 演示如何将效用AI与现有系统集成
    /// </summary>
    public class NPCUtilityAI : UtilityAIComponent
    {
        [Header("NPC引用")]
        [SerializeField] private Transform selfTransform;

        // 黑板键定义
        public static readonly BlackboardKey<float> HealthKey = "npc_health";
        public static readonly BlackboardKey<float> MaxHealthKey = "npc_max_health";
        public static readonly BlackboardKey<bool> HasTargetKey = "has_target";
        public static readonly BlackboardKey<Transform> TargetTransformKey = "target_transform";
        public static readonly BlackboardKey<Transform> SelfTransformKey = "self_transform";
        public static readonly BlackboardKey<float> ThreatLevelKey = "threat_level";
        public static readonly BlackboardKey<bool> IsInCombatKey = "is_in_combat";
        public static readonly BlackboardKey<Vector3> PatrolPointKey = "patrol_point";
        public static readonly BlackboardKey<float> LastDamageTimeKey = "last_damage_time";

        protected override void Awake()
        {
            base.Awake();

            // 初始化黑板数据
            Blackboard.SetValue(SelfTransformKey, selfTransform ?? transform);
            Blackboard.SetValue(HealthKey, 100f);
            Blackboard.SetValue(MaxHealthKey, 100f);
            Blackboard.SetValue(HasTargetKey, false);
            Blackboard.SetValue(IsInCombatKey, false);
            Blackboard.SetValue(ThreatLevelKey, 0f);
        }

        protected override void SetupOptions()
        {
            // ========== 1. 巡逻选项 ==========
            var patrolOption = new Option("Patrol", new PatrolAction())
            {
                baseWeight = 0.3f // 基础权重较低
            };
            patrolOption
                .AddConsideration(new BoolConsideration("NoTarget", HasTargetKey, false)) // 没有目标时
                .AddConsideration(new BoolConsideration("NotInCombat", IsInCombatKey, false)); // 不在战斗中

            // ========== 2. 追击选项 ==========
            var chaseOption = new Option("Chase", new ChaseAction())
            {
                baseWeight = 0.7f
            };
            chaseOption
                .AddConsideration(new BoolConsideration("HasTarget", HasTargetKey, true))
                .AddConsideration(new TransformDistanceConsideration(
                    "TargetDistance",
                    SelfTransformKey,
                    TargetTransformKey,
                    ResponseCurve.InverseLinear(5f, 30f))) // 距离5-30米时追击
                .AddConsideration(new HealthConsideration(
                    "SufficientHealth",
                    HealthKey,
                    100f,
                    ResponseCurve.Linear())); // 血量越高越积极追击

            // ========== 3. 攻击选项 ==========
            var attackOption = new Option("Attack", new AttackAction())
            {
                baseWeight = 1.0f,
                cooldown = 1.5f // 攻击冷却
            };
            attackOption
                .AddConsideration(new BoolConsideration("HasTarget", HasTargetKey, true))
                .AddConsideration(new TransformDistanceConsideration(
                    "InAttackRange",
                    SelfTransformKey,
                    TargetTransformKey,
                    ResponseCurve.Sigmoid(2f, 0f, 5f))) // 5米内攻击
                .AddConsideration(new HealthConsideration(
                    "Health",
                    HealthKey,
                    100f,
                    ResponseCurve.Exponential(0.5f))); // 血量影响攻击意愿

            // ========== 4. 逃跑选项 ==========
            var fleeOption = new Option("Flee", new FleeAction())
            {
                baseWeight = 0.8f
            };
            fleeOption
                .AddConsideration(new HealthConsideration(
                    "LowHealth",
                    HealthKey,
                    100f,
                    ResponseCurve.InverseLinear(0f, 0.3f))) // 30%以下血量时逃跑意愿高
                .AddConsideration(new FuncConsideration(
                    "ThreatLevel",
                    bb => bb.GetValue(ThreatLevelKey, 0f))); // 威胁等级

            // ========== 5. 待机选项 ==========
            var idleOption = new Option("Idle", new IdleAction())
            {
                baseWeight = 0.1f // 最低优先级
            };
            idleOption.AddConsideration(new ConstantConsideration("Always", 1f));

            // 添加所有选项
            AddOption(idleOption);
            AddOption(patrolOption);
            AddOption(chaseOption);
            AddOption(attackOption);
            AddOption(fleeOption);
        }

        protected override void OnOptionChangedCallback(Option oldOption, Option newOption)
        {
            Debug.Log($"[NPC AI] 决策变化: {oldOption?.name ?? "None"} -> {newOption?.name}");
        }

        // ============ 辅助方法 ============

        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(Transform target)
        {
            Blackboard.SetValue(TargetTransformKey, target);
            Blackboard.SetValue(HasTargetKey, target != null);
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            var currentHealth = Blackboard.GetValue(HealthKey, 100f);
            Blackboard.SetValue(HealthKey, Mathf.Max(0, currentHealth - damage));
            Blackboard.SetValue(LastDamageTimeKey, Time.time);
            Blackboard.SetValue(IsInCombatKey, true);

            // 触发立即决策
            ForceDecision();
        }

        /// <summary>
        /// 更新威胁等级
        /// </summary>
        public void UpdateThreatLevel(float level)
        {
            Blackboard.SetValue(ThreatLevelKey, Mathf.Clamp01(level));
        }
    }

    // ============ 示例动作实现 ============

    /// <summary>
    /// 巡逻动作
    /// </summary>
    public class PatrolAction : ContinuousAction
    {
        public override string Name => "Patrol";

        private Vector3 targetPoint;
        private float arriveDistance = 1f;

        public override void Enter(Blackboard blackboard)
        {
            base.Enter(blackboard);

            // 获取巡逻点
            if (blackboard.TryGetValue(NPCUtilityAI.PatrolPointKey, out Vector3 point))
            {
                targetPoint = point;
            }
            else
            {
                // 生成随机巡逻点
                targetPoint = GetRandomPatrolPoint(blackboard);
                blackboard.SetValue(NPCUtilityAI.PatrolPointKey, targetPoint);
            }

            Debug.Log($"[Patrol] 开始巡逻到 {targetPoint}");
        }

        protected override ActionState OnUpdate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(NPCUtilityAI.SelfTransformKey, out Transform self))
            {
                return ActionState.Failed;
            }

            float distance = Vector3.Distance(self.position, targetPoint);

            if (distance < arriveDistance)
            {
                // 到达目标点，生成新的巡逻点
                targetPoint = GetRandomPatrolPoint(blackboard);
                blackboard.SetValue(NPCUtilityAI.PatrolPointKey, targetPoint);
            }

            // 这里可以调用移动逻辑
            // MoveTowards(targetPoint);

            return ActionState.Running;
        }

        private Vector3 GetRandomPatrolPoint(Blackboard blackboard)
        {
            if (blackboard.TryGetValue(NPCUtilityAI.SelfTransformKey, out Transform self))
            {
                return self.position + Random.insideUnitSphere * 10f;
            }
            return Vector3.zero;
        }
    }

    /// <summary>
    /// 追击动作
    /// </summary>
    public class ChaseAction : ContinuousAction
    {
        public override string Name => "Chase";

        public override void Enter(Blackboard blackboard)
        {
            base.Enter(blackboard);
            Debug.Log("[Chase] 开始追击目标");
        }

        protected override ActionState OnUpdate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(NPCUtilityAI.TargetTransformKey, out Transform target) ||
                target == null)
            {
                return ActionState.Failed;
            }

            // 这里可以调用移动逻辑追踪目标
            // MoveTowards(target.position);

            return ActionState.Running;
        }
    }

    /// <summary>
    /// 攻击动作
    /// </summary>
    public class AttackAction : ActionBase
    {
        public override string Name => "Attack";

        private float attackDuration = 0.5f;
        private float startTime;

        public override void Enter(Blackboard blackboard)
        {
            base.Enter(blackboard);
            startTime = Time.time;
            Debug.Log("[Attack] 执行攻击!");

            // 这里可以触发攻击动画、伤害计算等
        }

        public override ActionState Execute(Blackboard blackboard)
        {
            if (Time.time - startTime >= attackDuration)
            {
                return ActionState.Success;
            }
            return ActionState.Running;
        }
    }

    /// <summary>
    /// 逃跑动作
    /// </summary>
    public class FleeAction : ContinuousAction
    {
        public override string Name => "Flee";

        private Vector3 fleeDirection;

        public override void Enter(Blackboard blackboard)
        {
            base.Enter(blackboard);

            // 计算逃跑方向（远离威胁）
            if (blackboard.TryGetValue(NPCUtilityAI.SelfTransformKey, out Transform self) &&
                blackboard.TryGetValue(NPCUtilityAI.TargetTransformKey, out Transform target) &&
                target != null)
            {
                fleeDirection = (self.position - target.position).normalized;
            }
            else
            {
                fleeDirection = Random.onUnitSphere;
                fleeDirection.y = 0;
            }

            Debug.Log("[Flee] 开始逃跑!");
        }

        protected override ActionState OnUpdate(Blackboard blackboard)
        {
            // 这里可以调用移动逻辑
            // MoveInDirection(fleeDirection);

            // 检查是否安全了
            if (blackboard.TryGetValue(NPCUtilityAI.HealthKey, out float health) && health > 50f)
            {
                return ActionState.Success;
            }

            return ActionState.Running;
        }
    }
}
