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
        public void Start()
        {
            AddState(new CharacterRandomMoveState());
            AddState(new CharacterIdleState());
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKey(KeyCode.A))
            {
                Debug.Log("状态机切换为静止状态");
                ChangeState(ECharacterRandomMoveState.idle);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Debug.Log("状态机切换为移动状态");
                ChangeState(ECharacterRandomMoveState.move);
            }
            //else
            //{
            //    Debug.Log("状态机没在动");
            //}
        }
    }
}
