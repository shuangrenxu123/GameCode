using AIBlackboard;
using BT.EditorIntegration;
using CharacterController;
using UnityEngine;

namespace BT.Nodes
{
    [BTEditorNode("CharacterAction/Move", BTEditorNodeKind.Action)]
    public class BTCharacterMoveNode : BTAction
    {
        [BTEditorExpose]
        public Vector2 vector2Value = Vector2.zero;

        CharacterBrain brain;
        CharacterActions inputAction => brain.CharacterActions;

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            brain = database.GetValue<string, CharacterBrain>("brain", null);
        }

        protected override void Enter()
        {
            base.Enter();
            if (brain == null)
            {
                return;
            }

            inputAction.movement.value = vector2Value;
        }

        protected override BTResult Execute()
        {
            return brain == null ? BTResult.Failed : BTResult.Success;
        }
    }
}
