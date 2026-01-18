using AIBlackboard;
using BT.EditorIntegration;
using CharacterController;
using UnityEngine;

namespace BT.Nodes
{
    [BTEditorNode("Action/MoveToPosition", BTEditorNodeKind.Action)]
    public class BTCharacterMoveToPosition : BTAction
    {
        [BTEditorExpose]
        public string positionKey = "targetPosition";

        private CharacterBrain brain;
        private CharacterActions inputAction => brain.CharacterActions;

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            brain = database.GetValue<string, CharacterBrain>("brain", null);
        }

        protected override BTResult Execute()
        {
            if (brain == null)
            {
                return BTResult.Failed;
            }

            if (string.IsNullOrEmpty(positionKey) || !database.TryGetValue(positionKey, out Vector3 targetPosition))
            {
                inputAction.movement.value = Vector2.zero;
                return BTResult.Failed;
            }

            Vector3 direction = targetPosition - brain.transform.position;
            var planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize();
            }

            var input = new Vector2(planar.x, planar.z);
            inputAction.movement.value = input;
            return BTResult.Running;
        }
    }
}
