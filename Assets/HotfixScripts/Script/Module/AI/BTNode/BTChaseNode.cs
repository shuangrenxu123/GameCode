using AIBlackboard;
using BT;
using CharacterController;
using Enemy;
using UnityEngine;

namespace BT.Action
{
    public class BTChaseNode : BTAction
    {
        private Enemy.Enemy enemy;
        private CharacterActions characterActions;
        private float stopDistance = 1.5f;

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            enemy = database.GetValue<EnemyAIDatabaseKey, Enemy.Enemy>(EnemyAIDatabaseKey.EnemyBody);
            characterActions = database.GetValue<string, CharacterActions>("characterActions");
        }

        protected override BTResult Execute()
        {
            if (enemy == null || characterActions == null)
            {
                return BTResult.Failed;
            }

            bool playerVisible = database.GetValue(EnemyAIDatabaseKey.PlayerVisible, false);
            if (!playerVisible)
            {
                StopMovement();
                characterActions.attack.value = false;
                Debug.Log("[追击] 目标超出感知范围，切回巡逻");
                return BTResult.Failed;
            }

            if (!database.TryGetValue("targetTransform", out Transform target) || target == null)
            {
                StopMovement();
                return BTResult.Failed;
            }

            Vector3 direction = target.position - enemy.characterActor.transform.position;
            float currentDistance = direction.magnitude;

            if (currentDistance <= stopDistance)
            {
                StopMovement();
                return BTResult.Success;
            }

            // Ensure attack is off
            characterActions.attack.value = false;

            ApplyInput(direction);
            return BTResult.Running;
        }

        private void ApplyInput(Vector3 worldDirection)
        {
            var planar = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize();
            }

            var input = new Vector2(planar.x, planar.z) * 0.65f; // 追击时保持行走速度
            characterActions.movement.value = input;
            characterActions.run.value = false;
        }

        private void StopMovement()
        {
            characterActions.movement.value = Vector2.zero;
            characterActions.run.value = false;
        }
    }
}
