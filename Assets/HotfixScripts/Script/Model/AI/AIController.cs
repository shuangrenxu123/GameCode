using AIBlackboard;
using Character.AI.Sensor;
using Character.Controller;
using Sirenix.OdinInspector;
using UnityEngine;
using UtilityAI;
using UtilityAI.Integration;

namespace Character.AI
{
    /// <summary>
    /// AI控制器 - 整合效用AI、传感器和状态机
    /// 这是一个完整的AI大脑，展示如何将各系统整合在一起
    /// </summary>
    public class AIController : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private SensorManager sensorManager;
        [SerializeField] private NPCStateMgr stateMgr;

        [Header("效用AI设置")]
        [SerializeField] private float decisionInterval = 0.5f;
        [SerializeField] private bool allowInterrupt = true;

        [Header("调试")]
        [SerializeField, ReadOnly] private string currentDecision = "None";
        [SerializeField] private bool enableDebug = false;

        private UtilityBrain brain;
        private Blackboard blackboard;

        // 黑板键
        public static readonly BlackboardKey<float> HealthKey = "ai_health";
        public static readonly BlackboardKey<float> MaxHealthKey = "ai_max_health";
        public static readonly BlackboardKey<bool> HasEnemyKey = "has_enemy";
        public static readonly BlackboardKey<Transform> EnemyTransformKey = "enemy_transform";
        public static readonly BlackboardKey<float> DistanceToEnemyKey = "distance_to_enemy";
        public static readonly BlackboardKey<bool> CanSeeEnemyKey = "can_see_enemy";
        public static readonly BlackboardKey<float> LastSeenEnemyTimeKey = "last_seen_enemy_time";
        public static readonly BlackboardKey<Vector3> LastKnownEnemyPosKey = "last_known_enemy_pos";
        public static readonly BlackboardKey<float> AlertLevelKey = "alert_level"; // 0-1
        public static readonly BlackboardKey<bool> IsHurtKey = "is_hurt";

        private void Awake()
        {
            blackboard = new Blackboard();
            InitializeBrain();
        }

        private void Start()
        {
            // 共享黑板给状态机
            if (stateMgr != null)
            {
                stateMgr.SetStateMachineData("utilityBrain", brain);
            }

            // 初始化传感器共享同一个黑板
            // 注意：需要修改SensorManager支持外部注入Blackboard

            brain.Start();
        }

        private void Update()
        {
            UpdateBlackboardFromSensors();
            brain.Update();
            currentDecision = brain.CurrentOption?.name ?? "None";
        }

        private void InitializeBrain()
        {
            brain = new UtilityBrain(blackboard, new HighestScoreSelector());
            brain.DecisionInterval = decisionInterval;
            brain.AllowInterrupt = allowInterrupt;

            SetupOptions();

            brain.OnOptionChanged += OnDecisionChanged;
        }

        private void SetupOptions()
        {
            // ========== 空闲/待机 ==========
            var idleOption = new Option("Idle", new LambdaAction("Idle"))
            {
                baseWeight = 0.1f
            };
            idleOption.AddConsideration(new ConstantConsideration("Default", 1f));

            // ========== 巡逻 ==========
            var patrolOption = new Option("Patrol", CreatePatrolAction())
            {
                baseWeight = 0.3f
            };
            patrolOption
                .AddConsideration(new BoolConsideration("NoEnemy", HasEnemyKey, false))
                .AddConsideration(new FuncConsideration("LowAlert",
                    bb => 1f - bb.GetValue(AlertLevelKey, 0f)));

            // ========== 调查 ==========
            var investigateOption = new Option("Investigate", CreateInvestigateAction())
            {
                baseWeight = 0.5f
            };
            investigateOption
                .AddConsideration(new FuncConsideration("AlertLevel",
                    bb => bb.GetValue(AlertLevelKey, 0f)))
                .AddConsideration(new BoolConsideration("CantSeeEnemy", CanSeeEnemyKey, false))
                .AddConsideration(new ExistsConsideration<Vector3>("HasLastKnownPos", LastKnownEnemyPosKey));

            // ========== 追击 ==========
            var chaseOption = new Option("Chase", CreateChaseAction())
            {
                baseWeight = 0.7f
            };
            chaseOption
                .AddConsideration(new BoolConsideration("HasEnemy", HasEnemyKey, true))
                .AddConsideration(new BoolConsideration("CanSee", CanSeeEnemyKey, true))
                .AddConsideration(new FuncConsideration("Distance",
                    bb =>
                    {
                        float dist = bb.GetValue(DistanceToEnemyKey, 100f);
                        // 距离3-20米时追击
                        if (dist < 3f) return 0.2f; // 太近了，应该攻击
                        if (dist > 20f) return 0.3f; // 太远了
                        return 1f;
                    }))
                .AddConsideration(new HealthConsideration("Health", HealthKey, 100f,
                    ResponseCurve.Linear())); // 血量影响追击意愿

            // ========== 攻击 ==========
            var attackOption = new Option("Attack", CreateAttackAction())
            {
                baseWeight = 1.0f,
                cooldown = 0.5f
            };
            attackOption
                .AddConsideration(new BoolConsideration("HasEnemy", HasEnemyKey, true))
                .AddConsideration(new FuncConsideration("InRange",
                    bb =>
                    {
                        float dist = bb.GetValue(DistanceToEnemyKey, 100f);
                        return dist < 3f ? 1f : 0f; // 3米内才能攻击
                    }))
                .AddConsideration(new BoolConsideration("CanSee", CanSeeEnemyKey, true));

            // ========== 防御/格挡 ==========
            var defendOption = new Option("Defend", CreateDefendAction())
            {
                baseWeight = 0.8f
            };
            defendOption
                .AddConsideration(new BoolConsideration("IsHurt", IsHurtKey, true))
                .AddConsideration(new HealthConsideration("LowHealth", HealthKey, 100f,
                    ResponseCurve.InverseLinear())) // 血量越低防御意愿越高
                .AddConsideration(new FuncConsideration("EnemyClose",
                    bb => bb.GetValue(DistanceToEnemyKey, 100f) < 5f ? 1f : 0.3f));

            // ========== 撤退 ==========
            var retreatOption = new Option("Retreat", CreateRetreatAction())
            {
                baseWeight = 0.6f
            };
            retreatOption
                .AddConsideration(new HealthConsideration("CriticalHealth", HealthKey, 100f,
                    new ResponseCurve
                    {
                        curveType = CurveType.Step,
                        slope = 0.2f, // 20%以下触发
                        invert = true
                    }));

            // 添加所有选项
            brain.AddOption(idleOption);
            brain.AddOption(patrolOption);
            brain.AddOption(investigateOption);
            brain.AddOption(chaseOption);
            brain.AddOption(attackOption);
            brain.AddOption(defendOption);
            brain.AddOption(retreatOption);
        }

