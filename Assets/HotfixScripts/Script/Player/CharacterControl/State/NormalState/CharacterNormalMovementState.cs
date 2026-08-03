using Character.Controller.State;
using CharacterController;
using Fight;
using Fight.Number;
using UnityEngine;

namespace Character.Controller.MoveState
{
    public class CharacterNormalMovementState : CharacterMovementStateBase
    {
        public override ECharacterMoveState currentType => ECharacterMoveState.NormalMove;

        public override void Update()
        {
            base.Update();
            if (!characterActor.IsGrounded)
            {
                parentMachine.ChangeState(ECharacterMoveState.Jump);
            }
            else if (TryGetLatestInput(CharacterInputType.Crouch, out var crouchCommand)
                && crouchCommand.BoolValue)
            {
                parentMachine.ChangeState(ECharacterMoveState.CrouchMove);
            }
            else if (TryGetInput(CharacterInputType.Jump, out var jumpCommand)
                && jumpCommand.BoolValue)
            {
                parentMachine.ChangeState(ECharacterMoveState.Jump, new CharacterJumpStateInput(true));
            }
            else if (TryGetLatestInput(CharacterInputType.Run, out var runCommand)
                && runCommand.BoolValue
                && characterActor.Velocity != Vector3.zero)
            {
                parentMachine.ChangeState(ECharacterMoveState.RunMove);
            }

            else if (characterActor.Triggers.Count > 0
                && characterActor.Triggers[0].transform.GetComponentInParent<Ladder>() != null
                && TryGetInput(CharacterInputType.Interact, out var interactCommand)
                && interactCommand.BoolValue)
            {
                parentMachine.ChangeState(ECharacterMoveState.Climb);
            }
            else if (TryGetInput(CharacterInputType.Lock, out var lockCommand)
                && lockCommand.BoolValue)
            {
                var nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    database.SetValue(CharacterLockOnMovementState.targetKey, nearestEnemy.transform);

                    parentMachine.ChangeState(ECharacterMoveState.LockOnMove);
                }
            }
        }

        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            if (!TryGetMoveSpeed(out float moveSpeed))
            {
                currentPlanarSpeedLimit = 0f;
                return Vector3.zero;
            }

            float groundSpeedMultiplier = materialControl == null ?
                1f :
                materialControl.CurrentSurface.speedMultiplier
                    * materialControl.CurrentVolume.speedMultiplier;
            Vector3 targetPlanarVelocity = default;

            switch (characterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:
                    if (characterActor.WasGrounded)
                    {
                        currentPlanarSpeedLimit = Mathf.Max
                        (characterActor.PlanarVelocity.magnitude, moveSpeed);
                    }

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, groundSpeedMultiplier, currentPlanarSpeedLimit);
                    break;

                case CharacterActorState.StableGrounded:
                    currentPlanarSpeedLimit = moveSpeed;

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, groundSpeedMultiplier, currentPlanarSpeedLimit);
                    break
                    ;
                case CharacterActorState.UnstableGrounded:
                    currentPlanarSpeedLimit = moveSpeed;

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, groundSpeedMultiplier, currentPlanarSpeedLimit);
                    break;
            }
            return targetPlanarVelocity;
        }

        //todo : 更新检测方式
        private CombatEntity FindNearestEnemy()
        {
            // Enemy[] enemies = UnityEngine.Object.FindObjectsOfType<Enemy>();
            // Enemy nearest = null;
            // float nearestDistance = float.MaxValue;

            // foreach (Enemy enemy in enemies)
            // {
            //     float distance = Vector3.Distance(characterActor.Position, enemy.transform.position);
            //     if (distance < 25f && distance < nearestDistance)
            //     {
            //         Vector3 directionToEnemy = enemy.transform.position - characterActor.Position;
            //         float angle = Vector3.Angle(characterActor.Forward, directionToEnemy);

            //         if (angle < 120f)
            //         {
            //             nearest = enemy;
            //             nearestDistance = distance;
            //         }
            //     }
            // }

            return null;
        }
    }
}
