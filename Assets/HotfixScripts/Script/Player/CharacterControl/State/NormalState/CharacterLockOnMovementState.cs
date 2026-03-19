using Animancer;
using Character.Controller.State;
using CharacterController;
using CharacterController.Camera;
using HFSM;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterLockOnMovementState : CharacterMovementStateBase
    {
        public const string targetKey = "LockOnTarget";
        public override ECharacterMoveState currentType => ECharacterMoveState.LockOnMove;
        public MixerTransition2D movementAnimation;
        public Transform lockTarget { get; private set; }

        [SerializeField] private float lockOnMoveSpeed = 0.8f;
        [SerializeField] private float maxLockDistance = 30f;

        public override void Init()
        {
            base.Init();
        }

        public override void Enter(StateBaseInput input = null)
        {
            Animancer.Play(movementAnimation);

            lookingDirectionParameters.lookingDirectionMode = LookingDirectionParameters.LookingDirectionMode.Target;

            var targetTransform = database.GetValue<string, Transform>(targetKey);
            if (targetTransform)
            {
                SetLockTarget(targetTransform);
            }
        }


        public override void Exit()
        {
            base.Exit();

            ClearLockTarget();

            lookingDirectionParameters.lookingDirectionMode = LookingDirectionParameters.LookingDirectionMode.Movement;
        }

        public override void Update()
        {
            base.Update();

            if (characterActions.@lock.Started)
            {
                parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                return;
            }
            if (characterActions.run.Started)
            {
                parentMachine.ChangeState(ECharacterMoveState.RunMove);
                return;
            }
            if (ShouldExitLockOnDueToDistance())
            {
                parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                return;
            }

            else if (characterActions.jump.Started ||
                (!characterActor.IsGrounded && characterActor.IsFalling))
            {
                parentMachine.ChangeState(ECharacterMoveState.Jump);
            }
        }

        protected override Vector3 ProcessPlanarMovement(float dt)
        {
            float characterSpeedMultiplier = combatEntity.properties
                .GetPropertyValue(Fight.Number.CombatNumberBox.PropertyType.SpeedMultiplier) / 100f;

            float finalSpeedMultiplier = characterSpeedMultiplier * lockOnMoveSpeed;
            currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

            Vector3 targetPlanarVelocity = CustomUtilities.Multiply(
                parentMachine.InputMovementReference,
                finalSpeedMultiplier,
                currentPlanarSpeedLimit
            );

            return targetPlanarVelocity;
        }

        protected override void HandleRotation(float dt)
        {
            if (lockTarget != null && ShouldExitLockOnDueToDistance())
            {
                return;
            }

            if (lockTarget != null)
            {
                lookingDirectionParameters.target = lockTarget;
            }

            base.HandleRotation(dt);
        }

        void SetLockTarget(Transform target)
        {
            lockTarget = target;
        }

        void ClearLockTarget()
        {
            lockTarget = null;
        }

        private bool ShouldExitLockOnDueToDistance()
        {
            if (lockTarget == null) return true;

            float distance = Vector3.Distance(characterActor.Position, lockTarget.position);
            return distance > maxLockDistance;
        }

        public override void PostCharacterSimulation()
        {
            movementAnimation.State.Parameter = new Vector2
                (characterActor.LocalVelocity.x, characterActor.LocalVelocity.z);
        }

        public override void RefreshAnimator()
        {
            Animancer.Play(movementAnimation);
        }
    }
}