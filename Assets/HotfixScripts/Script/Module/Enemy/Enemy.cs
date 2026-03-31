using System;
using Animancer;
using Character.Controller;
using CharacterController;
using Fight;
using Fight.Number;
using UnityEngine;
using UnityEngine.Events;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [Header("身体组件")]
        public CharacterActor characterActor;    // 移动器官
        public CombatEntity combatEntity;        // 战斗器官

        [Header("AI系统")]
        [SerializeField] private GameObject brainGo; // 敌人AI大脑引用

        [Header("动画系统")]
        public AnimancerComponent animancer;      // 动画控制器

        [SerializeField]
        private NPCStateMgr NPCStateMgr;
        IEnemyBrain enemyBrain;

        private void Awake()
        {
            enemyBrain = brainGo.GetComponent<IEnemyBrain>();
        }
        void Start()
        {
            // 初始化大脑系统
            InitializeBrain();

            InitCombatProperty();

        }

        /// <summary>
        /// 理论上应该可以直接写在CombatEntity里面的，目前先写着
        /// </summary>
        void InitCombatProperty()
        {
            combatEntity.properties.BeginBatch();
            combatEntity.properties.RegisterProperty(PropertyType.MaxHp, 100, 1, 999999);
            combatEntity.properties.RegisterProperty(PropertyType.Attack, 10, 0, 999999);
            combatEntity.properties.RegisterProperty(PropertyType.Defense, 10, 0, 999999);
            combatEntity.properties.RegisterProperty(PropertyType.SpeedMultiplier, 100, 0, 10000);
            combatEntity.properties.RegisterProperty(PropertyType.RotationMultiplier, 100, 0, 10000);
            combatEntity.properties.EndBatch();
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
