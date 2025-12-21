using AIBlackboard;
using BT;
using CharacterController;
using Enemy;
using UnityEngine;

namespace BT.Action
{
    /// <summary>
    /// 翻滚节点：触发翻滚动作，方向为远离玩家
    /// </summary>
    public class BTRollNode : BTAction
    {
        private Enemy.Enemy enemy;
        private CharacterActions characterActions;
        private Transform targetTransform;
        private bool rollTriggered;

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            enemy = database.GetValue<EnemyAIDatabaseKey, Enemy.Enemy>(EnemyAIDatabaseKey.EnemyBody);
            characterActions = database.GetValue<string, CharacterActions>("characterActions");
            targetTransform = database.GetValue<string, Transform>("targetTransform", (Transform)null);
            rollTriggered = false;
        }

        protected override BTResult Execute()
        {
            if (enemy == null || characterActions == null)
            {
                ClearRollFlag();
                return BTResult.Failed;
            }

            if (!rollTriggered)
            {
                TriggerRoll();
                rollTriggered = true;
            }

            // 翻滚触发后立即清除标志并返回成功
            ClearRollFlag();
            return BTResult.Success;
        }

        private void TriggerRoll()
        {
            // 计算远离玩家的方向
            Vector3 awayDirection = Vector3.back; // 默认向后
            if (targetTransform != null)
            {
                awayDirection = (enemy.transform.position - targetTransform.position).normalized;
            }

            // 将方向转换为输入
            var planar = Vector3.ProjectOnPlane(awayDirection, Vector3.up);
            if (planar.sqrMagnitude > 0.01f)
            {
                planar.Normalize();
                characterActions.movement.value = new Vector2(planar.x, planar.z);
            }

            // 触发翻滚
            characterActions.roll.value = true;

            // 停止攻击
            characterActions.attack.value = false;
        }

        private void ClearRollFlag()
        {
            database.SetValue(EnemyAIDatabaseKey.ShouldRoll, false);
            rollTriggered = false;
        }
    }
}
