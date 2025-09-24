using Animancer;
using Character.Controller.State;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterAirMovementState : CharacterMovementStateBase
    {
        enum AirState
        {
            Fall,
            Upward

        }
        public override ECharacterMoveState currentType => ECharacterMoveState.Jump;
        public ClipTransition jumpAnim;
        public ClipTransition downAnim;
        public ClipTransition jumpEndAnim;
        AirState currentAirState;
        bool isEnd;
        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            return characterActor.PlanarVelocity;
        }
        public override void Enter()
        {
            // characterActor.alwaysNotGrounded = true;
            characterActor.ForceNotGrounded(10);
            currentAirState = AirState.Upward;
            if (characterActions.jump.Started)
            {
                Jump();
                currentAirState = AirState.Fall;
            }
            isEnd = false;
        }

        protected override void HandleRotation(float dt)
        {

        }

        void Jump()
        {
            var JumpDirection = characterActor.Up;
            //移除上一次所带来的跳跃速度加成
            characterActor.Velocity -= Vector3.Project(characterActor.Velocity, JumpDirection);
            characterActor.Velocity += CustomUtilities.Multiply
                (JumpDirection, verticalMovementParameters.jumpSpeed);
        }

        public override void FixUpdate()
        {
            base.FixUpdate();
            if (characterActor.IsGrounded && !isEnd)
            {
                isEnd = true;
                characterActor.PlanarVelocity = Vector3.zero;
                Animancer.Play(jumpEndAnim).Events.OnEnd = () =>
                {
                    parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                    characterActor.ForceGrounded();
                    UseGravity = true;
                };
            }
        }

        public override void PostCharacterSimulation()
        {
            if (!characterActor.IsGrounded)
            {
                if (characterActor.Velocity.y > 0 && currentAirState != AirState.Upward)
                {
                    currentAirState = AirState.Upward;
                    Animancer.Play(jumpAnim);
                }
                else if (characterActor.Velocity.y < 0 && currentAirState != AirState.Fall)
                {
                    currentAirState = AirState.Fall;
                    Animancer.Play(downAnim);
                }
            }
        }
        public override void RefreshAnimator()
        {

        }
    }
}
