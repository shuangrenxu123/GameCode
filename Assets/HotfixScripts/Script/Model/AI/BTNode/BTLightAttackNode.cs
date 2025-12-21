using AIBlackboard;
using BT;
using Enemy;
using UnityEngine;

namespace BT.Action
{
    /// <summary>
    /// 轻攻击节点：使用characterActions.attack触发轻攻击
    /// </summary>
    public class BTLightAttackNode : BTAction
    {
        private IEnemyBrain entityBrain;

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            entityBrain = database.GetValue<string, IEnemyBrain>("entityBrain");
        }

        protected override BTResult Execute()
        {
            if (entityBrain == null)
            {
                Debug.LogError("[轻攻击] 实体大脑为空！");
                return BTResult.Failed;
            }

            bool playerVisible = database.GetValue(EnemyAIDatabaseKey.PlayerVisible, false);
            if (!playerVisible)
            {
                ResetAttackState();
                Debug.Log("[轻攻击] 目标不可见，停止攻击");
                return BTResult.Failed;
            }

            float playerDistance = database.GetValue(EnemyAIDatabaseKey.PlayerDistance, float.MaxValue);
            float attackRange = database.GetValue(EnemyAIDatabaseKey.AttackRange, 2f);
            if (playerDistance > attackRange)
            {
                ResetAttackState();
                Debug.Log($"[轻攻击] 超出攻击距离，当前: {playerDistance:F2}，阈值: {attackRange:F2}");
                return BTResult.Failed;
            }

            Debug.Log("[轻攻击] 执行轻攻击");

            // 停止移动
            entityBrain.characterActions.movement.value = UnityEngine.Vector2.zero;
            entityBrain.characterActions.run.value = false;

            // 触发轻攻击
            entityBrain.characterActions.attack.value = true;

            return BTResult.Success;
        }

        void ResetAttackState()
        {
            if (entityBrain?.characterActions != null)
            {
                entityBrain.characterActions.attack.value = false;
            }
        }
    }
}
