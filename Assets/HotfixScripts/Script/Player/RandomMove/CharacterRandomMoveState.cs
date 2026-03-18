using Character.Controller.State;
using UnityEngine;
using UnityEngine.UIElements;

namespace HFSM
{
    public class CharacterRandomMoveState : StateBase<ECharacterRandomMoveState>
    {
        public override ECharacterRandomMoveState currentType => ECharacterRandomMoveState.move;

        public RandomMoveRange randomMoveRange;

        public float moveSpeed;

        public override void Enter(StateBaseInput input = null)
        {
            base.Enter(input); 
            Debug.Log("进入了移动状态！");
            moveSpeed = 3f;
        }

        public override void Update()
        {
            base.Update();

            Debug.Log("移动");
            
            Vector3 dir = (randomMoveRange.currentTarget - randomMoveRange.transform.position).normalized;
            dir.y = 0; 

            randomMoveRange.transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        }

        public override void Exit()
        {
            base.Exit();
            moveSpeed = 0;
            Debug.Log("退出了移动状态！");
        }
    }
}
