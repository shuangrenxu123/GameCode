using AIBlackboard;
using BT;
using CharacterController;
using Enemy;
using UnityEngine;

namespace BT.Action
{
    public class BTFleeNode : BTAction
    {
        private Enemy.Enemy enemy;
        private CharacterActions characterActions;
        private float minSafeDistance = 3f;

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

            if (!database.TryGetValue("targetTransform", out Transform target) || target == null)
            {
                StopMovement();
                return BTResult.Failed;
            }

            float safeDistance = Mathf.Max(minSafeDistance, database.GetValue(EnemyAIDatabaseKey.FleeSafeDistance, minSafeDistance));
            Vector3 direction = enemy.characterActor.transform.position - target.position;
            float currentDistance = direction.magnitude;

            if (currentDistance >= safeDistance)
            {
                StopMovement();
                return BTResult.Success;
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = enemy.characterActor.transform.forward * -1f;
            }

            // Ensure attack is off
            characterActions.attack.value = false;

            ApplyInput(direction, true);
            return BTResult.Running;
        }

        private void ApplyInput(Vector3 worldDirection, bool run)
        {
            var planar = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize();
            }

            var input = new Vector2(planar.x, planar.z);
            characterActions.movement.value = input;
            characterActions.run.value = run;
        }

        private void StopMovement()
        {
            characterActions.movement.value = Vector2.zero;
            characterActions.run.value = false;
        }
    }
}
