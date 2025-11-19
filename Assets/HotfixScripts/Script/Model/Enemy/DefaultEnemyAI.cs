using AIBlackboard;
using BT;
using Character.Player;
using CharacterController;
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
        CharacterBrain _characterBrain;
        public CharacterBrain characterBrain => _characterBrain;

        public CharacterActions characterActions { get; set; } = new();


        /// <summary>
        /// 初始化大脑，注入身体引用
        /// </summary>
        /// <param name="enemyBody">敌人的身体组件</param>
        public void Initialize(Enemy enemyBody)
        {
            body = enemyBody;

            // 初始化行为树数据库
            var database = new Blackboard();
            database.SetValue(EnemyAIDatabaseKey.CharacterActor.ToString(), body.characterActor);
            database.SetValue(EnemyAIDatabaseKey.CombatEntity.ToString(), body.combatEntity);
            database.SetValue(EnemyAIDatabaseKey.Transform.ToString(), body.transform);
            database.SetValue(EnemyAIDatabaseKey.EnemyBody.ToString(), body);

            database.SetValue("entityBrain", this);
            database.SetValue("targetTransform", player.transform);

            // 创建行为树大脑
            behaviorTree = new EnemyBT();
            behaviorTree.Init(database);
        }

        /// <summary>
        /// 思考过程（每帧调用）
        /// </summary>
        public void Think()
        {
            // 执行行为树逻辑
            behaviorTree.Update();
        }

        /// <summary>
        /// 关闭大脑
        /// </summary>
        public void Shutdown()
        {
            // 清理资源
            behaviorTree = null;
        }
    }
}