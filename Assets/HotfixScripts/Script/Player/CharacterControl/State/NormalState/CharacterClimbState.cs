using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterClimbState : CharacterMovementStateBase
    {
        /// <summary>
        /// key - ur-向上右脚 ul 向上左脚
        ///       dr-向下右脚 dl 向下左脚
        ///       
        /// </summary>
        public Dictionary<string, ClipTransition> climbAnimations;
        public override ECharacterMoveState currentType => ECharacterMoveState.Climb;

        bool inMove = false;
        float YSpeed;
        bool isLiftFoot;
        public override void Enter()
        {
            UseGravity = false;
            characterActor.alwaysNotGrounded = true;
            characterActor.SetUpRootMotion(true, RootMotionVelocityType.SetVelocity, false);
            Animancer.Play(climbAnimations["ds"]);
        }

        public override void FixUpdate()
        {
            base.FixUpdate();
            YSpeed = characterBrain.CharacterActions.movement.value.y;
            if (YSpeed != 0 && !inMove)
            {
                PlayAnimation();
            }
        }
        protected override void HandleRotation(float dt)
        {

        }

        void PlayAnimation()
        {
            ClipTransition nextAnim = null;
            if (YSpeed > 0)
            {
                if (isLiftFoot)
                {
                    nextAnim = climbAnimations["ur"];
                }
                else
                {
                    nextAnim = climbAnimations["ul"];
                }
            }
            else if (YSpeed < 0)
            {
                if (isLiftFoot)
                {
                    nextAnim = climbAnimations["dr"];
                }
                else
                {
                    nextAnim = climbAnimations["dl"];
                }
            }
            if (nextAnim != null)
            {

                inMove = true;
                //交换当前的脚
                isLiftFoot = !isLiftFoot;
                Animancer.Play(nextAnim).Events.OnEnd = PlayAnimation;
            }
            else
            {
                inMove = false;
            }
        }

        public override void Exit()
        {
            base.Exit();
            UseGravity = true;
            characterActor.alwaysNotGrounded = false;


        }

        public override void PreCharacterSimulation()
        {
        }
        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            return characterActor.VerticalVelocity;
        }

        public override void PostCharacterSimulation()
        {
        }
    }
}
