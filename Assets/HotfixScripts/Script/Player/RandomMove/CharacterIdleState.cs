using Character.Controller.State;
using UnityEditorInternal;
using UnityEngine;

namespace HFSM
{
    public class CharacterIdleState : StateBase<ECharacterRandomMoveState>
    {
        public override ECharacterRandomMoveState currentType => ECharacterRandomMoveState.idle;

        public RandomMoveRange randomMoveRange;

        public float idleDuration = 1f;
        public float idleTimer;

        public override void Enter(StateBaseInput input = null)
        {
            base.Enter(input);
            Debug.Log("进入了静止状态！");
            idleTimer = idleDuration;
        }

        public override void Update()
        {
            base.Update();
            Debug.Log("静止");
            // 倒计时
            idleTimer -= Time.deltaTime;
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log("退出了静止状态！"); 
        }
    }
}
