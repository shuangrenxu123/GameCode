using System.Collections;
using UnityEngine;
using Utilities;
using VInspector;
/// <summary>
/// 角色的控制核心
/// </summary>
public abstract class PhysicsActor : MonoBehaviour
{
    [Tab("Rigidbody")]
    //是否启用插值
    public bool interpolaterActor = true;
    [Tooltip("是否启用连续检测，避免穿墙的情况")]
    public bool useContinuousCollisionDetection = true;
    public bool Is2D = false;
    [Tab("Root Motion")]
    //在爬梯子等情况下我们会需要用到rootmotion
    [Tooltip("是否启用了RootMotion")]
    public bool UseRootMotion = false;
    [Condition("UseRootMotion", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    [Tooltip("是否启用rootmotion中传递坐标")]
    public bool UpdateRootPosition = true;
    [Tooltip("是否启用rootmotion中传递旋转")]
    [Condition("UseRootMotion", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public bool UpdateRootRotation = true;
    [Condition(new string[] { "UseRootMotion", "UpdateRootPosition" }, new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsTrue }, new float[] { 0, 0 }, ConditionAttribute.VisibilityType.NotEditable)]
    public RootMotionVelocityType rootMotionVelocityType = RootMotionVelocityType.SetVelocity;
    [Condition(new string[] { "UseRootMotion", "UpdateRootRotation" }, new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsTrue }, new float[] { 0, 0 }, ConditionAttribute.VisibilityType.NotEditable)]
    public RootMotionRotationType rootMotionRotationType = RootMotionRotationType.SetRotation;

    /// <summary>
    /// 该事件会在物理模拟之前调用
    /// </summary>
    public event System.Action<float> OnPreSimulation;
    /// <summary>
    /// 该事件会在物理模拟以后调用
    /// </summary>
    public event System.Action<float> OnPostSimulation;

    Vector3 startingPosition;
    Vector3 targetPosition;
    Quaternion startingRotation;
    Quaternion targetRotation;
    AnimatorLink animatorLink = null;
    Coroutine postSimulationUpdateCoroutine;

    bool wasInterpolatingActor = false;

    public abstract RigidbodyComponent RigidbodyComponent { get; }
    public Animator Animator { get; private set; }
    /// <summary>
    /// 世界坐标下速度值
    /// </summary>
    public Vector3 Velocity
    {
        get => RigidbodyComponent.Velocity;
        set => RigidbodyComponent.Velocity = value;
    }
    /// <summary>
    /// 世界坐标下的水平速度值
    /// </summary>
    public Vector3 PlanarVelocity
    {
        get => transform.TransformDirection(LocalPlanarVelocity);
        set => LocalPlanarVelocity = transform.InverseTransformDirection(value);
    }
    /// <summary>
    /// 世界坐标下的垂直速度值
    /// </summary>
    public Vector3 VerticalVelocity
    {
        get => transform.TransformDirection(LocalVerticalVelocity);
        set => LocalVerticalVelocity = transform.InverseTransformDirection(value);
    }
    /// <summary>
    /// 获取/设置刚体局部速度。
    /// </summary>
    public Vector3 LocalVelocity
    {
        get => transform.InverseTransformDirection(RigidbodyComponent.Velocity);
        set => RigidbodyComponent.Velocity = transform.TransformDirection(value);
    }
    /// <summary>
    /// 获取/设置刚体局部平面速度。
    /// </summary>
    public Vector3 LocalPlanarVelocity
    {
        get
        {
            Vector3 localVelocity = LocalVelocity;
            localVelocity.y = 0f;
            return localVelocity;
        }
        set
        {
            value.y = 0f;
            LocalVelocity = value + LocalVerticalVelocity;
        }
    }
    /// <summary>
    /// 获取/设置刚体局部垂直速度。
    /// </summary>
    public Vector3 LocalVerticalVelocity
    {
        get
        {
            Vector3 localVelocity = LocalVelocity;
            localVelocity.x = localVelocity.z = 0f;
            return localVelocity;
        }
        set
        {
            value.x = value.z = 0f;
            LocalVelocity = LocalPlanarVelocity + value;
        }
    }

    /// <summary>
    /// 是否下落中
    /// </summary>
    public bool IsFalling => LocalVelocity.y < 0f;
    /// <summary>
    /// 是否上升中
    /// </summary>
    public bool IsAscending => LocalVelocity.y > 0f;
    /// <summary>
    /// 角色的实际位置，如果直接调整该值会导致物体传送
    /// </summary>
    public Vector3 Position
    {
        get => RigidbodyComponent.Position;
        set
        {
            RigidbodyComponent.Position = value;
            targetPosition = value;
        }
    }
    /// <summary>
    /// 实际的旋转值，直接修改会直接导致物体转过去
    /// </summary>
    public Quaternion Rotation
    {
        get => transform.rotation;
        set
        {
            transform.rotation = value;
            targetRotation = value;
        }
    }
    /// <summary>
    /// 是否为运动学，即他不会受到外力的影响
    /// </summary>
    public bool IsKinematic
    {
        get => RigidbodyComponent.IsKinematic;
        set => RigidbodyComponent.IsKinematic = value;
    }
    /// <summary>
    /// 移动物体，如果是运动学的话会通过直接坐标实现，如果是动力学的话则会通过添加力来实现
    /// </summary>
    /// <param name="position"></param>
    public void Move(Vector3 position)
    {
        RigidbodyComponent.Move(position);
    }
    /// <summary>
    /// 传送后触发的事件
    /// </summary>
    public event System.Action<Vector3, Quaternion> OnTeleport;
    public event System.Action OnAnimatorMoveEvent;
    public event System.Action<int> OnAnimatorIKEvent;

    /// <summary>
    /// 设定好位置，角色会以内部自己的逻辑来移动
    /// </summary>
    /// <param name="position"></param>
    public void Teleport(Vector3 position)
    {
        Teleport(position, Rotation);
    }
    public void Teleport(Quaternion rotation)
    {
        Teleport(Position, rotation);
    }
    public void Teleport(Transform reference) => Teleport(reference.position, reference.rotation);
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;

        ResetInterpolationPosition();
        ResetInterpolationRotation();

        OnTeleport?.Invoke(Position, Rotation);

    }

    /// <summary>
    /// 根据刚体旋转获取当前向上方向（不一定是 transform.up）。
    /// </summary>
    public virtual Vector3 Up
    {
        get
        {
            return Rotation * Vector3.up;
        }
        set
        {
            Quaternion deltaRotation = Quaternion.FromToRotation(Up, value);
            Rotation = deltaRotation * Rotation;
        }
    }
    /// <summary>
    /// 根据刚体旋转获取/设置当前前进方向（不一定是 transform.forward）。
    /// </summary>
    public virtual Vector3 Forward
    {
        get
        {
            return Rotation * Vector3.forward;
        }
        set
        {
            Quaternion deltaRotation = Quaternion.FromToRotation(Forward, value);
            Rotation = deltaRotation * Rotation;
        }
    }

    /// <summary>
    /// 根据刚体旋转获取当前向上方向（不一定是 transform.right）
    /// </summary>
    public virtual Vector3 Right
    {
        get
        {
            return Rotation * Vector3.right;
        }
        set
        {
            Quaternion deltaRotation = Quaternion.FromToRotation(Right, value);
            Rotation = deltaRotation * Rotation;
        }
    }


    #region 旋转
    /// <summary>
    /// 设置基于“向前”和“向上”的旋转。这相当于Quaternion.LookRotation。
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="up"></param>
    public virtual void SetRotation(Vector3 forward, Vector3 up)
    {
        Rotation = Quaternion.LookRotation(forward, up);
    }
    /// <summary>
    ///
    /// </summary>
    /// <param name="deltaRotation"></param>
    /// <param name="pivot"></param>
    public virtual void RotateAround(Quaternion deltaRotation, Vector3 pivot)
    {
        Vector3 preReferenceToPivot = pivot - Position;
        Rotation = deltaRotation * Rotation;
        Vector3 postReferEnceToPivot = deltaRotation * preReferenceToPivot;
        Position += preReferenceToPivot - postReferEnceToPivot;
    }
    /// <summary>
    /// 参数为一个要朝向的向量，本质还是旋转一个角度，可以看作Rotate的重载
    /// </summary>
    /// <param name="forward"></param>
    public virtual void SetYaw(Vector3 forward)
    {
        Rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Forward, forward, Up), Up) * Rotation;
    }
    /// <summary>
    /// 绕着Up轴来旋转一个角度
    /// </summary>
    /// <param name="angle"></param>
    public virtual void RotateYaw(float angle)
    {
        Rotation = Quaternion.AngleAxis(angle, Up) * Rotation;
    }
    /// <summary>
    /// 沿着Up轴旋转，
    /// </summary>
    /// <param name="angle">角度</param>
    /// <param name="pivot">空间中的旋转枢轴。</param>
    public virtual void RotateYaw(float angle, Vector3 pivot)
    {
        Quaternion delatRotation = Quaternion.AngleAxis(angle, Up);
        RotateAround(delatRotation, pivot);
    }
    /// <summary>
    /// 绕着Right轴旋转
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>   
    public virtual void RotatePitch(float angle)
    {
        Rotation = Quaternion.AngleAxis(angle, Right) * Rotation;
    }

    /// <summary>
    /// 绕着Right轴旋转
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>       
    /// <param name="pivot">The rotation pivot in space.</param>      
    public virtual void RotatePitch(float angle, Vector3 pivot)
    {
        Quaternion deltaRotation = Quaternion.AngleAxis(angle, Right);
        RotateAround(deltaRotation, pivot);
    }

    /// <summary>
    /// 绕着Forward轴旋转
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>  
    public virtual void RotateRoll(float angle)
    {
        Rotation = Quaternion.AngleAxis(angle, Forward) * Rotation;
    }

    /// <summary>
    /// 绕着Forward轴旋转
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>        
    /// <param name="pivot">The rotation pivot in space.</param>     
    public virtual void RotateRoll(float angle, Vector3 pivot)
    {
        Quaternion deltaRotation = Quaternion.AngleAxis(angle, Forward);
        RotateAround(deltaRotation, pivot);
    }

    /// <summary>
    /// 通过执行 180 度的偏航旋转（绕其垂直轴）来旋转角色。此外，插值（旋转）会自动重置
    /// </summary>
    public virtual void Turnaround()
    {
        ResetInterpolationRotation();
        RotateYaw(180);
    }
    #endregion
    /// <summary>
    /// 基于给定的动画器组件配置所有与动画相关的组件。动画器提供根运动数据
    /// </summary>
    public void InitializeAnimation()
    {
        Animator = this.GetComponentInBranch<CharacterActor, Animator>();

#if UNITY_2023_1_OR_NEWER
            Animator.updateMode = AnimatorUpdateMode.Fixed;
#else
        Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
#endif
        if (!Animator.TryGetComponent(out animatorLink))
        {
            animatorLink = Animator.gameObject.AddComponent<AnimatorLink>();
        }
    }

    public void ResetIKWeights()
    {
        if (animatorLink != null)
        {
            animatorLink.ResetIkWeights();
        }
    }
    protected virtual void PreSimulationUpdate(float dt) { }
    protected virtual void PostSimulationUpdate(float dt) { }

    protected virtual void UpdateKinematicRootMotionPosition()
    {
        if (!UpdateRootPosition)
        {
            return;
        }
        Position += Animator.deltaPosition;
    }
    protected virtual void UpdateKinematicRootMotionRotatino()
    {
        if (!UpdateRootRotation)
            return;
        if (rootMotionRotationType == RootMotionRotationType.AddRotation)
            Rotation *= Animator.deltaRotation;
        else
            Rotation = Animator.rootRotation;
    }

    protected virtual void UpdateDynamicRootMotionPosition()
    {
        if (!UpdateRootPosition)
            return;

        RigidbodyComponent.Move(Position + Animator.deltaPosition);
    }
    protected virtual void UpdateDynamicRootMotionRotation()
    {
        if (!UpdateRootRotation)
            return;
        if (rootMotionRotationType == RootMotionRotationType.AddRotation)
            Rotation *= Animator.deltaRotation;
        else
            Rotation = Animator.rootRotation;
    }
    void PresimulationRootMotionUpdate()
    {
        if (RigidbodyComponent.IsKinematic)
        {
            if (UpdateRootPosition)
                UpdateKinematicRootMotionPosition();
            if (UpdateRootRotation)
                UpdateKinematicRootMotionRotatino();
        }
        else
        {
            if (UpdateRootPosition)
                UpdateDynamicRootMotionPosition();
            if (UpdateRootRotation)
                UpdateKinematicRootMotionRotatino();
        }
    }
    void OnAnimatorIKLinkMethod(int layerIndex) => OnAnimatorIKEvent?.Invoke(layerIndex);
    #region 插值
    bool internalResetFlag = true;
    public void SyncBody()
    {
        if (!interpolaterActor)
            return;
        if (!wasInterpolatingActor)
        {
            return;
        }

        //取消Rigibody所带来的位移
        Position = startingPosition = targetPosition;
        Rotation = startingRotation = targetRotation;

        if (internalResetFlag)
        {
            internalResetFlag = false;
            resetPositionFlag = false;
            resetRotationFlag = false;
        }
    }


    public void InterpolateBody()
    {
        if (!interpolaterActor)
            return;
        if (wasInterpolatingActor)
        {
            float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            transform.SetLocalPositionAndRotation(
                resetPositionFlag ? targetPosition : Vector3.Lerp(startingPosition, targetPosition, interpolationFactor),
                resetRotationFlag ? targetRotation : Quaternion.Slerp(startingRotation, targetRotation, interpolationFactor)
                );
        }
        else
        {
            ResetInterpolationPosition();
            ResetInterpolationRotation();
        }
    }
    /// <summary>
    /// 更新插值的目标值
    /// </summary>
    public void UpdateInterpolationTargets()
    {
        if (!interpolaterActor)
        {
            return;
        }
        targetPosition = Position;
        targetRotation = Rotation;
        if (resetPositionFlag)
        {
            startingPosition = targetPosition;
        }
        if (resetRotationFlag)
        {
            startingRotation = targetRotation;
        }
    }

    bool resetPositionFlag = false;
    bool resetRotationFlag = false;

    /// <summary>
    /// 防止主体在一次物理更新期间插入其位置。
    /// </summary>
    public void ResetInterpolationPosition() => resetPositionFlag = true;

    /// <summary>
    /// 防止在一次物理更新期间对主体进行旋转插值。
    /// </summary>
    public void ResetInterpolationRotation() => resetRotationFlag = true;
    /// <summary>
    /// 防止在一次物理更新期间对主体进行插值。
    /// </summary>
    public void ResetInterpolation()
    {
        ResetInterpolationPosition();
        ResetInterpolationRotation();
    }
    #endregion
    /// <summary>
    /// 判断是否存在一个有效的动画控制器
    /// </summary>
    /// <returns></returns>
    public bool IsAnimatorValid()
    {
        if (Animator == null)
            return false;
        if (Animator.runtimeAnimatorController == null)
            return false;
        if (!Animator.gameObject.activeSelf)
            return false;

        return true;
    }

    protected virtual void Awake()
    {
        gameObject.GetOrAddComponent<PhysicsActorSync>();
        InitializeAnimation();
    }

    protected virtual void OnEnable()
    {
        postSimulationUpdateCoroutine ??= StartCoroutine(PostSimulationUpdate());

        if (animatorLink != null)
        {
            animatorLink.OnAnimatorMoveEvent += OnAnimatorMoveLinkMethod;
            animatorLink.OnAnimatorIKEvent += OnAnimatorIKLinkMethod;
        }
        startingPosition = targetPosition = transform.position;
        startingRotation = targetRotation = transform.rotation;

        ResetInterpolationPosition();
        ResetInterpolationRotation();
    }
    protected virtual void OnDisable()
    {
        if (postSimulationUpdateCoroutine != null)
        {
            StopCoroutine(postSimulationUpdateCoroutine);
            postSimulationUpdateCoroutine = null;
        }
        if (animatorLink != null)
        {
            animatorLink.OnAnimatorMoveEvent -= OnAnimatorMoveLinkMethod;
            animatorLink.OnAnimatorIKEvent -= OnAnimatorIKLinkMethod;
        }
    }
    protected virtual void Start()
    {
        RigidbodyComponent.ContinuousCollisionDetection = useContinuousCollisionDetection;
        RigidbodyComponent.UseInterpolation = false;

        //插值（interpolation）
        targetPosition = startingPosition = transform.position;
        targetRotation = startingRotation = transform.rotation;
    }

    private void Update()
    {
        InterpolateBody();

        wasInterpolatingActor = interpolaterActor;
        internalResetFlag = true;
    }
    void OnAnimatorMoveLinkMethod()
    {
        if (!enabled || !UseRootMotion)
        {
            return;
        }
        float dt = Time.deltaTime;
        OnAnimatorMoveEvent?.Invoke();

        PresimulationRootMotionUpdate();
        PreSimulationUpdate(dt);
        OnPreSimulation?.Invoke(dt);
        transform.SetPositionAndRotation(Position, Rotation);

    }
    private void FixedUpdate()
    {
        if (UseRootMotion)
            return;
        float dt = Time.deltaTime;
        PreSimulationUpdate(dt);
        OnPreSimulation?.Invoke(dt);
        //手动同步，防止坐标值被污染。
        transform.SetLocalPositionAndRotation(Position, Rotation);

    }

    IEnumerator PostSimulationUpdate()
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();
        while (true)
        {
            yield return waitForFixedUpdate;
            float dt = Time.deltaTime;

            if (enabled)
            {
                PostSimulationUpdate(dt);
                OnPostSimulation?.Invoke(dt);
                UpdateInterpolationTargets();
            }
        }
    }
}
/// <summary>
/// 定义如何将根速度数据应用于执行组件。
/// </summary>
public enum RootMotionVelocityType
{
    /// <summary>
    /// 根运动速度将应用为速度。
    /// </summary>
    SetVelocity,
    /// <summary>
    /// 根运动速度将应用为平面速度。
    /// </summary>
    SetPlanarVelocity,
    /// <summary>
    /// 根运动速度将应用为垂直速度。
    /// </summary>
    SetVerticalVelocity,
}
/// <summary>
/// 定义如何将根轮换数据应用于执行组件.
/// </summary>
public enum RootMotionRotationType
{
    /// <summary>
    /// 根运动旋转将覆盖当前旋转。
    /// </summary>
    SetRotation,
    /// <summary>
    /// 根运动旋转将添加到当前旋转中。
    /// </summary>
    AddRotation
}
