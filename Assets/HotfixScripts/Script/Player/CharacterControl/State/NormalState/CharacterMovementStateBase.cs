using Animancer;
using Character.Controller.State;
using HFSM;
using UnityEngine;
using static PlanarMovementParameters;

namespace Character.Controller.MoveState
{
    public abstract class CharacterMovementStateBase : StateBase<ECharacterMoveState>
    {
        public PlanarMovementParameters planarMovementParameters = new();
        public VerticalMovementParameters verticalMovementParameters = new();
        public LookingDirectionParameters lookingDirectionParameters = new();

        public AnimatorHelper Animancer { get; set; }
        protected CharacterActor characterActor { get; private set; }
        protected CharacterBrain characterBrain { get; private set; }

        public LinearMixerTransition currentAnimator;
        public CharacterActions characterActions
        {
            get
            {
                return characterBrain == null ?
                    new CharacterActions() : characterBrain.CharacterActions;
            }
        }

        protected new CharacterMovementStateMachine parentMachine
            => (CharacterMovementStateMachine)base.parentMachine;

        /// <summary>
        /// 地面与区域检测器
        /// </summary>
        public MaterialControl materialControl;

        public override void Enter()
        {
            base.Enter();
            Animancer.Play(currentAnimator);
        }
        protected float currentPlanarSpeedLimit = 0f;
        public bool UseGravity
        {
            get => verticalMovementParameters.useGravity;
            set => verticalMovementParameters.useGravity = value;
        }

        public override void Init()
        {
            characterActor = parentMachine.characterActor;
            characterBrain = parentMachine.characterBrain;
            database = parentMachine.database;
            Animancer = parentMachine.animancer;
            materialControl = parentMachine.materialControl;
        }
        public virtual void UpdateIK(int layerIndex)
        {

        }
        public virtual void PreCharacterSimulation()
        {

        }

        public override void FixUpdate()
        {
            base.FixUpdate();
            float dt = Time.deltaTime;

            ProcessVelocity(dt);

            if (parentMachine.rotationInput
                && !(characterActor.UpdateRootRotation
                    && characterActor.rootMotionRotationType == RootMotionRotationType.SetRotation))
            {
                HandleRotation(dt);
            }
        }

        public virtual void PostCharacterSimulation()
        {
            currentAnimator.State.Parameter =
                characterActor.PlanarVelocity.magnitude;
        }

        void ProcessVelocity(float dt)
        {
            if (parentMachine.positionInput)
            {
                var targetVelocity = ProcessPlanarMovement(dt);
                var motionInfo = SetMotionValues(targetVelocity);


                float acceleration = motionInfo.acceleration;

                bool needToAccelerate = CustomUtilities.Multiply
                    (parentMachine.InputMovementReference, currentPlanarSpeedLimit).sqrMagnitude
                        >= characterActor.PlanarVelocity.sqrMagnitude;


                if (needToAccelerate)
                {
                    acceleration *= motionInfo.angleAccelerationMultiplier;
                }
                else
                {
                    acceleration = motionInfo.deceleration;
                }
                characterActor.PlanarVelocity =
                    Vector3.MoveTowards(characterActor.PlanarVelocity, targetVelocity, acceleration * dt);
            }
            ProcessGravity(dt);
        }

        /// <summary>
        /// 速度
        /// </summary>
        protected abstract Vector3 ProcessPlanarMovement(float dt);

        protected virtual void ProcessVerticalVelocity(float dt)
        {

        }

        /// <summary>
        /// 重力
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void ProcessGravity(float dt)
        {
            ProcessVerticalVelocity(dt);
            if (!UseGravity)
                return;

            verticalMovementParameters.UpdateParameters();

            float gravityMultiplier = 1f;

            if (materialControl != null)
                gravityMultiplier = characterActor.LocalVelocity.y >= 0 ?
                    materialControl.CurrentVolume.gravityAscendingMultiplier :
                    materialControl.CurrentVolume.gravityDescendingMultiplier;

            float gravity = gravityMultiplier * verticalMovementParameters.gravity;
            if (!characterActor.IsStable)
                characterActor.VerticalVelocity +=
                CustomUtilities.Multiply(-characterActor.Up, gravity, dt);
        }

