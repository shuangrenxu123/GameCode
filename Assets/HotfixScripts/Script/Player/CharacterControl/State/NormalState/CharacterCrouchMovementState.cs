using Character.Controller.State;
using CharacterController;
using HFSM;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterCrouchMovementState : CharacterMovementStateBase
    {
        const float HeightTolerance = 0.01f;

        public CrouchParameters crouchParameters = new CrouchParameters();
        bool isTryingToStand;

        public override ECharacterMoveState currentType
            => ECharacterMoveState.CrouchMove;
        public override void Enter(StateBaseInput input = null)
        {
            base.Enter(input);
            isTryingToStand = false;
        }
        public override void Update()
        {
            base.Update();
            if (!characterActor.IsGrounded)
            {
                RestoreDefaultHeightImmediately();
                parentMachine.ChangeState(ECharacterMoveState.Jump);
                return;
            }
            else if (crouchParameters.inputMode == InputMode.Hold)
            {
                if (TryGetLatestInput(CharacterInputType.Crouch, out var crouchCommand))
                {
                    isTryingToStand = !crouchCommand.BoolValue;
                }
            }
            else
            {
                if (TryGetInput(CharacterInputType.Crouch, out var crouchCommand)
                    && crouchCommand.Phase == CharacterInputPhase.Started)
                {
                    isTryingToStand = !isTryingToStand;
                }
            }
        }

        public override void FixUpdate()
        {
            float targetHeight = isTryingToStand
                ? characterActor.DefaultBodySize.y
                : characterActor.DefaultBodySize.y * crouchParameters.heightRatio;

            bool canResize = characterActor.CheckAndInterpolateHeight(
                targetHeight,
                Mathf.Clamp01(crouchParameters.sizeLerpSpeed * Time.fixedDeltaTime),
                SizeReferenceType.Bottom);

            if (isTryingToStand
                && canResize
                && Mathf.Abs(characterActor.BodySize.y - targetHeight) <= HeightTolerance)
            {
                characterActor.SetSize(
                    characterActor.DefaultBodySize,
                    SizeReferenceType.Bottom);
                parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                return;
            }

            base.FixUpdate();
        }

        public override void Exit()
        {
            base.Exit();
            isTryingToStand = false;
        }

        void RestoreDefaultHeightImmediately()
        {
            characterActor.CheckAndInterpolateHeight(
                characterActor.DefaultBodySize.y,
                1f,
                SizeReferenceType.Bottom);
        }
        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            if (!TryGetMoveSpeed(out float moveSpeed))
            {
                currentPlanarSpeedLimit = 0f;
                return Vector3.zero;
            }

            float speedMultiplier = materialControl == null ?
                1f : materialControl.CurrentSurface.speedMultiplier * materialControl.CurrentVolume.speedMultiplier;

            Vector3 targetPlanarVelocity;

            currentPlanarSpeedLimit = moveSpeed
                * crouchParameters.speedMultiplier;

            targetPlanarVelocity = CustomUtilities.Multiply
                (parentMachine.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
            return targetPlanarVelocity;
        }
    }
}
