using System;
using Animancer;
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
        public CCAnimatorConfig animatorConfig;  // 使用现有的动画配置

        [Header("动画控制")]
        [SerializeField] private EnemyAnimationController _animationController; // 动画控制器实例

        IEnemyBrain enemyBrain;

        private void Awake()
        {
            enemyBrain = brainGo.GetComponent<IEnemyBrain>();

            // 初始化动画系统
            InitializeAnimationSystem();

            // 初始化大脑系统
            InitializeBrain();
        }

        private void Start()
        {
            // 初始化战斗属性
            combatEntity.hp.SetMaxValue(100);
            combatEntity.properties.RegisterAttribute(PropertyType.Attack, 10);
            combatEntity.properties.RegisterAttribute(PropertyType.Defense, 10);
            combatEntity.properties.RegisterAttribute(PropertyType.SpeedMultiplier, 10);

            // 初始化完成
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

            // 初始化动画控制器
            if (_animationController == null)
            {
                _animationController = new EnemyAnimationController();
                _animationController.Initialize(this, animancer, characterActor, animatorConfig);

            }
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