using Animancer;
using Character.Controller.State;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterAirMovementState : CharacterMovementStateBase
    {
        public override ECharacterMoveState currentType => ECharacterMoveState.Jump;
        public ClipTransition jumpAnim;
        public ClipTransition downAnim;
        public ClipTransition jumpEndAnim;

        bool isEnd;
        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            return characterActor.PlanarVelocity;
        }
        public override void Enter()
        {
            characterActor.ForceNotGrounded();
            if (characterActions.jump.Started)
            {
                Jump();
            }
            isEnd = false;
        }

        void Jump()
        {
            var JumpDirection = characterActor.Up;
            //移除上一次所带来的跳跃速度加成
            characterActor.Velocity -= Vector3.Project(characterActor.Velocity, JumpDirection);
            characterActor.Velocity += CustomUtilities.Multiply
                (JumpDirection, verticalMovementParameters.jumpSpeed);
            // Animancer.Play(animatorConfig.clipAnimators[jump]);
        }

        public override void FixUpdate()
        {
            base.FixUpdate();

            if (characterActor.IsGrounded && !isEnd)
            {
                isEnd = true;
                Animancer.Play(jumpEndAnim).Events.OnEnd = () =>
                {
                    parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                    characterActor.ForceGrounded();
                };
            }
        }

        public override void PostCharacterSimulation()
        {
            if (!characterActor.IsGrounded)
            {
                if (characterActor.Velocity.y > 0)
                {
                    Animancer.Play(jumpAnim);
                }
                else if (characterActor.Velocity.y < 0)
                {
                    Animancer.Play(downAnim);
                }
            }
        }
    }
}
