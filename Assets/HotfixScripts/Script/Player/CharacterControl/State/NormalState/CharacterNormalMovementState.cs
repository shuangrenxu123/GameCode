using Character.Controller.State;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterNormalMovementState : CharacterMovementStateBase
    {
        public override ECharacterMoveState currentType => ECharacterMoveState.NormalMove;

        public override void Update()
        {
            base.Update();
            if (characterActions.crouch.Started)
            {
                parentMachine.ChangeState(ECharacterMoveState.CrouchMove);
            }
            else if (characterActions.jump.Started ||
                (!characterActor.IsGrounded && characterActor.IsFalling))
            {
                parentMachine.ChangeState(ECharacterMoveState.Jump);
            }
            else if (characterActions.interact.Started
                && characterActor.Triggers.Count > 0)
            {
                var ladder = characterActor.Triggers[0].transform.GetComponentInParent<Ladder>();
                if (ladder != null)
                {
                    parentMachine.ChangeState(ECharacterMoveState.Climb);
                }
            }
        }

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
