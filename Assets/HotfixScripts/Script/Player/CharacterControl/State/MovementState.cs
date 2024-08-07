using Animancer;
using Audio;
using System;
using UIWindow;
using UnityEngine;
namespace CharacterControlerStateMachine
{
    public class MovementState : CharacterControlStateBase
    {
        //todo 将跳跃，下蹲等动画提出来，最终让整个变为分层状态机
        #region parameters
        public PlanarMovementParameters planarMovementParameters = new();
        public VerticalMovementParameters verticalMovementParameters = new VerticalMovementParameters();
        public CrouchParameters crouchParameters = new CrouchParameters();
        public LookingDirectionParameters lookingDirectionParameters = new LookingDirectionParameters();
        #endregion

        #region Animator
        public LinearMixerTransition normalMoveAnimator;
        public LinearMixerTransition crouchMoveAnimator;
        public MixerState<Vector2> lockEnemyAnimator;

        private LinearMixerTransition currentAnimator;

        private const string jump = "Jump";
        private const string jumpFall = "JumpFall";
        private const string jumpEnd = "JumpEnd";
        #endregion

        #region Events
        /// <summary>
        /// 角色跳跃时触发的事件。（只要跳跃了就会触发）
        /// </summary>
        public event Action OnJumpPerformed;

        /// <summary>
        /// 当角色从地面跳跃时触发的事件。
        /// </summary>
        public event Action<bool> OnGroundedJumpPerformed;

        /// <summary>
        /// 角色跳跃连跳时触发的事件（即不在地面时跳跃）。
        /// </summary>
        public event Action<int> OnNotGroundedJumpPerformed;

        #endregion

        #region Audio
        public AudioData moveAudio;
        private AudioAgent moveagent;
        #endregion
        public MaterialControl MaterialControl;
        /// <summary>
        /// 连跳次数
        /// </summary>
        protected int notGroundedJumpsLeft = 0;
        /// <summary>
        /// 是否允许取消跳跃
        /// </summary>
        protected bool isAllowedToCancelJump = false;
        protected bool wantToRun = false;
        public bool IsRun { get => wantToRun; }
        protected bool lockFlag = false;
        protected float currentPlanarSpeedLimit = 0f;
        /// <summary>
        /// 当前地面是否可以跳跃
        /// </summary>
        protected bool groundedJumpAvailable = true;
        /// <summary>
        /// 跳跃的方向，（用于斜面跳板）
        /// </summary>
        protected Vector3 JumpDirection = Vector3.zero;
        protected Vector3 targetLookingDirection = Vector3.zero;
        protected float targetHeight = 1f;
        /// <summary>
        /// 是否要蹲下
        /// </summary>
        protected bool wantToCrouch = false;
        public bool isCrouched = false;
        /// <summary>
        /// 是否启用重力
        /// </summary>
        public bool UseGravity
        {
            get => verticalMovementParameters.useGravity;
            set => verticalMovementParameters.useGravity = value;
        }

        protected PlanarMovementParameters.PlanarMovementProperties currentMotion = new();
        bool reducedAirControlFlag = false;
        float reducedAirControlInitialTime = 0f;
        float reductionDuration = 0.5f;

