using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using HFSM;
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
        int currentStep = 1;
        int targetStep = 10;
        public override void Enter(StateBaseInput input = null)
        {
            UseGravity = false;
            characterActor.alwaysNotGrounded = true;
            characterActor.SetUpRootMotion(true, RootMotionVelocityType.SetVelocity, true, RootMotionRotationType.SetRotation);

            inMove = true;
            var trigger = characterActor.Triggers[0];
            var ladder = trigger.transform.GetComponentInParent<Ladder>();
            ClipTransition startAnim = null;

            if (trigger.collider3D.gameObject == ladder.topReference.gameObject)
            {
                isLiftFoot = false;
                currentStep = ladder.climbCount;
                startAnim = climbAnimations["us"];
                characterActor.Teleport(ladder.topReference.position);
                characterActor.SetYaw(ladder.topReference.forward);
                targetStep = ladder.climbCount;

            }
            else
            {
                targetStep = ladder.climbCount;
                isLiftFoot = false;
                currentStep = 1;
                startAnim = climbAnimations["ds"];
                characterActor.Teleport(ladder.bottomReference.position);
                characterActor.SetYaw(ladder.bottomReference.forward);

            }
            Animancer.Play(startAnim).Events.OnEnd = () => inMove = false;

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

            if (currentStep == targetStep && YSpeed > 0)
            {
                inMove = true;
                ClipTransition exitAnimation = null;

                if (isLiftFoot)
                {
                    exitAnimation = climbAnimations["ure"];
                }
                else
                {
                    exitAnimation = climbAnimations["ule"];
                }

                if (exitAnimation != null)
                {
                    Animancer.Play(exitAnimation).Events.OnEnd =
                        () => parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                }
            }
            else if (currentStep == 1 && YSpeed < 0)
            {
                inMove = true;
                ClipTransition exitAnimation = null;
                if (isLiftFoot)
                {
                    exitAnimation = climbAnimations["dre"];
                }
                else
                {
                    exitAnimation = climbAnimations["dle"];
                }

                if (exitAnimation != null)
                {
                    Animancer.Play(exitAnimation).Events.OnEnd =
                        () => parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                }
            }
            else
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
                    currentStep++;
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
                    currentStep--;
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
        }

        public override void Exit()
        {
            base.Exit();
            UseGravity = true;
            characterActor.IsKinematic = false;
            characterActor.alwaysNotGrounded = false;
            characterActor.UseRootMotion = false;
            characterActor.ForceGrounded();
            inMove = false;
            characterActor.VerticalVelocity = Vector3.zero;
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
