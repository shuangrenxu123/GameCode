using AIBlackboard;
using BT;
using Character.AI.Sensor;
using Character.Controller;
using Character.Player;
using CharacterController;
using Enemy.AI.Utility;
using Fight;
using UnityEngine;
using UtilityAI;

namespace Enemy.AI
{
    /// <summary>
    /// 轻巡者AI：
    /// 1. 无敌人时以0.5倍速度围绕初始位置巡逻
    /// 2. 发现敌人后追击并使用轻攻击
    /// 3. 受击时按概率触发翻滚
    /// </summary>
    public class PatrolSkirmisherAI : MonoBehaviour, IEnemyBrain
    {
        private Enemy body;
        private PatrolSkirmisherBT behaviorTree;

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

        [Header("巡逻设置")]
        [SerializeField, Range(1f, 30f), Tooltip("巡逻半径")]
        float patrolRadius = 8f;

        [SerializeField, Range(0.1f, 1f), Tooltip("巡逻时的速度倍率")]
        float patrolSpeedMultiplier = 0.5f;

        [Header("战斗设置")]
        [SerializeField, Range(1f, 50f), Tooltip("感知范围")]
        float detectionRange = 15f;

        [SerializeField, Range(0.5f, 5f), Tooltip("轻攻击触发距离")]
        float attackRange = 2f;

        [SerializeField, Range(0f, 1f), Tooltip("受击翻滚概率")]
        float rollChanceOnHit = 0.35f;

        [Header("Utility AI 设置")]
        [SerializeField, Range(0.05f, 1f)]
        float utilityDecisionInterval = 0.2f;

        [SerializeField]
        bool allowUtilityInterrupt = true;

        [SerializeField, Range(1f, 3f)]
        float interruptThreshold = 1.2f;

        Blackboard database;
        UtilityBrain utilityBrain;
        bool actionsInitialized;

        // 受击翻滚相关
        bool pendingRoll;

        /// <summary>
        /// 初始化大脑，注入身体引用
        /// </summary>
        public void Initialize(Enemy enemyBody)
        {
            body = enemyBody;

            if (_characterBrain == null)
            {
                _characterBrain = body.GetComponentInChildren<CharacterBrain>();
            }

            if (player == null)
            {
                player = FindFirstObjectByType<Player>();
                if (player == null)
                {
                    Debug.LogError("[轻巡者AI] 无法找到玩家对象！");
                }
                else
                {
                    Debug.Log("[轻巡者AI] 找到玩家对象: " + player.name);
                }
            }

            if (!actionsInitialized)
            {
                characterActions.InitializeActions();
                actionsInitialized = true;
            }
            characterActions.Reset();

            database = new Blackboard();

            // 初始化黑板数据（同步 EnemyAIDatabaseKey 与 UtilityBlackboardKey）
            database.SetValue(EnemyAIDatabaseKey.CharacterActor, body.characterActor);
            database.SetValue(EnemyAIDatabaseKey.CombatEntity, body.combatEntity);
            database.SetValue(EnemyAIDatabaseKey.Transform, body.transform);
            database.SetValue(EnemyAIDatabaseKey.EnemyBody, body);
            database.SetValue(EnemyAIDatabaseKey.PatrolOrigin, body.transform.position);
            database.SetValue(EnemyAIDatabaseKey.PatrolRadius, patrolRadius);
            database.SetValue(EnemyAIDatabaseKey.PatrolSpeedMultiplier, patrolSpeedMultiplier);
            database.SetValue(EnemyAIDatabaseKey.DetectionRange, detectionRange);
            database.SetValue(EnemyAIDatabaseKey.AttackRange, attackRange);
            database.SetValue(EnemyAIDatabaseKey.RollChanceOnHit, rollChanceOnHit);
            database.SetValue(EnemyAIDatabaseKey.ShouldRoll, false);

            database.SetValue<string, IEnemyBrain>("entityBrain", this);
            database.SetValue("targetTransform", player?.transform);
            database.SetValue("characterBrain", _characterBrain);
            database.SetValue("characterActions", characterActions);

            Debug.Log($"[轻巡者AI] 初始化完成 - 玩家: {player?.name}, 感知范围: {detectionRange}");

            npcStateMgr?.SetStateMachineData("aiBlackboard", database);

            SetupSensors();
            SetupUtilityAI();
            UpdateSensorSnapshot();

            // 注册受击事件
            RegisterDamageCallback();

            // 创建行为树
            behaviorTree = new PatrolSkirmisherBT();
            behaviorTree.Init(database);
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.J))
            {
                characterActions.attack.value = true;
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                characterActions.jump.value = true;
            }
            Think();
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
            ProcessPendingRoll();

            // 添加调试信息
            bool playerVisible = database.GetValue(EnemyAIDatabaseKey.PlayerVisible, false);
            float playerDistance = database.GetValue(EnemyAIDatabaseKey.PlayerDistance, float.MaxValue);

