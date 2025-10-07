using Character.Controller.State;
using CharacterController;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterCrouchMovementState : CharacterMovementStateBase
    {
        public CrouchParameters crouchParameters = new CrouchParameters();
        public override ECharacterMoveState currentType
            => ECharacterMoveState.CrouchMove;
        public override void Enter()
        {
            base.Enter();
            Crouch();
        }
        public override void Update()
        {
            base.Update();
            if (crouchParameters.inputMode == InputMode.Hold)
            {
                if (characterActions.crouch.value == false)
                {
                    parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                }
            }
            else
            {
                if (characterActions.crouch.Started)
                {
                    parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                }
            }
        }
        void Crouch()
        {
            var dt = Time.deltaTime;
            SizeReferenceType sizeReferenceType = SizeReferenceType.Bottom;

            characterActor.CheckAndInterpolateHeight(
               characterActor.DefaultBodySize.y * crouchParameters.heightRatio,
               crouchParameters.sizeLerpSpeed * dt, sizeReferenceType);
        }
        public override void Exit()
        {
            base.Exit();
            StandUp();
        }
        void StandUp()
        {
            var dt = Time.deltaTime;
            SizeReferenceType sizeReferenceType = SizeReferenceType.Bottom;

            characterActor.CheckAndInterpolateHeight
               (characterActor.DefaultBodySize.y, crouchParameters.sizeLerpSpeed * dt, sizeReferenceType);
        }
        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            float speedMultiplier = materialControl == null ?
                1f : materialControl.CurrentSurface.speedMultiplier * materialControl.CurrentVolume.speedMultiplier;

            Vector3 targetPlanarVelocity;

            currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit
                * crouchParameters.speedMultiplier;

            targetPlanarVelocity = CustomUtilities.Multiply
                (parentMachine.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
            return targetPlanarVelocity;
        }
    }
}
