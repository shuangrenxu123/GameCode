using AIBlackboard;
using BT;
using CharacterController;
using Enemy;
using UnityEngine;

namespace BT.Action
{
    public class BTPatrolNode : BTAction
    {
        private Enemy.Enemy enemy;
        private CharacterActions characterActions;
        private float arrivalThreshold = 0.5f;

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

            var destination = GetOrCreateDestination();
            Vector3 direction = destination - enemy.characterActor.transform.position;

            if (direction.magnitude <= arrivalThreshold)
            {
                GenerateNewDestination();
                StopMovement();
                return BTResult.Success;
            }

            // Ensure attack is off
            characterActions.attack.value = false;

            ApplyInput(direction, false);
            return BTResult.Running;
        }

        private Vector3 GetOrCreateDestination()
        {
            if (!database.TryGetValue(EnemyAIDatabaseKey.PatrolDestination, out Vector3 destination))
            {
                destination = GenerateNewDestination();
            }
            return destination;
        }

        private Vector3 GenerateNewDestination()
        {
            var origin = database.GetValue(EnemyAIDatabaseKey.PatrolOrigin, enemy.transform.position);
            var radius = Mathf.Max(1f, database.GetValue(EnemyAIDatabaseKey.PatrolRadius, 5f));
            var random = Random.insideUnitCircle * radius;
            var destination = origin + new Vector3(random.x, 0f, random.y);
            database.SetValue(EnemyAIDatabaseKey.PatrolDestination, destination);
            return destination;
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