            // 检查各个条件
            bool shouldPatrol = !playerVisible;
            bool shouldChase = playerVisible && playerDistance > attackRange;
            bool shouldAttack = playerVisible && playerDistance <= attackRange;

            // 执行行为树逻辑
            behaviorTree?.Update();
        }

        /// <summary>
        /// 关闭大脑
        /// </summary>
        public void Shutdown()
        {
            UnregisterDamageCallback();
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
                Debug.Log($"[轻巡者AI] 传感器配置完成，范围: {detectionRange}");
            }
            else
            {
                Debug.LogWarning($"[轻巡者AI] 未找到感知传感器或玩家为空，将使用手动检测");
            }
        }

        void SetupUtilityAI()
        {
            if (body == null || _characterBrain == null)
            {
                Debug.LogWarning("效用AI设置跳过，因为身体或角色大脑组件缺失", this);
                return;
            }

            var playerVisibleKey = CreateKey<bool>(EnemyAIDatabaseKey.PlayerVisible);
            var playerDistanceKey = CreateKey<float>(EnemyAIDatabaseKey.PlayerDistance);
            var combatEntityKey = CreateKey<CombatEntity>(EnemyAIDatabaseKey.CombatEntity);

            utilityBrain = new UtilityBrain(database)
            {
                DecisionInterval = utilityDecisionInterval,
                AllowInterrupt = allowUtilityInterrupt,
                InterruptThreshold = interruptThreshold
            };

            // 1. Patrol - 无敌人时巡逻
            var patrolOption = new Option("Patrol")
            {
                baseWeight = 0.3f
            };
            patrolOption.AddConsideration(new BoolConsideration("NoEnemy", playerVisibleKey, false));

            // 2. Chase - 发现敌人且距离较远时追击
            var chaseOption = new Option("Chase")
            {
                baseWeight = 0.5f
            };
            chaseOption
                .AddConsideration(new BoolConsideration("HasEnemy", playerVisibleKey, true))
                .AddConsideration(new PlayerDistanceConsideration("PlayerFar", playerDistanceKey, attackRange, false));

            // 3. LightAttack - 发现敌人且距离足够近时轻攻击
            var lightAttackOption = new Option("LightAttack")
            {
                baseWeight = 0.7f
            };
            lightAttackOption
                .AddConsideration(new BoolConsideration("HasEnemy", playerVisibleKey, true))
                .AddConsideration(new PlayerDistanceConsideration("PlayerClose", playerDistanceKey, attackRange, true));

            utilityBrain.AddOption(patrolOption);
            utilityBrain.AddOption(chaseOption);
            utilityBrain.AddOption(lightAttackOption);
            utilityBrain.Start();

            database.SetValue(EnemyAIDatabaseKey.UtilityBrain, utilityBrain);
        }

        void UpdateSensorSnapshot()
        {
            if (player == null || body == null)
            {
                Debug.LogWarning("[轻巡者AI] 玩家或身体对象为空！");
                return;
            }

            // 手动更新感知数据（无论是否有传感器，都要确保数据正确）
            float distance = Vector3.Distance(player.transform.position, body.transform.position);
            bool visible = distance <= detectionRange;


            // 更新黑板数据（双写）
            database.SetValue(EnemyAIDatabaseKey.PlayerDistance, distance);
            database.SetValue(EnemyAIDatabaseKey.PlayerVisible, visible);

            // 验证黑板数据是否正确写入
            bool readBackVisible = database.GetValue(EnemyAIDatabaseKey.PlayerVisible, false);
            float readBackDistance = database.GetValue(EnemyAIDatabaseKey.PlayerDistance, float.MaxValue);


            // 如果有传感器，让它也更新（但我们不依赖它）
            if (perceptionSensor != null)
            {
                perceptionSensor.ForceSample();
            }
        }

        static BlackboardKey<TValue> CreateKey<TValue>(EnemyAIDatabaseKey key)
        {
            return new BlackboardKey<TValue>(key.GetHashCode(), typeof(EnemyAIDatabaseKey));
        }

        #region 受击翻滚逻辑

        void RegisterDamageCallback()
        {
            if (body?.combatEntity?.hp != null)
            {
                body.combatEntity.hp.OnHit += OnDamageTaken;
            }
        }

        void UnregisterDamageCallback()
        {
            if (body?.combatEntity?.hp != null)
            {
                body.combatEntity.hp.OnHit -= OnDamageTaken;
            }
        }

        void OnDamageTaken()
        {
            // 按概率决定是否翻滚
            float chance = database.GetValue(EnemyAIDatabaseKey.RollChanceOnHit, 0.35f);
            if (Random.value < chance)
            {
                pendingRoll = true;
            }
        }

        void ProcessPendingRoll()
        {
            if (pendingRoll)
            {
                database.SetValue(EnemyAIDatabaseKey.ShouldRoll, true);
                pendingRoll = false;
            }
        }

        #endregion
    }
}
