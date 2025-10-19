using System;
using Animancer;
using CharacterController;
using Fight;
using UnityEngine;
using UnityEngine.Events;
using static Fight.Number.CombatNumberBox;

namespace Enemy
{
    /// <summary>
    /// Enemy动画类型枚举
    /// </summary>
    public enum EnemyAnimationType
    {
        None,
        Idle,
        Move,
        Attack,
        Hurt,
        Dead
    }

    public class Enemy : MonoBehaviour
    {
        [Header("身体组件")]
        public CharacterActor characterActor;    // 移动器官
        public CombatEntity combatEntity;        // 战斗器官

        [Header("AI系统")]
        [SerializeField] private GameObject brainGo; // 敌人AI大脑引用

        [Header("动画系统")]
        public AnimancerComponent animancer;      // 动画控制器
        public AnimatorHelper animancerHelper;   // 动画助手（复用玩家动画助手）

        IEnemyBrain enemyBrain;

        private void Awake()
        {
            enemyBrain = brainGo.GetComponent<IEnemyBrain>();

            // 初始化动画系统
            InitializeAnimationSystem();

            // 初始化大脑系统
            InitializeBrain();

            InitCombatProperty();
        }

        /// <summary>
        /// 理论上应该可以直接写在CombatEntity里面的，目前先写着
        /// </summary>
        void InitCombatProperty()
        {
            combatEntity.hp.SetMaxValue(100);
            combatEntity.properties.RegisterAttribute(PropertyType.Attack, 10);
            combatEntity.properties.RegisterAttribute(PropertyType.Defense, 10);
            combatEntity.properties.RegisterAttribute(PropertyType.SpeedMultiplier, 100);
            combatEntity.properties.RegisterAttribute(PropertyType.RotationMultiplier, 100);
        }

        private void Update()
        {
            // 使用大脑进行思考
            if (enemyBrain != null)
            {
                enemyBrain.Think();
            }
        }

        /// <summary>
        /// 初始化大脑系统
        /// </summary>
        private void InitializeBrain()
        {
            // 初始化大脑
            if (enemyBrain != null)
            {
                enemyBrain.Initialize(this);
            }
        }

        /// <summary>
        /// 初始化动画系统
        /// </summary>
        private void InitializeAnimationSystem()
        {
            // 初始化动画助手
            animancerHelper = new AnimatorHelper(animancer);
        }


        private void OnDestroy()
        {
            // 清理AI资源
            if (enemyBrain != null)
            {
                enemyBrain.Shutdown();
            }
        }
    }
}