        public override void Init()
        {
            base.Init();
            notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;

            targetHeight = CharacterActor.DefaultBodySize.y;
            float minCrouchHeightRatio = CharacterActor.BodySize.x / CharacterActor.BodySize.y;
            crouchParameters.heightRatio = Mathf.Max(minCrouchHeightRatio, crouchParameters.heightRatio);
            CharacterActor.OnTeleport += OnTeleport;
            CharacterActor.OnGroundedStateEnter += HandleJumpEnterGround;

            normalMoveAnimator = animatorConfig.linearMixerAnimators["NormalMove"];
            crouchMoveAnimator = animatorConfig.linearMixerAnimators["CrouchMove"];
            var state = Animancer.Animancer.States.GetOrCreate(animatorConfig.LockMovement);
            lockEnemyAnimator = (MixerState<Vector2>)state;
        }
        public override void Enter()
        {
            CharacterActor.alwaysNotGrounded = false;
            targetLookingDirection = CharacterActor.Forward;
            currentAnimator = normalMoveAnimator;
            currentPlanarSpeedLimit = Mathf.Max(CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

            //moveagent = AudioManager.Instance.PlayAudio(moveAudio.GetClip(MaterialControl.CurrentSurface.movementSound),moveAudio.layer,true);
            //moveagent.Pause();
            CharacterActor.UseRootMotion = false;

            if (lockFlag)
            {
                Animancer.Play(lockEnemyAnimator);
            }
            else
            {
                Animancer.Play(currentAnimator);
            }
        }
        public override void Exit()
        {
            CharacterActor.OnTeleport -= OnTeleport;
            reducedAirControlFlag = false;
            //moveagent.Stop();
        }
        void OnTeleport(Vector3 position, Quaternion rotation)
        {
            targetLookingDirection = CharacterActor.Forward;
            isAllowedToCancelJump = false;
        }
        public override void FixUpdate()
        {
            float dt = Time.deltaTime;
            HandleSize(dt);
            HandleVelocity(dt);
            HandleRotation(dt);
        }
        public override void Update()
        {
            if (CharacterActions.roll.Started && CharacterActor.IsGrounded)
            {
                database.SetData<bool>("roll", true);
            }
            if (CharacterActions.attack.Started)
            {
                database.SetData<bool>("attack", true);
            }
            CheckForInteractableObject();
        }
        #region 锁敌(lock)

        public bool HandleLockEnemy(Transform target)
        {
            if (target == null || lockFlag)
            {
                lookingDirectionParameters.target = null;
                lookingDirectionParameters.lookingDirectionMode = LookingDirectionParameters.LookingDirectionMode.Movement;
                lockFlag = false;
                Animancer.Play(currentAnimator);
                return false;
            }
            else
            {
                lookingDirectionParameters.target = target;
                lookingDirectionParameters.lookingDirectionMode = LookingDirectionParameters.LookingDirectionMode.Target;
                lockFlag = true;
                Animancer.Play(lockEnemyAnimator);
                return true;
            }

        }
        #endregion
        #region 移动（movement）
        private void HandleVelocity(float dt)
        {
            ProcessVerticalMovement(dt);
            ProcessPlanarMovement(dt);
        }
        /// <summary>
        /// 处理竖直的速度相关
        /// </summary>
        /// <param name="dt"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProcessVerticalMovement(float dt)
        {
            ProcessGravity(dt);
            ProcessJump(dt);
        }
        /// <summary>
        /// 处理角色平面移动
        /// </summary>
        /// <param name="dt"></param>
        private void ProcessPlanarMovement(float dt)
        {
            float speedMultiplier = MaterialControl == null ? 1f : MaterialControl.CurrentSurface.speedMultiplier * MaterialControl.CurrentVolume.speedMultiplier;
            bool needToAccalerate = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, currentPlanarSpeedLimit).sqrMagnitude >= CharacterActor.PlanarVelocity.sqrMagnitude;
            //目标速度
            Vector3 targetPlanarVelocity = default;
            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:
                    if (CharacterActor.WasGrounded)
                        currentPlanarSpeedLimit = Mathf.Max(CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);
                    targetPlanarVelocity = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
                    break;
                case CharacterActorState.StableGrounded:
                    if (planarMovementParameters.runInputMode == InputMode.Toggle)
                    {
                        if (CharacterActions.run.Started)
                            wantToRun = !wantToRun;
                    }
                    else
                    {
                        wantToRun = CharacterActions.run.value;
                    }

                    if (wantToCrouch || !planarMovementParameters.canRun)
                        wantToRun = false;
                    //是否下蹲
                    if (isCrouched)
                    {
                        currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit * crouchParameters.speedMultiplier;
                    }
                    else
                    {
                        currentPlanarSpeedLimit = wantToRun ? planarMovementParameters.boostSpeedLimit : planarMovementParameters.baseSpeedLimit;
                    }
                    targetPlanarVelocity = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
                    break;
                case CharacterActorState.UnstableGrounded:
                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);
                    break;
            }

            SetMotionValues(targetPlanarVelocity);
            float acceleration = currentMotion.acceleration;
            //启用加速度
            if (needToAccalerate)
            {
                acceleration *= currentMotion.angleAccelerationMultiplier;
            }
            else
            {
                acceleration = currentMotion.deceleration;
            }

