using BT;
using CharacterController;
using Enemy;
using UnityEngine;

namespace BT.Action
{
    public class BTMoveAction : BTAction
    {
        private Transform targetTransform;
        private IEnemyBrain entityBrain;

        public string entityBrainKey = "entityBrain";
        public string targetTransformKey = "targetTransform";
        public float minCheckDistance = 1f;

        public override void Activate(DataBase<string, object> database)
        {
            base.Activate(database);

            entityBrain = database.GetData<IEnemyBrain>(entityBrainKey);
            targetTransform = database.GetData<Transform>(targetTransformKey);
        }
        protected override void Enter()
        {
            base.Enter();
        }

        protected override BTResult Execute()
        {
            var direction = targetTransform.position -
                 entityBrain.characterBrain.transform.position;

            Vector3 inputXZ = Vector3.zero;
            if (direction.sqrMagnitude > minCheckDistance * minCheckDistance)
            {
                inputXZ = Vector3.ProjectOnPlane(direction, Vector3.up);

                inputXZ.Normalize();
                inputXZ.y = inputXZ.z;
                inputXZ.z = 0f;
            }

            entityBrain.characterActions.movement.value = inputXZ;

            return BTResult.Running;
        }
        protected override void Exit()
        {
            base.Exit();
        }
    }
}