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

        public override void Activate(DataBase<string, object> database)
        {
            base.Activate(database);

            entityBrain = database.GetData<IEnemyBrain>("entityBrain");
        }
        protected override void Enter()
        {
            base.Enter();
        }

        protected override BTResult Execute()
        {
            var direction = Vector2.one;

            Vector3 inputXZ = Vector3.ProjectOnPlane(direction, Vector3.up);
            inputXZ.Normalize();
            inputXZ.y = inputXZ.z;
            inputXZ.z = 0f;
            var actions = entityBrain.characterActions;

            actions.movement.value = inputXZ;

            entityBrain.characterActions = actions;

            return BTResult.Running;
        }
        protected override void Exit()
        {
            base.Exit();
        }
    }
}