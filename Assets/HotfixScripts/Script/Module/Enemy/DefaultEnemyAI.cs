using AIBlackboard;
using BT;
using Character.AI.Sensor;
using Character.Controller;
using Character.Player;
using CharacterController;
using Fight;
using UnityEngine;

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
        // UtilityBrain utilityBrain;
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

            // 创建行为树大脑
            behaviorTree = new EnemyBT();
            behaviorTree.Init(database);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                characterActions.jump.value = true;
            }
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
            // utilityBrain?.Stop();
        }

        void SetupSensors()
        {
            if (sensorManager == null)
            {
                sensorManager = GetComponentInChildren<SensorManager>();
            }

            sensorManager?.BindBlackboard(database);
        }

        static BlackboardKey<TValue> CreateKey<TValue>(EnemyAIDatabaseKey key)
        {
            return new BlackboardKey<TValue>(key.GetHashCode(), typeof(EnemyAIDatabaseKey));
        }
    }
}