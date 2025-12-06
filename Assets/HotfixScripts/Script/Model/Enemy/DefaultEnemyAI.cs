using AIBlackboard;
using BT;
using Character.AI.Sensor;
using Character.Controller;
using Character.Player;
using CharacterController;
using Enemy.AI.Utility;
using UnityEngine;
using UtilityAI;

namespace Enemy.AI
{
    public class DefaultEnemyAI : MonoBehaviour, IEnemyBrain
    {
        private Enemy body;
        private EnemyBT behaviorTree;
        [SerializeField]
        Player player;

        [SerializeField]
        SensorManager sensorManager;

        [SerializeField]
        EnemyPerceptionSensor perceptionSensor;

        [SerializeField]
        NPCStateMgr npcStateMgr;

        [SerializeField]
        CharacterBrain _characterBrain;
        public CharacterBrain characterBrain => _characterBrain;

        public CharacterActions characterActions { get; set; } = new();

        [SerializeField, Range(1f, 30f)]
        float patrolRadius = 8f;

        [SerializeField, Range(0.05f, 1f)]
        float lowHealthThreshold = 0.35f;

        [SerializeField, Range(1f, 40f)]
        float fleeSafeDistance = 12f;

        [SerializeField, Range(1f, 50f)]
        float detectionRange = 15f;

        [SerializeField, Range(0.5f, 5f)]
        float attackRange = 2f;

        [SerializeField, Range(0.05f, 1f)]
        float utilityDecisionInterval = 0.2f;

        [SerializeField]
        bool allowUtilityInterrupt = true;

        [SerializeField, Range(1f, 3f)]
        float interruptThreshold = 1.2f;

        Blackboard database;
        UtilityBrain utilityBrain;
        bool actionsInitialized;


        /// <summary>
        /// 初始化大脑，注入身体引用
        /// </summary>
        /// <param name="enemyBody">敌人的身体组件</param>
        public void Initialize(Enemy enemyBody)
        {
            body = enemyBody;

            if (_characterBrain == null)
            {
                _characterBrain = body.GetComponentInChildren<CharacterBrain>();
            }

            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }

            if (!actionsInitialized)
            {
                characterActions.InitializeActions();
                actionsInitialized = true;
            }
            characterActions.Reset();

            database = new Blackboard();

            // 初始化行为树数据库
            database.SetValue(EnemyAIDatabaseKey.CharacterActor, body.characterActor);
            database.SetValue(EnemyAIDatabaseKey.CombatEntity, body.combatEntity);
            database.SetValue(EnemyAIDatabaseKey.Transform, body.transform);
            database.SetValue(EnemyAIDatabaseKey.EnemyBody, body);
            database.SetValue(EnemyAIDatabaseKey.PatrolOrigin, body.transform.position);
            database.SetValue(EnemyAIDatabaseKey.PatrolRadius, patrolRadius);
            database.SetValue(EnemyAIDatabaseKey.FleeSafeDistance, fleeSafeDistance);
            database.SetValue(EnemyAIDatabaseKey.LowHealthThreshold, lowHealthThreshold);
            database.SetValue(EnemyAIDatabaseKey.DetectionRange, detectionRange);
            database.SetValue(EnemyAIDatabaseKey.AttackRange, attackRange);

            database.SetValue("entityBrain", this);
            database.SetValue("targetTransform", player.transform);
            database.SetValue("characterBrain", _characterBrain);
            database.SetValue("characterActions", characterActions);

            npcStateMgr?.SetStateMachineData("aiBlackboard", database);

            SetupSensors();
            SetupUtilityAI();
            UpdateSensorSnapshot();

            // 创建行为树大脑
            behaviorTree = new EnemyBT();
            behaviorTree.Init(database);
        }

        /// <summary>
        /// 思考过程（每帧调用）
        /// </summary>
        public void Think()
        {
            if (database == null)
            {
                return;
            }

            UpdateSensorSnapshot();

            // 执行行为树逻辑
            behaviorTree?.Update();
        }

        /// <summary>
        /// 关闭大脑
        /// </summary>
        public void Shutdown()
        {
            // 清理资源
            behaviorTree = null;
            utilityBrain?.Stop();
        }

        void SetupSensors()
        {
            if (sensorManager == null)
            {
                sensorManager = GetComponentInChildren<SensorManager>();
            }

            sensorManager?.BindBlackboard(database);

            if (perceptionSensor == null)
            {
                perceptionSensor = sensorManager?.GetComponentInChildren<EnemyPerceptionSensor>();
            }

            if (perceptionSensor != null && player != null)
            {
                perceptionSensor.AssignTarget(player.transform);
                perceptionSensor.ConfigureRange(detectionRange);
                perceptionSensor.ForceSample();
            }
        }

        void SetupUtilityAI()
        {
            if (body == null || _characterBrain == null)
            {
                Debug.LogWarning("Utility AI setup skipped because body or character brain is missing.", this);
                return;
            }

            utilityBrain = new UtilityBrain(database)
            {
                DecisionInterval = utilityDecisionInterval,
                AllowInterrupt = allowUtilityInterrupt,
                InterruptThreshold = interruptThreshold
            };

            // 1. Patrol (Default behavior)
            // Weight: 0.3
            // Condition: Player NOT visible
            var patrolOption = new Option("Patrol")
            {
                baseWeight = 0.3f
            };
            patrolOption.AddConsideration(new BoolConsideration("NoEnemy", EnemyUtilityBlackboardKeys.PlayerVisible, false));

            // 2. Chase
            // Weight: 0.5
            // Condition: Player Visible AND Far
            var chaseOption = new Option("Chase")
            {
                baseWeight = 0.5f
            };
            chaseOption
                .AddConsideration(new BoolConsideration("HasEnemy", EnemyUtilityBlackboardKeys.PlayerVisible, true))
                .AddConsideration(new PlayerDistanceConsideration("PlayerFar", EnemyUtilityBlackboardKeys.PlayerDistance, attackRange, false));

            // 3. Attack (Aggressive)
            // Weight: 0.7 (Base) -> Boosted by Low Health
            // Condition: Player Visible AND Close
            var attackOption = new Option("Attack")
            {
                baseWeight = 0.7f
            };
            attackOption
                .AddConsideration(new BoolConsideration("HasEnemy", EnemyUtilityBlackboardKeys.PlayerVisible, true))
                .AddConsideration(new PlayerDistanceConsideration("PlayerClose", EnemyUtilityBlackboardKeys.PlayerDistance, attackRange, true))
                // Aggressive: Health lower -> Score higher
                // Curve: Linear, Slope -0.5, Offset 1.0 => 1.0 at 0 health, 0.5 at 1 health
                .AddConsideration(new CombatEntityHealthConsideration("Aggression", EnemyUtilityBlackboardKeys.CombatEntity,
                    new ResponseCurve { curveType = CurveType.Linear, slope = -0.5f, yOffset = 1.0f }));

            utilityBrain.AddOption(patrolOption);
            utilityBrain.AddOption(chaseOption);
            utilityBrain.AddOption(attackOption);
            utilityBrain.Start();

            database.SetValue(EnemyAIDatabaseKey.UtilityBrain, utilityBrain);
        }

        void UpdateSensorSnapshot()
        {
            if (perceptionSensor != null)
            {
                return;
            }

            if (player == null || body == null)
            {
                return;
            }

            float distance = Vector3.Distance(player.transform.position, body.transform.position);
            database.SetValue(EnemyAIDatabaseKey.PlayerDistance, distance);
            bool visible = distance <= detectionRange;
            database.SetValue(EnemyAIDatabaseKey.PlayerVisible, visible);
        }
    }
}