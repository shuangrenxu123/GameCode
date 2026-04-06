using Character.Controller.State;
using CharacterController;
using HFSM;
using UnityEngine;
using static CharacterController.LookingDirectionParameters;

namespace Character.Controller.MoveState
{
    public class CharacterRunMovementState : CharacterMovementStateBase
    {
        RunMoveParameters runMoveParameters = new RunMoveParameters();

        public override ECharacterMoveState currentType => ECharacterMoveState.RunMove;


        public override void Enter(StateBaseInput input = null)
        {
            base.Enter(input);
            lookingDirectionParameters.lookingDirectionMode = LookingDirectionMode.Movement;
        }

        public override void Update()
        {
            base.Update();
            base.Update();
            if (runMoveParameters.runInputMode == InputMode.Hold)
            {
                if (characterActions.run.value == false)
                {
                    parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                }
            }
            else
            {
                if (characterActions.run.Started)
                {
                    parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            lookingDirectionParameters.lookingDirectionMode = LookingDirectionMode.ExternalReference;
        }

        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            if (!TryGetMoveSpeed(out float moveSpeed))
            {
                currentPlanarSpeedLimit = 0f;
                return Vector3.zero;
            }

            float speedMultiplier = materialControl == null ?
                1f : materialControl.CurrentSurface.speedMultiplier
                * materialControl.CurrentVolume.speedMultiplier;

            Vector3 targetPlanarVelocity;

            currentPlanarSpeedLimit = moveSpeed
                * runMoveParameters.runSpeedMultiplier;

            targetPlanarVelocity = CustomUtilities.Multiply
                (parentMachine.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
            return targetPlanarVelocity;
        }
    }
}
