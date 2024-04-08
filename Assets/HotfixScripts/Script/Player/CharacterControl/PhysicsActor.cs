using System.Collections;
using UnityEngine;
using Utilities;
using VInspector;
/// <summary>
/// ��ɫ�Ŀ��ƺ���
/// </summary>
public abstract class PhysicsActor : MonoBehaviour
{
    [Tab("Rigidbody")]
    //�Ƿ����ò�ֵ
    public bool interpolaterActor = true;
    [Tooltip("�Ƿ�����������⣬���⴩ǽ�����")]
    public bool useContinuousCollisionDetection = true;
    public bool Is2D = false;
    [Tab("Root Motion")]
    //�������ӵ���������ǻ���Ҫ�õ�rootmotion
    [Tooltip("�Ƿ�������RootMotion")]
    public bool UseRootMotion = false;
    [Condition("UseRootMotion", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    [Tooltip("�Ƿ�����rootmotion�д�������")]
    public bool UpdateRootPosition = true;
    [Tooltip("�Ƿ�����rootmotion�д�����ת")]
    [Condition("UseRootMotion", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public bool UpdateRootRotation = true;
    [Condition(new string[] { "UseRootMotion", "UpdateRootPosition" }, new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsTrue }, new float[] { 0, 0 }, ConditionAttribute.VisibilityType.NotEditable)]
    public RootMotionVelocityType rootMotionVelocityType = RootMotionVelocityType.SetVelocity;
    [Condition(new string[] { "UseRootMotion", "UpdateRootRotation" }, new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsTrue }, new float[] { 0, 0 }, ConditionAttribute.VisibilityType.NotEditable)]
    public RootMotionRotationType rootMotionRotationType = RootMotionRotationType.SetRotation;

    /// <summary>
    /// ���¼���������ģ��֮ǰ����
    /// </summary>
    public event System.Action<float> OnPreSimulation;
    /// <summary>
    /// ���¼���������ģ���Ժ����
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
    /// �����������ٶ�ֵ
    /// </summary>
    public Vector3 Velocity
    {
        get => RigidbodyComponent.Velocity;
        set => RigidbodyComponent.Velocity = value;
    }
    /// <summary>
    /// ���������µ�ˮƽ�ٶ�ֵ
    /// </summary>
    public Vector3 PlanarVelocity
    {
        get => transform.TransformDirection(LocalPlanarVelocity);
        set => LocalPlanarVelocity = transform.InverseTransformDirection(value);
    }
    /// <summary>
    /// ���������µĴ�ֱ�ٶ�ֵ
    /// </summary>
    public Vector3 VerticalVelocity
    {
        get => transform.TransformDirection(LocalVerticalVelocity);
        set => LocalVerticalVelocity = transform.InverseTransformDirection(value);
    }
    /// <summary>
    /// ��ȡ/���ø���ֲ��ٶȡ�
    /// </summary>
    public Vector3 LocalVelocity
    {
        get => transform.InverseTransformDirection(RigidbodyComponent.Velocity);
        set => RigidbodyComponent.Velocity = transform.TransformDirection(value);
    }
    /// <summary>
    /// ��ȡ/���ø���ֲ�ƽ���ٶȡ�
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
    /// ��ȡ/���ø���ֲ���ֱ�ٶȡ�
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
    /// �Ƿ�������
    /// </summary>
    public bool IsFalling => LocalVelocity.y < 0f;
    /// <summary>
    /// �Ƿ�������
    /// </summary>
    public bool IsAscending => LocalVelocity.y > 0f;
    /// <summary>
    /// ��ɫ��ʵ��λ�ã����ֱ�ӵ�����ֵ�ᵼ�����崫��
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
    /// ʵ�ʵ���תֵ��ֱ���޸Ļ�ֱ�ӵ�������ת��ȥ
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
    /// �Ƿ�Ϊ�˶�ѧ�����������ܵ�������Ӱ��
    /// </summary>
    public bool IsKinematic
    {
        get => RigidbodyComponent.IsKinematic;
        set => RigidbodyComponent.IsKinematic = value;
    }
    /// <summary>
    /// �ƶ����壬������˶�ѧ�Ļ���ͨ��ֱ������ʵ�֣�����Ƕ���ѧ�Ļ����ͨ���������ʵ��
    /// </summary>
    /// <param name="position"></param>
    public void Move(Vector3 position)
    {
        RigidbodyComponent.Move(position);
    }
    /// <summary>
    /// ���ͺ󴥷����¼�
    /// </summary>
    public event System.Action<Vector3, Quaternion> OnTeleport;
    public event System.Action OnAnimatorMoveEvent;
    public event System.Action<int> OnAnimatorIKEvent;

    /// <summary>
    /// �趨��λ�ã���ɫ�����ڲ��Լ����߼����ƶ�
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
    /// ���ݸ�����ת��ȡ��ǰ���Ϸ��򣨲�һ���� transform.up����
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
    /// ���ݸ�����ת��ȡ/���õ�ǰǰ�����򣨲�һ���� transform.forward����
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
    /// ���ݸ�����ת��ȡ��ǰ���Ϸ��򣨲�һ���� transform.right��
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


    #region ��ת
    /// <summary>
    /// ���û��ڡ���ǰ���͡����ϡ�����ת�����൱��Quaternion.LookRotation��
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
    /// ����Ϊһ��Ҫ��������������ʻ�����תһ���Ƕȣ����Կ���Rotate������
    /// </summary>
    /// <param name="forward"></param>
    public virtual void SetYaw(Vector3 forward)
    {
        Rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Forward, forward, Up), Up) * Rotation;
    }
    /// <summary>
    /// ����Up������תһ���Ƕ�
    /// </summary>
    /// <param name="angle"></param>
    public virtual void RotateYaw(float angle)
    {
        Rotation = Quaternion.AngleAxis(angle, Up) * Rotation;
    }
    /// <summary>
    /// ����Up����ת��
    /// </summary>
    /// <param name="angle">�Ƕ�</param>
    /// <param name="pivot">�ռ��е���ת���ᡣ</param>
    public virtual void RotateYaw(float angle, Vector3 pivot)
    {
        Quaternion delatRotation = Quaternion.AngleAxis(angle, Up);
        RotateAround(delatRotation, pivot);
    }
    /// <summary>
    /// ����Right����ת
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>   
    public virtual void RotatePitch(float angle)
    {
        Rotation = Quaternion.AngleAxis(angle, Right) * Rotation;
    }