        /// <summary>
        /// 通过目标速度求得当前的加减速度
        /// </summary>
        /// <param name="targetPlanarVelocity"></param>
        protected virtual PlanarMovementProperties SetMotionValues(Vector3 targetPlanarVelocity)
        {
            float angleCurrentTargetVelocity =
                Vector3.Angle(characterActor.PlanarVelocity, targetPlanarVelocity);
            var currentVelocityInfo = new PlanarMovementProperties();
            switch (characterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:
                    currentVelocityInfo.acceleration = planarMovementParameters.notGroundedAcceleration;
                    currentVelocityInfo.deceleration = planarMovementParameters.notGroundedDeceleration;

                    currentVelocityInfo.angleAccelerationMultiplier =
                        planarMovementParameters.notGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;

                case CharacterActorState.StableGrounded:
                    currentVelocityInfo.acceleration = planarMovementParameters.stableGroundedAcceleration;
                    currentVelocityInfo.deceleration = planarMovementParameters.stableGroundedDeceleration;
                    currentVelocityInfo.angleAccelerationMultiplier =
                        planarMovementParameters.stableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);
                    break;

                case CharacterActorState.UnstableGrounded:
                    currentVelocityInfo.acceleration = planarMovementParameters.unstableGroundedAcceleration;
                    currentVelocityInfo.deceleration = planarMovementParameters.unstableGroundedDeceleration;
                    currentVelocityInfo.angleAccelerationMultiplier =
                        planarMovementParameters.unstableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);
                    break;

            }

            if (materialControl != null)
            {
                if (characterActor.IsGrounded)
                {
                    currentVelocityInfo.acceleration *= materialControl.CurrentSurface.accelerationMultiplier;
                    currentVelocityInfo.deceleration *= materialControl.CurrentSurface.decelerationMultiplier;
                }
                else
                {
                    currentVelocityInfo.acceleration *= materialControl.CurrentVolume.accelerationMultiplier;
                    currentVelocityInfo.deceleration *= materialControl.CurrentVolume.decelerationMultiplier;
                }
            }
            return currentVelocityInfo;
        }
        protected Vector3 targetLookingDirection = Vector3.zero;

        protected virtual void HandleRotation(float dt)
        {
            HandleLookingDirection(dt);
        }

        void HandleLookingDirection(float dt)
        {
            if (!lookingDirectionParameters.changeLookingDirection)
            {
                return;
            }
            switch (lookingDirectionParameters.lookingDirectionMode)
            {
                case LookingDirectionParameters.LookingDirectionMode.Movement:

                    switch (characterActor.CurrentState)
                    {
                        case CharacterActorState.NotGrounded:
                            SetTargetLookingDirection(lookingDirectionParameters.notGroundedLookingDirectionMode);
                            break;
                        case CharacterActorState.StableGrounded:
                            SetTargetLookingDirection(lookingDirectionParameters.stableGroundedLookingDirectionMode);
                            break;
                        case CharacterActorState.UnstableGrounded:
                            SetTargetLookingDirection(lookingDirectionParameters.unstableGroundedLookingDirectionMode);
                            break;
                    }
                    break;
                case LookingDirectionParameters.LookingDirectionMode.Target:
                    targetLookingDirection = Vector3.ProjectOnPlane
                        (lookingDirectionParameters.target.position - characterActor.Position,
                            characterActor.Up).normalized;
                    break;
                case LookingDirectionParameters.LookingDirectionMode.ExternalReference:
                    targetLookingDirection = parentMachine.MovementReferenceForward;
                    break;
            }

            Quaternion targetDeltaRotation = Quaternion.FromToRotation(characterActor.Forward,
                targetLookingDirection);

            Quaternion currentDeltaDotation = Quaternion.Slerp(Quaternion.identity,
                targetDeltaRotation,
                lookingDirectionParameters.speed * dt);

            characterActor.SetYaw(currentDeltaDotation * characterActor.Forward);
        }

        void SetTargetLookingDirection
            (LookingDirectionParameters.LookingDirectionMovementSource lookingDirectionMode)
        {
            if (lookingDirectionMode == LookingDirectionParameters.LookingDirectionMovementSource.Input)
            {
                if (parentMachine.InputMovementReference != Vector3.zero)
                {
                    targetLookingDirection = parentMachine.InputMovementReference;
                }
                else
                {
                    targetLookingDirection = characterActor.Forward;
                }
            }
            else
            {
                if (characterActor.PlanarVelocity != Vector3.zero)
                {
                    targetLookingDirection = Vector3.ProjectOnPlane
                        (characterActor.PlanarVelocity, characterActor.Up);
                }
                else
                {
                    targetLookingDirection = characterActor.Forward;
                }
            }
        }
    }
}
