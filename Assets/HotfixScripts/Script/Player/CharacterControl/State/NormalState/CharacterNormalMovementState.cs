using Character.Controller.State;
using Fight;
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
            else if (characterActions.@lock.Started)
            {
                var nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    database.SetData(CharacterLockOnMovementState.targetKey, nearestEnemy.transform);

                    parentMachine.ChangeState(ECharacterMoveState.LockOnMove);
                }
            }
        }

        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            float characterSpeedMultiplier = parentMachine.stateManger.player
                .CombatEntity.properties
                .GetPropertyValue(Fight.Number.CombatNumberBox.PropertyType.SpeedMultiplier) / 100f;

            float groundSpeedMultiplier = materialControl == null ?
                1f :
                materialControl.CurrentSurface.speedMultiplier
                    * materialControl.CurrentVolume.speedMultiplier;

            float finalSpeedMultiplier = characterSpeedMultiplier * groundSpeedMultiplier;
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
                        (parentMachine.InputMovementReference, finalSpeedMultiplier, currentPlanarSpeedLimit);
                    break;

                case CharacterActorState.StableGrounded:
                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, finalSpeedMultiplier, currentPlanarSpeedLimit);
                    break
                    ;
                case CharacterActorState.UnstableGrounded:
                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply
                        (parentMachine.InputMovementReference, finalSpeedMultiplier, currentPlanarSpeedLimit);
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