            CharacterActor.PlanarVelocity = Vector3.MoveTowards(CharacterActor.PlanarVelocity, targetPlanarVelocity, acceleration * dt);
        }
        private void ProcessGravity(float dt)
        {
            if (!UseGravity)
                return;
            verticalMovementParameters.UpdateParameters();
            float gravityMutiplier = 1f;
            if (MaterialControl != null)
                gravityMutiplier = CharacterActor.LocalVelocity.y >= 0 ?
                    MaterialControl.CurrentVolume.gravityAscendingMultiplier :
                    MaterialControl.CurrentVolume.gravityDescendingMultiplier;
            float gravity = gravityMutiplier * verticalMovementParameters.gravity;
            if (!CharacterActor.IsStable)
                CharacterActor.VerticalVelocity += CustomUtilities.Multiply(-CharacterActor.Up, gravity, dt);
        }
        #endregion
        #region 旋转（rotate）
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

                    switch (CharacterActor.CurrentState)
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
                    targetLookingDirection = Vector3.ProjectOnPlane(lookingDirectionParameters.target.position - CharacterActor.Position, CharacterActor.Up).normalized;
                    break;
                case LookingDirectionParameters.LookingDirectionMode.ExternalReference:
                    targetLookingDirection = CharacterStateController.MovementReferenceForward;
                    break;
            }

            Quaternion targetDeltaRotation = Quaternion.FromToRotation(CharacterActor.Forward, targetLookingDirection);
            Quaternion currentDeltaDotation = Quaternion.Slerp(Quaternion.identity, targetDeltaRotation, lookingDirectionParameters.speed * dt);

            CharacterActor.SetYaw(currentDeltaDotation * CharacterActor.Forward);
        }
        void SetTargetLookingDirection(LookingDirectionParameters.LookingDirectionMovementSource lookingDirectionMode)
        {
            if (lookingDirectionMode == LookingDirectionParameters.LookingDirectionMovementSource.Input)
            {
                if (CharacterStateController.InputMovementReference != Vector3.zero)
                {
                    targetLookingDirection = CharacterStateController.InputMovementReference;
                }
                else
                {
                    targetLookingDirection = CharacterActor.Forward;
                }
            }
            else
            {
                if (CharacterActor.PlanarVelocity != Vector3.zero)
                {
                    targetLookingDirection = Vector3.ProjectOnPlane(CharacterActor.PlanarVelocity, CharacterActor.Up);
                }
                else
                {
                    targetLookingDirection = CharacterActor.Forward;
                }
            }
        }
        #endregion
        #region 下蹲(Crouch)
        /// <summary>
        /// 下蹲
        /// </summary>
        /// <param name="dt"></param>
        void Crouch(float dt)
        {
            SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ? SizeReferenceType.Bottom : crouchParameters.notGroundedReference;

            bool validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y * crouchParameters.heightRatio,
                crouchParameters.sizeLerpSpeed * dt, sizeReferenceType);

            if (validSize)
            {
                isCrouched = true;
                currentAnimator = crouchMoveAnimator;
                Animancer.Play(currentAnimator);
            }
        }
        /// <summary>
        /// 起立
        /// </summary>
        /// <param name="dt"></param>
        void StandUp(float dt)
        {
            SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ? SizeReferenceType.Bottom : crouchParameters.notGroundedReference;

            bool validSize = CharacterActor.CheckAndInterpolateHeight(CharacterActor.DefaultBodySize.y, crouchParameters.sizeLerpSpeed * dt, sizeReferenceType);
            if (validSize && isCrouched)
            {
                isCrouched = false;
                currentAnimator = normalMoveAnimator;
                Animancer.Play(currentAnimator);
            }

        }
        #endregion
        #region 跳跃(jump)
        public enum JumpResult
        {
            Invalid,
            Grounded,
            NotGrounded,
        }
        JumpResult CanJump()
        {
            JumpResult jumpResult = JumpResult.Invalid;
            if (!verticalMovementParameters.canJump || isCrouched)
                return JumpResult.Invalid;

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:
                    if (CharacterActions.jump.Started)
                    {
                        if (CharacterActor.NotGroundedTime <= verticalMovementParameters.postGroundedJumpTime && groundedJumpAvailable)
                        {
                            jumpResult = JumpResult.Grounded;
                        }
                        else if (notGroundedJumpsLeft != 0)
                        {
                            jumpResult = JumpResult.NotGrounded;
                        }
                    }
                    break;
                case CharacterActorState.StableGrounded:
                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime && groundedJumpAvailable)
                    {
                        jumpResult = JumpResult.Grounded;
                    }
                    break;
                case CharacterActorState.UnstableGrounded:
                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime && verticalMovementParameters.canJumpOnUnstableGround)
                        jumpResult = JumpResult.Grounded;
                    break;
            }
            return jumpResult;
        }

        protected void ProcessJump(float dt)
        {
            ProcessRegularJump(dt);
            ProcessJumpDown(dt);
        }
        private void ProcessRegularJump(float dt)
        {
            if (CharacterActor.IsGrounded)
            {
                notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;
                groundedJumpAvailable = true;
            }
            if (isAllowedToCancelJump)
            {
                if (verticalMovementParameters.cancelJumpOnRelease)
                {
                    if (CharacterActions.jump.StartedElapsedTime >= verticalMovementParameters.cancelJumpMinTime || CharacterActor.IsFalling)
                    {
                        isAllowedToCancelJump = false;
                    }
                    //如果已经松开了跳跃键且按下的时间超越了最小的持续时间则被认定为可以进入了跳跃状态
                    else if (!CharacterActions.jump.value && CharacterActions.jump.StartedElapsedTime >= verticalMovementParameters.cancelJumpMinTime)
                    {
                        Vector3 projectJumpVerlocity = Vector3.Project(CharacterActor.Velocity, JumpDirection);
                        CharacterActor.Velocity -= CustomUtilities.Multiply(projectJumpVerlocity, 1f - verticalMovementParameters.cancelJumpMultiplier);
                        isAllowedToCancelJump = false;
                    }
                }
            }
            else
            {
                JumpResult jumpResult = CanJump();
                switch (jumpResult)
                {
                    case JumpResult.Grounded:
                        groundedJumpAvailable = false;
                        break;
                    case JumpResult.NotGrounded:
                        notGroundedJumpsLeft--;
                        break;
                    case JumpResult.Invalid:
                        return;
                }

                if (CharacterActor.IsGrounded)
                {

                    OnGroundedJumpPerformed?.Invoke(true);
                }
                else
                {
                    OnNotGroundedJumpPerformed?.Invoke(notGroundedJumpsLeft);
                }

                OnJumpPerformed?.Invoke();

                JumpDirection = SetJumpDirection();

                //只有调用该函数才能取消强制接触地面的状态，否则无法跳跃
                if (CharacterActor.IsGrounded)
                    CharacterActor.ForceNotGrounded();

                CharacterActor.Velocity -= Vector3.Project(CharacterActor.Velocity, JumpDirection);
                CharacterActor.Velocity += CustomUtilities.Multiply(JumpDirection, verticalMovementParameters.jumpSpeed);
                Animancer.Play(animatorConfig.clipAnimators[jump]);
                if (verticalMovementParameters.cancelJumpOnRelease)
                {
                    isAllowedToCancelJump = true;
                }

            }

        }
        private void HandleJumpEnterGround(Vector3 speed)
        {
            if (lockFlag)
            {
                Animancer.Play(animatorConfig.clipAnimators[jumpEnd]).Events.OnEnd = () => { Animancer.Play(lockEnemyAnimator); };
            }
            else
            {
                Animancer.Play(animatorConfig.clipAnimators[jumpEnd]).Events.OnEnd = () => { Animancer.Play(currentAnimator); };
            }
        }

        /// <summary>
        /// 返回跳跃的方向，目前是角色头顶，可以后续从斜坡上起跳等
        /// </summary>
        private Vector3 SetJumpDirection()
        {
            return CharacterActor.Up;
        }
        /// <summary>
        /// 处理自身的下落，如从单向平台下落等
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private bool ProcessJumpDown(float dt)
        {
            if (!verticalMovementParameters.canJumpDown)
                return false;
            if (!CharacterActor.IsStable)
                return false;
            if (!CharacterActor.IsGroundAOneWayPlatform)
                return false;
            //是否对所在地面进行过滤（tag）
            if (verticalMovementParameters.filterByTag)
            {
                if (!CharacterActor.gameObject.CompareTag(verticalMovementParameters.jumpDownTag))
                    return false;
            }
            //是否执行了跳下的行为，即是否按下按键从单项平台上下来
            if (!ProcessJumpDownAction())
                return false;

            JumpDown(dt);
            return true;

        }
        private void JumpDown(float dt)
        {
            float groundDispalacementExtraDistance = 0f;
            Vector3 groundDisplacement = CustomUtilities.Multiply(CharacterActor.GroundVelocity, dt);
            if (!CharacterActor.IsGroundAscending)
                groundDispalacementExtraDistance = groundDisplacement.magnitude;
            CharacterActor.ForceNotGrounded();
            CharacterActor.Position -= CustomUtilities.Multiply(CharacterActor.Up, CharacterConstants.ColliderMinBottomOffset + verticalMovementParameters.jumpDownDistance + groundDispalacementExtraDistance);
        }
        private bool ProcessJumpDownAction()
        {
            return isCrouched && CharacterActions.jump.Started;
        }
        #endregion
        #region 切换状态(Transform)
        /// <summary>
        /// 切换到与物品交互状态(不可移动)
        /// </summary>
        private void CheckForInteractableObject()
        {
            RaycastHit hit;
            if (Physics.SphereCast(CharacterActor.transform.position, 0.3f, CharacterActor.transform.forward, out hit, 1f))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    var interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable != null)
                    {
                        //UIManager.Instance.OpenUI<InteractPanel>();
                        if (CharacterActions.interact.Started)
                        {
                            database.SetData("interaction", true);
                            database.SetData("interactable", interactable);
                        }
                    }
                }
            }
            else
            {
                UIManager.Instance.CloseUI<InteractPanel>();

            }
        }
        #endregion
        void HandleSize(float dt)
        {
            if (crouchParameters.enableCrouch)
            {
                if (crouchParameters.inputMode == InputMode.Toggle)
                {
                    if (CharacterActions.crouch.Started)
                        wantToCrouch = !wantToCrouch;
                }
                else
                {
                    wantToCrouch = CharacterActions.crouch.value;
                }

                if (!crouchParameters.notGroundedCrouch && !CharacterActor.IsGrounded)
                    wantToCrouch = false;
                if (CharacterActor.IsGrounded && wantToRun)
                    wantToCrouch = false;
            }
            else
            {
                wantToCrouch = false;
            }
            if (wantToCrouch)
            {
                Crouch(dt);
            }
            else
            {
                StandUp(dt);
            }
        }
        /// <summary>
        /// 处理加速度相关
        /// </summary>
        /// <param name="targetPlanarVelocity"></param>
        void SetMotionValues(Vector3 targetPlanarVelocity)
        {
            float angleCurrentTargetVelocity = Vector3.Angle(CharacterActor.PlanarVelocity, targetPlanarVelocity);
            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:
                    if (reducedAirControlFlag)
                    {
                        float time = Time.time - reducedAirControlInitialTime;
                        if (time <= reductionDuration)
                        {
                            currentMotion.acceleration = (planarMovementParameters.notGroundedAcceleration / reductionDuration) * time;
                            currentMotion.deceleration = (planarMovementParameters.notGroundedDeceleration / reductionDuration) * time;
                        }
                        else
                        {
                            reducedAirControlFlag = false;
                            currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                            currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                        }
                    }
                    else
                    {
                        currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                        currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                    }
                    currentMotion.angleAccelerationMultiplier = planarMovementParameters.notGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;
                case CharacterActorState.StableGrounded:
                    currentMotion.acceleration = planarMovementParameters.stableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.stableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier = planarMovementParameters.stableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);
                    break;
                case CharacterActorState.UnstableGrounded:
                    currentMotion.acceleration = planarMovementParameters.unstableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.unstableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier = planarMovementParameters.unstableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);
                    break;

            }

            if (MaterialControl != null)
            {
                if (CharacterActor.IsGrounded)
                {
                    currentMotion.acceleration *= MaterialControl.CurrentSurface.accelerationMultiplier;
                    currentMotion.deceleration *= MaterialControl.CurrentSurface.decelerationMultiplier;
                }
                else
                {
                    currentMotion.acceleration *= MaterialControl.CurrentVolume.accelerationMultiplier;
                    currentMotion.deceleration *= MaterialControl.CurrentVolume.decelerationMultiplier;
                }
            }
        }
        #region 更新动画(Update Animator Parmeters)
        /// <summary>
        /// 在物理完毕之后更新动画，意味着我们在不输入速度情况下也可以处理相关动画
        /// </summary>
        public override void PostCharacterSimulation()
        {
            //if (!CharacterActor.IsAnimatorValid())
            //{
            //    return;
            //}
            // CharacterStateController.Animator.SetFloat(verticalSpeedParameter, CharacterActor.LocalVelocity.y);
            // CharacterStateController.Animator.SetFloat(planarSpeedParmeter, CharacterActor.PlanarVelocity.magnitude);
            if (CharacterActor.IsStable)
            {

                //if (CharacterActor.PlanarVelocity.magnitude > 0.01f && !moveagent.isPlaying())
                //{
                //    moveagent.Continue();
                //}
                //else if(CharacterActor.PlanarVelocity.magnitude  <= 0.01f && moveagent.isPlaying())
                //{
                //    moveagent.Pause();
                //}
                currentAnimator.State.Parameter = CharacterActor.PlanarVelocity.magnitude;

                if (lockFlag)
                {
                    lockEnemyAnimator.Parameter = new Vector2(CharacterActor.LocalVelocity.z, CharacterActor.LocalVelocity.x).normalized;
                }
            }
            else
            {

                if (CharacterActor.Velocity.y > 0)
                {
                    Animancer.Play(animatorConfig.clipAnimators[jump]);
                }
                else if (CharacterActor.Velocity.y < 0)
                {
                    Animancer.Play(animatorConfig.clipAnimators[jumpFall]);
                }
            }

        }
        #endregion
    }
}