        /// <summary>
        /// 从传感器更新黑板数据
        /// </summary>
        private void UpdateBlackboardFromSensors()
        {
            // 这里应该从传感器获取数据更新黑板
            // 示例：
            // if (sightSensor.HasVisibleTarget)
            // {
            //     blackboard.SetValue(HasEnemyKey, true);
            //     blackboard.SetValue(EnemyTransformKey, sightSensor.Target);
            //     blackboard.SetValue(DistanceToEnemyKey, sightSensor.DistanceToTarget);
            //     blackboard.SetValue(CanSeeEnemyKey, true);
            //     blackboard.SetValue(LastSeenEnemyTimeKey, Time.time);
            //     blackboard.SetValue(LastKnownEnemyPosKey, sightSensor.Target.position);
            // }
        }

        private void OnDecisionChanged(Option oldOption, Option newOption)
        {
            if (enableDebug)
            {
                Debug.Log($"[AI] 决策: {oldOption?.name ?? "None"} -> {newOption?.name}");
            }

            // 可以在这里触发状态机切换或其他逻辑
        }

        #region 动作工厂方法

        private IAction CreatePatrolAction()
        {
            return new LambdaAction(
                "Patrol",
                enter: bb => Debug.Log("[AI] 开始巡逻"),
                execute: bb => ActionState.Running,
                exit: bb => Debug.Log("[AI] 停止巡逻")
            );
        }

        private IAction CreateInvestigateAction()
        {
            return new LambdaAction(
                "Investigate",
                enter: bb =>
                {
                    if (bb.TryGetValue(LastKnownEnemyPosKey, out Vector3 pos))
                    {
                        Debug.Log($"[AI] 调查位置: {pos}");
                    }
                },
                execute: bb => ActionState.Running
            );
        }

        private IAction CreateChaseAction()
        {
            return new LambdaAction(
                "Chase",
                enter: bb => Debug.Log("[AI] 开始追击"),
                execute: bb => ActionState.Running
            );
        }

        private IAction CreateAttackAction()
        {
            float attackTime = 0;
            return new LambdaAction(
                "Attack",
                enter: bb =>
                {
                    attackTime = Time.time;
                    Debug.Log("[AI] 攻击!");
                },
                execute: bb =>
                {
                    // 攻击持续0.5秒
                    if (Time.time - attackTime > 0.5f)
                        return ActionState.Success;
                    return ActionState.Running;
                }
            );
        }

        private IAction CreateDefendAction()
        {
            return new TimedAction(
                "Defend",
                duration: 1f,
                onStart: bb => Debug.Log("[AI] 防御姿态"),
                onComplete: bb =>
                {
                    bb.SetValue(IsHurtKey, false);
                }
            );
        }

        private IAction CreateRetreatAction()
        {
            return new LambdaAction(
                "Retreat",
                enter: bb => Debug.Log("[AI] 撤退!"),
                execute: bb => ActionState.Running
            );
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 设置敌人目标
        /// </summary>
        public void SetEnemy(Transform enemy)
        {
            blackboard.SetValue(HasEnemyKey, enemy != null);
            blackboard.SetValue(EnemyTransformKey, enemy);
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.position);
                blackboard.SetValue(DistanceToEnemyKey, dist);
            }
        }

        /// <summary>
        /// 触发警觉
        /// </summary>
        public void Alert(float level, Vector3 sourcePosition)
        {
            float current = blackboard.GetValue(AlertLevelKey, 0f);
            blackboard.SetValue(AlertLevelKey, Mathf.Max(current, level));
            blackboard.SetValue(LastKnownEnemyPosKey, sourcePosition);
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void OnDamaged(float damage)
        {
            float health = blackboard.GetValue(HealthKey, 100f);
            blackboard.SetValue(HealthKey, health - damage);
            blackboard.SetValue(IsHurtKey, true);
            brain.ForceDecision(); // 立即重新决策
        }

        #endregion
    }
}
