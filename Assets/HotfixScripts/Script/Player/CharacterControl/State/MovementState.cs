using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MovementState : CharacterControlStateBase
{
    #region parameters
    public PlanarMovementParameters planarMovementParameters = new PlanarMovementParameters();
    public VerticalMovementParameters verticalMovementParameters = new VerticalMovementParameters();
    public CrouchParameters crouchParameters = new CrouchParameters();
    public LookingDirectionParameters lookingDirectionParameters = new LookingDirectionParameters();
    #endregion
    
    #region Animator
    [SerializeField] protected string groundParamerter = "Grounded";
    [SerializeField] protected string stableParameter = "Stable";
    [SerializeField] protected string verticalSpeedParameter = "VerticalSpeed";
    [SerializeField] protected string planarSpeedParmeter = "PlanarSpeed";
    [SerializeField] protected string horizontalAxisParameter = "HorizontalAxis";
    [SerializeField] protected string verticalAxisParameter = "VerticalAxis";
    [SerializeField] protected string heightParameter = "Height";
    #endregion
    
    #region Events
    /// <summary>
    /// ��ɫ��Ծʱ�������¼���
    /// </summary>
    public event Action OnJumpPerformed;

    /// <summary>
    /// ����ɫ�ӵ�����Ծʱ�������¼���
    /// </summary>
    public event Action<bool> OnGroundedJumpPerformed;

    /// <summary>
    /// ��ɫ��Ծʱ�������¼���
    /// </summary>
    public event Action<int> OnNotGroundedJumpPerformed;

    #endregion
    protected int notGroundedJumpsLeft = 0;
    /// <summary>
    /// �Ƿ�����ȡ����Ծ
    /// </summary>
    protected bool isAllowedToCancelJump = false;
    protected bool wantToRun = false;
    protected float currentPlanarSpeedLimit = 0f;
    /// <summary>
    /// ��ǰ�����Ƿ������Ծ
    /// </summary>
    protected bool groundedJumpAvailable = true;
    /// <summary>
    /// ��Ծ�ķ��򣬣�����б�����壩
    /// </summary>
    protected Vector3 JumpDirection = Vector3.zero;

    protected Vector3 targetLookingDirection = Vector3.zero;
    protected float targetHeight = 1f;

    protected bool wantToCrouch = false;
    protected bool isCrouched = false;
    /// <summary>
    /// �Ƿ���������
    /// </summary>
    public bool UseGravity
    {
        get => verticalMovementParameters.useGravity;
        set => verticalMovementParameters.useGravity = value;
    }

    protected PlanarMovementParameters.PlanarMovementProperties currentMotion = new PlanarMovementParameters.PlanarMovementProperties();
    bool reducedAirControlFlag = false;
    float reducedAirControlInitialTime = 0f;
    float reductionDuration = 0.5f;

    public override void Init()
    {
        CharacterActor = (parentMachine as CharacterStateController_New).CharacterActor;
        CharacterBrain = (parentMachine as CharacterStateController_New).CharacterBrain;
        CharacterStateController = (parentMachine as CharacterStateController_New);
        notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;

        targetHeight = CharacterActor.DefaultBodySize.y;
        float minCrouchHeightRatio = CharacterActor.BodySize.x / CharacterActor.BodySize.y;
        crouchParameters.heightRatio = Mathf.Max(minCrouchHeightRatio, crouchParameters.heightRatio);
        CharacterActor.OnTeleport += OnTeleport;
    }
    public override void Enter()
    {
        CharacterActor.alwaysNotGrounded = false;
        targetLookingDirection = CharacterActor.Forward;

        currentPlanarSpeedLimit = Mathf.Max(CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

        CharacterActor.UseRootMotion = false;
        if(CharacterStateController.Animator != null && RuntimeAnimatorController != null)
        {
            CharacterStateController.Animator.runtimeAnimatorController = RuntimeAnimatorController;
        }
    }
    public override void Exit()
    {
        CharacterActor.OnTeleport -= OnTeleport;
        reducedAirControlFlag = false;
    }
    void OnTeleport(Vector3 position,Quaternion rotation)
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
    #region �ƶ���movement��
    private void HandleVelocity(float dt)
    {
        ProcessVerticalMovement(dt);
        ProcessPlanarMovement(dt);
    }
    /// <summary>
    /// ������ֱ���ٶ����
    /// </summary>
    /// <param name="dt"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ProcessVerticalMovement(float dt)
    {
        ProcessGravity(dt);
        ProcessJump(dt);
    }
    /// <summary>
    /// �����ɫƽ���ƶ�
    /// </summary>
    /// <param name="dt"></param>
    private void ProcessPlanarMovement(float dt)
    {
        float speedMultiplier = 1;
        //�Ƿ���Ҫ����
        bool needToAccalerate = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, currentPlanarSpeedLimit).sqrMagnitude >= CharacterActor.PlanarVelocity.sqrMagnitude;
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
                //�Ƿ��¶�
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

        //todo  �������������������
        float gravityMutiplier = 1f;

        float gravity = gravityMutiplier * verticalMovementParameters.gravity;
        if (!CharacterActor.IsStable)
            CharacterActor.VerticalVelocity += CustomUtilities.Multiply(-CharacterActor.Up, gravity, dt);
    }
    #endregion
    #region ��ת��rotate��
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

                targetLookingDirection = (lookingDirectionParameters.target.position - CharacterActor.Position);
                targetLookingDirection.Normalize();
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
    #region �¶�(Crouch)
    /// <summary>
    /// �¶�
    /// </summary>
    /// <param name="dt"></param>
    void Crouch(float dt)
    {
        SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ? SizeReferenceType.Bottom : crouchParameters.notGroundedReference;

        bool validSize = CharacterActor.CheckAndInterpolateHeight(
            CharacterActor.DefaultBodySize.y * crouchParameters.heightRatio,
            crouchParameters.sizeLerpSpeed * dt, sizeReferenceType);

        if (validSize)
            isCrouched = true;
    }
    /// <summary>
    /// ����
    /// </summary>
    /// <param name="dt"></param>
    void StandUp(float dt)
    {
        SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ? SizeReferenceType.Bottom : crouchParameters.notGroundedReference;

        bool validSize = CharacterActor.CheckAndInterpolateHeight(CharacterActor.DefaultBodySize.y, crouchParameters.sizeLerpSpeed * dt, sizeReferenceType);
        if (validSize)
        {
            isCrouched = false;
        }

    }
    #endregion
    #region ��Ծ(jump)
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
                //����Ѿ��ɿ�����Ծ���Ұ��µ�ʱ�䳬Խ����С�ĳ���ʱ�����϶�Ϊ���Խ�������Ծ״̬
                else if(!CharacterActions.jump.value && CharacterActions.jump.StartedElapsedTime >= verticalMovementParameters.cancelJumpMinTime)
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

            //ֻ�е��øú�������ȡ��ǿ�ƽӴ������״̬�������޷���Ծ
            if(CharacterActor.IsGrounded)
                CharacterActor.ForceNotGrounded();

            CharacterActor.Velocity -= Vector3.Project(CharacterActor.Velocity, JumpDirection);
            CharacterActor.Velocity += CustomUtilities.Multiply(JumpDirection, verticalMovementParameters.jumpSpeed);

            if (verticalMovementParameters.cancelJumpOnRelease)
            {
                isAllowedToCancelJump = true;
            }

        }

    }

    /// <summary>
    /// ������Ծ�ķ���Ŀǰ�ǽ�ɫͷ�������Ժ�����б����������
    /// </summary>
    private Vector3 SetJumpDirection()
    {
        return CharacterActor.Up;
    }
    private bool ProcessJumpDown(float dt)
    {
        if (!verticalMovementParameters.canJumpDown)
            return false;
        if (!CharacterActor.IsStable)
            return false;
        if(!CharacterActor.IsGroundAOneWayPlatform)
            return false;
        //�Ƿ�����ڵ�����й��ˣ�tag��
        if (verticalMovementParameters.filterByTag)
        {
            if(!CharacterActor.gameObject.CompareTag(verticalMovementParameters.jumpDownTag))
                return false;
        }
        //�Ƿ�ִ�������µ���Ϊ�����Ƿ��°����ӵ���ƽ̨������
        if(!ProcessJumpDownAction())
            return false;

        JumpDown(dt);
        return true;

    }
    private void JumpDown(float dt)
    {
        float groundDispalacementExtraDistance = 0f;
        Vector3 groundDisplacement = CustomUtilities.Multiply(CharacterActor.GroundVelocity, dt);
        if(!CharacterActor.IsGroundAscending)
            groundDispalacementExtraDistance = groundDisplacement.magnitude;
        CharacterActor.ForceNotGrounded();
        CharacterActor.Position -= CustomUtilities.Multiply(CharacterActor.Up, CharacterConstants.ColliderMinBottomOffset + verticalMovementParameters.jumpDownDistance + groundDispalacementExtraDistance);
    }
    private bool ProcessJumpDownAction()
    {
        return isCrouched && CharacterActions.jump.Started;
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
    }
    #region ���¶���(Update Animator Parmeters)
    public override void PreCharacterSimulation()
    {
        if (!CharacterActor.IsAnimatorValid())
            return;
        CharacterStateController.Animator.SetBool(groundParamerter, CharacterActor.IsGrounded);
        CharacterStateController.Animator.SetBool(stableParameter, CharacterActor.IsStable);
        CharacterStateController.Animator.SetFloat(horizontalAxisParameter, CharacterActions.movement.value.x);
        CharacterStateController.Animator.SetFloat(verticalAxisParameter, CharacterActions.movement.value.y);
        CharacterStateController.Animator.SetFloat(heightParameter, CharacterActor.BodySize.y);
    }
    public override void PostCharacterSimulation()
    {
        if (!CharacterActor.IsAnimatorValid())
        {
            return;
        }
        CharacterStateController.Animator.SetFloat(verticalSpeedParameter, CharacterActor.LocalVelocity.y);
        CharacterStateController.Animator.SetFloat(planarSpeedParmeter, CharacterActor.PlanarVelocity.magnitude);
    }
    #endregion
}
