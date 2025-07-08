using Character.Controller.State;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterNormalMovementState : CharacterMovementStateBase
    {

        public override ECharacterMoveState currentType => ECharacterMoveState.NormalMove;

        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            float speedMultiplier = materialControl == null ?
                1f :
                materialControl.CurrentSurface.speedMultiplier * materialControl.CurrentVolume.speedMultiplier;

            //Ŀ���ٶ�
            Vector3 targetPlanarVelocity = default;

            switch (characterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:
                    if (characterActor.WasGrounded)
                    {
                        currentPlanarSpeedLimit = Mathf.Max
                        (characterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);
                    }

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
                    break;

                case CharacterActorState.StableGrounded:
                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
                    break
                    ;
                case CharacterActorState.UnstableGrounded:
                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
                    break;
            }
            return targetPlanarVelocity;
        }
    }
}