    /// <summary>
    /// ����Right����ת
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>       
    /// <param name="pivot">The rotation pivot in space.</param>      
    public virtual void RotatePitch(float angle, Vector3 pivot)
    {
        Quaternion deltaRotation = Quaternion.AngleAxis(angle, Right);
        RotateAround(deltaRotation, pivot);
    }

    /// <summary>
    /// ����Forward����ת
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>  
    public virtual void RotateRoll(float angle)
    {
        Rotation = Quaternion.AngleAxis(angle, Forward) * Rotation;
    }

    /// <summary>
    /// ����Forward����ת
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>        
    /// <param name="pivot">The rotation pivot in space.</param>     
    public virtual void RotateRoll(float angle, Vector3 pivot)
    {
        Quaternion deltaRotation = Quaternion.AngleAxis(angle, Forward);
        RotateAround(deltaRotation, pivot);
    }

    /// <summary>
    /// ͨ��ִ�� 180 �ȵ�ƫ����ת�����䴹ֱ�ᣩ����ת��ɫ�����⣬��ֵ����ת�����Զ�����
    /// </summary>
    public virtual void Turnaround()
    {
        ResetInterpolationRotation();
        RotateYaw(180);
    }
    #endregion
    /// <summary>
    /// ���ڸ����Ķ�����������������붯����ص�������������ṩ���˶�����
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
    #region ��ֵ
    bool internalResetFlag = true;
    public void SyncBody()
    {
        if (!interpolaterActor)
            return;
        if (!wasInterpolatingActor)
        {
            return;
        }

        //ȡ��Rigibody��������λ��
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
    /// ���²�ֵ��Ŀ��ֵ
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
    /// ��ֹ������һ����������ڼ������λ�á�
    /// </summary>
    public void ResetInterpolationPosition() => resetPositionFlag = true;

    /// <summary>
    /// ��ֹ��һ����������ڼ�����������ת��ֵ��
    /// </summary>
    public void ResetInterpolationRotation() => resetRotationFlag = true;
    /// <summary>
    /// ��ֹ��һ����������ڼ��������в�ֵ��
    /// </summary>
    public void ResetInterpolation()
    {
        ResetInterpolationPosition();
        ResetInterpolationRotation();
    }
    #endregion
    /// <summary>
    /// �ж��Ƿ����һ����Ч�Ķ���������
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

        //��ֵ��interpolation��
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
        //�ֶ�ͬ������ֹ����ֵ����Ⱦ��
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
/// ������ν����ٶ�����Ӧ����ִ�������
/// </summary>
public enum RootMotionVelocityType
{
    /// <summary>
    /// ���˶��ٶȽ�Ӧ��Ϊ�ٶȡ�
    /// </summary>
    SetVelocity,
    /// <summary>
    /// ���˶��ٶȽ�Ӧ��Ϊƽ���ٶȡ�
    /// </summary>
    SetPlanarVelocity,
    /// <summary>
    /// ���˶��ٶȽ�Ӧ��Ϊ��ֱ�ٶȡ�
    /// </summary>
    SetVerticalVelocity,
}
/// <summary>
/// ������ν����ֻ�����Ӧ����ִ�����.
/// </summary>
public enum RootMotionRotationType
{
    /// <summary>
    /// ���˶���ת�����ǵ�ǰ��ת��
    /// </summary>
    SetRotation,
    /// <summary>
    /// ���˶���ת����ӵ���ǰ��ת�С�
    /// </summary>
    AddRotation
}
