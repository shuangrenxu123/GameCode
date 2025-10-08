using Animancer;
using Character.Controller.State;
using CharacterController;
using CharacterController.Camera;
using UnityEngine;
namespace Character.Controller.MoveState
{
    public class CharacterLockOnMovementState : CharacterMovementStateBase
    {
        public const string targetKey = "LockOnTarget";
        public override ECharacterMoveState currentType => ECharacterMoveState.LockOnMove;
        public MixerTransition2D movementAnimation;
        private Transform lockTarget;
        private CameraLockOnEffect lockOnEffect;
        private Camera3D camera3D;

        [SerializeField] private float lockOnMoveSpeed = 0.8f;
        [SerializeField] private float maxLockDistance = 30f;

        public override void Init()
        {
            base.Init();
            camera3D = database.GetData<Camera3D>("Camera3D");

            if (camera3D)
            {
                lockOnEffect = camera3D.effectManager.GetEffect<CameraLockOnEffect>();
            }
        }

        public override void Enter()
        {
            Animancer.Play(movementAnimation);

            if (lockOnEffect != null)
            {
                lockOnEffect.Activate();
            }

            lookingDirectionParameters.lookingDirectionMode = LookingDirectionParameters.LookingDirectionMode.Target;

            var targetTransform = database.GetData<Transform>(targetKey);
            if (targetTransform)
            {
                SetLockTarget(targetTransform);
            }
        }


        public override void Exit()
        {
            base.Exit();

            if (lockOnEffect != null)
            {
                lockOnEffect.Deactivate();
            }

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

            if (ShouldExitLockOnDueToDistance())
            {
                parentMachine.ChangeState(ECharacterMoveState.NormalMove);
                return;
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
            if (lockOnEffect != null && target != null)
            {
                lockOnEffect.SetLockTarget(target);
            }
        }

        void ClearLockTarget()
        {
            lockTarget = null;
            if (lockOnEffect != null)
            {
                lockOnEffect.ClearLockTarget();
            }
        }

        private bool ShouldExitLockOnDueToDistance()
        {
            if (lockTarget == null) return true;

            float distance = Vector3.Distance(characterActor.Position, lockTarget.position);
            return distance > maxLockDistance;
        }
        public override void PostCharacterSimulation()
        {
            movementAnimation.State.Parameter = new Vector2(characterActor.LocalVelocity.x, characterActor.LocalVelocity.z);
        }
        public override void RefreshAnimator()
        {
            Animancer.Play(movementAnimation);
        }
    }
}