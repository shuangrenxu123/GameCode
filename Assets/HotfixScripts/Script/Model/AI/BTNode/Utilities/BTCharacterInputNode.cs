using AIBlackboard;
using BT.EditorIntegration;
using CharacterController;
using UnityEngine;

namespace BT.Nodes
{
    [BTEditorNode("CharacterAction", BTEditorNodeKind.Action)]
    public class BTCharacterInputNode : BTAction
    {
        public enum ActionType
        {
            Jump,
            Run,
            Interact,
            Roll,
            Lock,
            Attack,
            HeavyAttack,
            Crouch,
            OpenUI,
            OpenConsoleUI,
            Movement
        }

        [BTEditorExpose]
        public ActionType actionType = ActionType.Run;

        [BTEditorExpose]
        public bool boolValue = true;

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

            switch (actionType)
            {
                case ActionType.Jump:
                    inputAction.jump.value = boolValue;
                    break;
                case ActionType.Run:
                    inputAction.run.value = boolValue;
                    break;
                case ActionType.Interact:
                    inputAction.interact.value = boolValue;
                    break;
                case ActionType.Roll:
                    inputAction.roll.value = boolValue;
                    break;
                case ActionType.Lock:
                    inputAction.@lock.value = boolValue;
                    break;
                case ActionType.Attack:
                    inputAction.attack.value = boolValue;
                    break;
                case ActionType.HeavyAttack:
                    inputAction.heavyAttack.value = boolValue;
                    break;
                case ActionType.Crouch:
                    inputAction.crouch.value = boolValue;
                    break;
                case ActionType.OpenUI:
                    inputAction.OpenUI.value = boolValue;
                    break;
                case ActionType.OpenConsoleUI:
                    inputAction.OpenConsoleUI.value = boolValue;
                    break;
                case ActionType.Movement:
                    inputAction.movement.value = vector2Value;
                    break;
            }
        }
        protected override BTResult Execute()
        {
            return brain == null ? BTResult.Failed : BTResult.Success;
        }
    }
}
