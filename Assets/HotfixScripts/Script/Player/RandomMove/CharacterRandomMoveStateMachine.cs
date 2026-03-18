using Character.Controller.MoveState;
using Character.Controller.State;
using CharacterController;
using PlayerInfo;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace HFSM
{
    public class CharacterRandomMoveStateMachine : StateMachine<ECharacterRandomMoveState>
    {
        public override ECharacterRandomMoveState currentType => ECharacterRandomMoveState.idle;

        public RandomMoveRange randomMoveRange;

       

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("状态机切换为静止状态");
                ChangeState(ECharacterRandomMoveState.idle);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeState(ECharacterRandomMoveState.move);

            }
        }
    }
}
