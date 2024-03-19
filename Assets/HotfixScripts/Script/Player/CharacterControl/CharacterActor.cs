using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using VInspector;
[RequireComponent(typeof(CharacterBody))]
[DefaultExecutionOrder(10)]
public class CharacterActor : PhysicsActor
{
    #region 相关变量
    [Tab("单项平台设置")]
    public LayerMask oneWayPlatformsLayerMask = 0;

    [Range(0, 179f)]
    public float oneWayPlayformsValidArc = 175;

    [Tab("stable")]
    [Tooltip("坡度限制,只有当地面坡度必须小于或等于此值的时候才能保持稳定， 角度的计算是使用地面法线来计算，即可以爬上倾斜度更高的坡")]
    public float slopeLimit = 55f;
    [Tooltip("没有被此图层标记的对象将会被视为不稳定对象")]
    public LayerMask stableLayerMask = -1;
    [Tooltip("是否允许将其他Character视为稳定表面")]
    public bool allowCharactersAsStableSurfaces = true;
    [Tooltip("在平面游戏中可能会因为速度过高导致翻越一些不能被翻过的障碍，该值可以通过删除额外的速度值来防止情况")]
    public bool preventtUnstableClimbing = true;
    [Tooltip("这可以防止角色跨过不稳定的表面，如果不需要这种级别的精度可以禁用他")]
    public bool preventBadSteps = true;

    [Tab("Step handing")]
    [Tooltip("应用于Character底部的偏移距离。更高的偏移量意味着更多的可步行表面,即可以上更高的楼梯")]
    public float stepUpDistance = 0.5f;
    [Tooltip("Character能检测地面的距离，使用该变量将Character固定在地面上")]
    public float stepDownDistance = 0.5f;

    [Tab("Grounding")]
    [Tooltip("“防止Character进入接地状态（IsGrounded 将是false")]
    public bool alwaysNotGrounded = false;

    [Tooltip("该值会在游戏开始的时候进行地面检测，如果没检测到那么IsGround将会是false")]
    [Condition("alwaysNotGrounded", ConditionAttribute.ConditionType.IsFalse)]
    public bool forceGroundedAtStart = true;

    [Tooltip("是否启用OnTrigger等触发器函数（与地面），一般来说他们不会被触发，因为我们的角色是悬空的")]
    public bool useGroundTrigger = true;
    [Tooltip("启用该值以后，角色底部的将会被模拟为圆柱体，仅在边缘时候有效")]
    public bool edgeCompensation = false;
    [Tooltip("当角色与地面接触且站在了稳定边缘的时候 " +
        "ture:无论怎么碰撞，字符都将会进入稳定状态" +
        "false ： 角色将使用接触角来确定稳定性（值小于斜率限制）")]
    public bool useStableEdgeWhenLanding = true;
    [Tooltip("如果角色的垂直速度为正，则字符是否检测到新的（和有效的）地面")]
    public bool detectGroundWhileAscending = false;

    [Tab("Dynamic ground")]
    [Tooltip("角色是否应该受到地面运动的影响")]
    public bool supportDynamicGround = true;
    public LayerMask dynamicGroundLayerMask = -1;
    [Tooltip("角色的正方向会不会受到地面移动的影响")]
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public bool rotateForwardDirection = true;
    [Tooltip("这是角色能忍受的最大地面速度，如果地面速度太快，角色将会停止移动")]
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float maxGroundVelocityChange = 30f;
    [Tooltip("继承动态地面速度所需的最小值（转化为水平速度）")]
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float inheritedGroundPlannarlVelocityThreshold = 2f;
    [Tooltip("继承动态地面速度的系数（转化为水平速度）")]
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float inheritedGroundPlanarVelocityMultiplier = 1f;
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    [Tooltip("继承动态地面速度的系数（转化为垂直速度）")]
    public float inheritedGroundVerticalVelocityThreshold = 2f;
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    [Tooltip("继承动态地面速度的系数（转化为垂直速度）")]
    public float inheritedGroundVerticalVelocityMultiplier = 1f;

    [Tab("Velocity")]
    public bool slideOnWalls = true;

    [SerializeField]
    //传送后是否重置速度
    bool resetVelocityOnTeleport = true;

    public CharacterVelocityMode stablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;
    public CharacterVelocityMode unstablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;

    [Tab("Rotation")]
    [Tooltip("该组件是否需要重新定义角色的坐标轴")]
    public bool constraintRotation = true;

    /// <summary>
    /// 如果需要重新定义坐标轴，对向上的Transform的引用
    /// </summary>
    [Condition("constraintRotation", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public Transform upDirectionReference = null;


    [Condition(
    new string[] { "constraintRotation", "upDirectionReference" },
    new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsNull },
    new float[] { 0f, 0f },
    ConditionAttribute.VisibilityType.Hidden)]
    /// <summary>
    /// 所需的向上方向
    /// </summary>
    public Vector3 constraintUpDirection = Vector3.up;


    [Condition(
     new string[] { "constraintRotation", "upDirectionReference" },
     new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsNotNull },
     new float[] { 0f, 0f },
     ConditionAttribute.VisibilityType.Hidden)]
    public VerticalAlignmentSettings.VerticalReferenceMode upDirectionReferenceMode = VerticalAlignmentSettings.VerticalReferenceMode.Away;


    [Tab("Physics")]
    public bool CanPushDynamicRigidbodies = true;
    [Condition("CanPushDynamicRigidbodies", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public LayerMask pushableRigidbodyLayerMask = -1;

    public bool applyWeightToGround = true;
    [Condition("applyWeightToGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public LayerMask applyWeightLayerMask = -1;
    [Condition("applyWeightToGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float weightGravity = CharacterConstants.DefaultGravity;

    Vector3 groundToCharacter;
    bool forceNotGroundedFlag = false;
    int forceNotGroundedFrames = 0;
    bool inheritVelocityFlag = false;
    Quaternion preSimulationFroundRotation;
    Vector3 preSimulationPosition;
    RigidbodyComponent groundRigidbodyComponent = null;
    SphereCollider groundTriggerCollider3D = null;
    float unstableGroundContactTime = 0f;

    ColliderComponent.PentrationDelegate _removePenetrationAction;

    public Vector2 BodySize { get; private set; }

    public Vector2 DefaultBodySize => CharacterBody.BodySize;
    public float StepOffset => stepUpDistance - BodySize.x / 2f;

    /// <summary>
    /// 设置root motion相关
    /// </summary>
    /// <param name="updateRootPosition"></param>
    /// <param name="updateRootRotation"></param>

    public CharacterBody CharacterBody { get; private set; }
    public override RigidbodyComponent RigidbodyComponent => CharacterBody.RigidbodyComponent;

    public ColliderComponent ColliderComponent => CharacterBody.ColliderComponent;

    public PhysicsComponent PhysicsComponent => CharacterCollisions.PhysicsComponent;

    protected CharacterCollisionInfo characterCollisionInfo = new CharacterCollisionInfo();
    /// <summary>
    /// 当前的角色地面状态，分为1.在稳定地面 2.运动地面 3.不在地面上
    /// </summary>
    public CharacterActorState CurrentState
    {
        get
        {
            if (IsGrounded)
                return IsStable ? CharacterActorState.StableGrounded : CharacterActorState.UnstableGrounded;
            else
                return CharacterActorState.NotGrounded;
        }
    }
    /// <summary>
    /// 返回上一帧的角色状态
    /// </summary>
    public CharacterActorState PreviousState
    {
        get
        {
            if (WasGrounded)
                return WasStable ? CharacterActorState.StableGrounded : CharacterActorState.UnstableGrounded;
            else
                return CharacterActorState.NotGrounded;
        }
    }
    #endregion
    public void SetUpRootMotion(bool updateRootPosition = true, bool updateRootRotation = true)
    {
        UseRootMotion = true;
        UpdateRootPosition = updateRootPosition;
        UpdateRootRotation = updateRootRotation;
    }
    public void SetupRootMotion(bool updateRootPosition = true, RootMotionVelocityType rootMotionVelocityType = RootMotionVelocityType.SetVelocity,
        bool updateRootRotation = true, RootMotionRotationType rootMotionRotationType = RootMotionRotationType.AddRotation)
    {
        UseRootMotion = true;
        UpdateRootPosition = updateRootPosition;
        UpdateRootRotation = updateRootRotation;
        this.rootMotionRotationType = rootMotionRotationType;
        this.rootMotionVelocityType = rootMotionVelocityType;
    }

    #region Collision
    #region Ground
    public LayerMask ObstaclesLayerMask => PhysicsComponent.CollisionLayerMask | oneWayPlatformsLayerMask;
    public LayerMask ObstaclesWithoutOWPLayerMask => PhysicsComponent.CollisionLayerMask & ~(oneWayPlatformsLayerMask);

    /// <summary>
    ///获取包含有关角色碰撞的所有信息的结构。大多数字符属性（例如 IsGrounded、IsStable、GroundObject 等）
    ///可以从这种结构中获得。
    /// </summary>
    public CharacterCollisionInfo CharacterCollisionInfo => characterCollisionInfo;



    /// <summary>
    /// 角色是否在边缘上
    /// </summary>
    public bool IsOnEdge => characterCollisionInfo.isOnEdge;
    /// <summary>
    /// 边缘角度
    /// </summary>
    public float EdgeAngle => characterCollisionInfo.edgeAngle;
    /// <summary>
    /// 是否在地面上
    /// </summary>
    public bool IsGrounded { get; private set; }
    /// <summary>
    /// 获取角色的UP轴向量和稳定法线之间的角度。
    /// </summary>
    public float GroundSlopeAngle => characterCollisionInfo.groundSlopeAngle;
    /// <summary>
    /// 地面获得的接触点。
    /// </summary>
    public Vector3 GroundContactPoint => characterCollisionInfo.groundContactPoint;
    public Vector3 GroundContactNormal => characterCollisionInfo.groundContactNormal;

    public Vector3 GroundStableNormal => IsStable ? characterCollisionInfo.groundStableNormal : Up;

    public GameObject GroundObject => characterCollisionInfo.groundObject;
    public Transform GroundTransform => GroundObject != null ? GroundObject.transform : null;
    public Collider2D GroundCollider2D => characterCollisionInfo.groundCollider2D;
    public Collider GroundCollider3D => characterCollisionInfo.groundCollider3D;

    public Rigidbody2D GroundRigidbody2D => characterCollisionInfo.groundRigidbody2D;
    public Rigidbody GroundRigidbody3D => characterCollisionInfo.groundRigidbody3D;
    #endregion
    #region Wall
    public bool wallCollision => characterCollisionInfo.wallCollision;
    public float wallAngle => characterCollisionInfo.wallAngle;
    public Contact wallContact => characterCollisionInfo.wallContact;
    #endregion
    #region head
    /// <summary>
    /// 获取角色的当前稳定性状态。稳定性等于“接地+坡度角<=坡度极限”。
    /// </summary>
    public bool IsStable { get; private set; }
    public bool HeadCollision => characterCollisionInfo.headCollision;
    public float HaedAngle => characterCollisionInfo.headAngle;
    public Contact HeadContact => characterCollisionInfo.headContact;
    public bool IsOnUnstableGround => IsGrounded && characterCollisionInfo.groundSlopeAngle > slopeLimit;
    /// <summary>
    /// 获取以前的地面状态
    /// </summary>
    public bool WasGrounded { get; private set; }
    /// <summary>
    /// 返回上一帧的稳定状态
    /// </summary>
    public bool WasStable { get; private set; }
    /// <summary>
    /// 一个属性，指示角色在上一次物理更新期间是否已接地。
    /// </summary>
    public bool HasBecomeGrounded { get; private set; }
    public bool HasBecomeStable { get; private set; }
    public bool HasBecomeNotGrounded { get; private set; }
    public bool HasBecomeUnStable { get; private set; }

    public RigidbodyComponent GroundRigidbdyCompoent
    {
        get
        {
            if (!IsStable)
                groundRigidbodyComponent = null;
            return groundRigidbodyComponent;
        }
    }

    public Vector3 GroundPosition => GroundRigidbody3D.position;
    public Quaternion GroundRotation => GroundRigidbody3D.rotation;
    public bool IsGroundRigidbody => characterCollisionInfo.groundRigidbody3D != null;

    public bool IsGroundKinematicRigidbody => characterCollisionInfo.groundRigidbody3D.isKinematic;

    public Vector3 GetGroundPointVelocity(Vector3 point)
    {
        if (!IsGroundRigidbody)
            return Vector3.zero;
        return characterCollisionInfo.groundRigidbody3D.GetPointVelocity(point);
    }
    Dictionary<Transform, RigidbodyComponent> groundRigidbodyComponents = new Dictionary<Transform, RigidbodyComponent>();
    #endregion

    #endregion

    public float GroundedTime { get; private set; }
    public float NotGroundedTime { get; private set; }

    public float StableElapsedTime { get; private set; }
    public float UnStableElapsedTime { get; private set; }

    /// <summary>
    /// 获取/设置投影到由其向上方向形成的平面上的刚体速度。
    /// </summary>
    public Vector3 StableVelocity
    {
        get
        {
            return CustomUtilities.ProjectOnTangent(Velocity, GroundStableNormal, Up);
        }
        set
        {
            Velocity = CustomUtilities.ProjectOnTangent(value, GroundStableNormal, Up);
        }
    }
    public Vector3 LastGroundedVelocity { get; private set; }

    #region public Body properties
    public Vector3 Center
    {
        get
        {
            return GetCenter(Position);
        }
    }

    public Vector3 Top
    {
        get
        {
            return GetTop(Position);
        }
    }
    public Vector3 Bottom
    {
        get
        {
            return GetBottom(Position);
        }
    }
    public Vector3 TopCenter
    {
        get
        {
            return GetTopCenter(Position);
        }
    }
    public Vector3 BottomCenter
    {
        get
        {
            return GetBottomCenter(Position, 0f);
        }
    }
    public Vector3 OffsetedBottomCenter
    {
        get
        {
            return GetBottomCenter(Position, StepOffset);
        }
    }
    #endregion

    #region Body function
    public Vector3 GetCenter(Vector3 position)
    {
        return position + CustomUtilities.Multiply(Up, BodySize.y / 2f);
    }
    public Vector3 GetTop(Vector3 position)
    {
        return position + CustomUtilities.Multiply(Up, BodySize.y - CharacterConstants.SkinWidth);
    }
    public Vector3 GetBottom(Vector3 position)
    {
        return position + CustomUtilities.Multiply(Up, CharacterConstants.SkinWidth);
    }
    public Vector3 GetTopCenter(Vector3 position)
    {
        return position + CustomUtilities.Multiply(Up, BodySize.y - BodySize.x / 2f);
    }
    public Vector3 GetTopCenter(Vector3 position, Vector2 bodysize)
    {
        return position + CustomUtilities.Multiply(Up, bodysize.y - bodysize.x / 2f);
    }
    public Vector3 GetBottomCenter(Vector3 position, float bottomOffset = 0f)
    {
        return position + CustomUtilities.Multiply(Up, BodySize.x / 2f + bottomOffset);
    }
    public Vector3 GetBottomCenter(Vector3 position, Vector2 bodySize)
    {
        return position + CustomUtilities.Multiply(Up, bodySize.x / 2f);
    }
    public Vector3 GetBottomCenterToTopCenter()
    {
        return CustomUtilities.Multiply(Up, BodySize.y - BodySize.x);
    }
    public Vector3 GetBottomCenterToTopCenter(Vector2 bodySize)
    {
        return CustomUtilities.Multiply(Up, BodySize.y - bodySize.x);
    }
    #endregion

    public CharacterCollisions CharacterCollisions { get; private set; }
    HitFilterDelegate _collisionHitFilter;

    #region Unity生命周期
    protected override void Awake()
    {
        base.Awake();
        CharacterBody = GetComponent<CharacterBody>();
        BodySize = CharacterBody.BodySize;
        CharacterCollisions = CharacterCollisions.CreateInstance(gameObject);

        RigidbodyComponent.IsKinematic = false;
        RigidbodyComponent.UseGravity = false;
        RigidbodyComponent.Mass = CharacterBody.Mass;
        RigidbodyComponent.LinerDrag = 0f;
        RigidbodyComponent.AngularDrag = 0f;
        RigidbodyComponent.Constraints = RigidbodyConstraints.FreezeRotation;

        groundTriggerCollider3D = gameObject.AddComponent<SphereCollider>();
        groundTriggerCollider3D.hideFlags = HideFlags.NotEditable;
        groundTriggerCollider3D.isTrigger = true;
        groundTriggerCollider3D.radius = BodySize.x / 2f;
        groundTriggerCollider3D.center = Vector3.up * (BodySize.x / 2f - CharacterConstants.GroundTriggerOffset);
        //忽略碰撞
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), groundTriggerCollider3D, true);

        _removePenetrationAction = RemovePenetrationAction;
        _collisionHitFilter = CollisionHitFilter;
    }

    protected override void Start()
    {
        base.Start();

        HitInfoFilter filter = new HitInfoFilter(
            ObstaclesLayerMask,
            false,
            true,
            oneWayPlatformsLayerMask
            );
        CharacterCollisions.CheckOverlap(Position, 0f, in filter, _collisionHitFilter);

        if (forceGroundedAtStart && !alwaysNotGrounded)
        {
            ForceGrounded();
        }
        SetColliderSize();
    }
    /// <summary>
    /// 暂时用于注册相关事件
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        OnTeleport += OnTeleportMethod;

    }
    /// <summary>
    /// 暂时用于取消所注册的相关事件
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        OnTeleport -= OnTeleportMethod;
    }
    #endregion
    /// <summary>
    /// 传送后的事件，暂时只有清除速度
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    void OnTeleportMethod(Vector3 position, Quaternion rotation)
    {
        if (resetVelocityOnTeleport)
        {
            Velocity = Vector3.zero;
        }
    }
    /// <summary>
    /// 在地面接触点沿重量方向（质量乘以重力）施加力。
    /// </summary>
    /// <param name="contactPoint"></param>
    protected virtual void ApplyWeight(Vector3 contactPoint)
    {
        if (!applyWeightToGround)
        {
            return;
        }
        if (GroundObject == null)
        {
            return;
        }
        if (!CustomUtilities.BelongsToLayerMask(GroundObject.layer, applyWeightLayerMask))
        {
            return;
        }

        if (GroundCollider3D?.attachedRigidbody == null)
        {
            return;
        }
        GroundCollider3D.attachedRigidbody.AddForceAtPosition(CustomUtilities.Multiply(-Up, CharacterBody.Mass, weightGravity), contactPoint);

    }
    /// <summary>
    /// 返回物体所接触的信息
    /// </summary>
    public List<Contact> Contacts
    {
        get
        {
            if (PhysicsComponent == null)
                return null;
            return PhysicsComponent.Contacts;
        }
    }
    /// <summary>
    /// 获取最新的Trigger。
    /// </summary>
    public Trigger CurrentTrigger
    {
        get
        {
            if (PhysicsComponent.Triggers.Count == 0)
                return new Trigger();
            return PhysicsComponent.Triggers[PhysicsComponent.Triggers.Count - 1];
        }
    }
    public List<Trigger> Triggers
    {
        get
        {
            return PhysicsComponent.Triggers;
        }
    }
    /// <summary>
    /// 获取到在FixUpdate调用之前输入的速度值，由玩家来输入确定
    /// </summary>
    public Vector3 InputVelocity { get; private set; }
    /// <summary>
    /// 将世界空间下的速度应用到本地空间下
    /// </summary>
    public Vector3 LocalInputVelocity => transform.InverseTransformDirection(InputVelocity);
    /// <summary>
    /// 模拟前的速度
    /// </summary>
    public Vector3 PreSimulationVelocity { get; private set; }
    /// <summary>
    /// 物理模拟后的速度
    /// </summary>
    public Vector3 PostSimulationVelocity { get; private set; }
    /// <summary>
    /// 外界所提供的速度
    /// </summary>
    public Vector3 ExternalVelocity { get; private set; }
    /// <summary>
    /// 处理旋转
    /// </summary>
    void HandleRotation()
    {
        if (!constraintRotation)
            return;
        if (upDirectionReference != null)
        {
            Vector3 targetPosition = Position;
            float sign = upDirectionReferenceMode == VerticalAlignmentSettings.VerticalReferenceMode.Towards ? 1f : -1f;
            Vector3 referenceToTarget = upDirectionReference.position - targetPosition;
            referenceToTarget.Normalize();
            constraintUpDirection = CustomUtilities.Multiply(referenceToTarget, sign);
        }
        Up = constraintUpDirection;
    }
    /// <summary>
    /// 返回与墙壁碰撞事件相关的所有接触的点。
    /// </summary>
    public List<Contact> WallContacts { get; private set; } = new List<Contact>(10);
    /// <summary>
    /// 返回与头部碰撞事件相关的所有接触点。
    /// </summary>
    public List<Contact> HeadContacts { get; private set; } = new List<Contact>(10);
    /// <summary>
    /// 返回与地面碰撞相关的接触点
    /// </summary>
    public List<Contact> GroundContacts { get; private set; } = new List<Contact>(10);

    /// <summary>
    /// 更新碰撞信息.从所有的碰撞信息中来分类
    /// </summary>
    void GetContactsInformation()
    {
        bool wasCollidingwithWall = characterCollisionInfo.wallCollision;
        bool wasCollidingwithHead = characterCollisionInfo.headCollision;

        GroundContacts.Clear();
        WallContacts.Clear();
        HeadContacts.Clear();

        for (int i = 0; i < Contacts.Count; i++)
        {
            var contact = Contacts[i];
            float verticalAngle = Vector3.Angle(Up, contact.normal);
            if (CustomUtilities.isCloseTo(verticalAngle, 90f, CharacterConstants.WallContactAngleTolerance))
                WallContacts.Add(contact);
            if (verticalAngle >= CharacterConstants.HeadContactMinAngle)
                HeadContacts.Add(contact);
            if (verticalAngle <= 89f)
                GroundContacts.Add(contact);
        }
        if (WallContacts.Count == 0)
        {
            characterCollisionInfo.ResetWallInfo();
        }
        else
        {
            Contact wallContact = WallContacts[0];
            characterCollisionInfo.SetWallInfo(in wallContact, this);
            if (!wasCollidingwithWall)
            {
                OnWallHit?.Invoke(wallContact);
            }
        }
        if (HeadContacts.Count == 0)
        {
            characterCollisionInfo.ResetHeadInfo();
        }
        else
        {
            Contact headContact = WallContacts[0];
            characterCollisionInfo.SetHeadInfo(headContact, this);
            if (!wasCollidingwithHead)
                OnHeadHit?.Invoke(headContact);
        }
    }

    protected override void PreSimulationUpdate(float dt)
    {
        UpdateStabilityFlags();
        PhysicsComponent.ClearContacts();
        InputVelocity = Velocity;
        if (alwaysNotGrounded && WasGrounded)
        {
            ForceNotGrounded();
        }
        if (!IsKinematic)
        {
            ProcessVelocity(dt);
        }
        SetColliderSize();

        PreSimulationVelocity = Velocity;
        preSimulationPosition = Position;
        groundTriggerCollider3D.enabled = useGroundTrigger;
    }

    protected override void PostSimulationUpdate(float dt)
    {
        HandleRotation();
        GetContactsInformation();

        PostSimulationVelocity = Velocity;
        ExternalVelocity = PostSimulationVelocity - PreSimulationVelocity;

        PreGroundProbingPosition = PostGroundProbingPosition = Position;

        if (!IsKinematic)
        {
            if (!IsStable)
            {
                Vector3 position = Position;
                UnstableProbeGround(ref position, dt);
                SetDynamicGroundData(position);
                Position = position;
            }
            //==============================================不确定是否可以改为else
            if (IsStable)
            {
                ProcessDynamicGroundMovement(dt);
                PreGroundProbingPosition = Position;

                ProbeGround(dt);
                PostGroundProbingPosition = Position;
            }
            GroundProbingDisplacement = PostGroundProbingPosition - PreGroundProbingPosition;

            PostSimulationVelocityUpdate();
            UpdateTimer(dt);
            UpdatePostSimulationFlags();
        }
    }
    /// <summary>
    /// 更新稳定情况下的Flags
    /// </summary>
    void UpdateStabilityFlags()
    {
        HasBecomeGrounded = IsGrounded && !WasGrounded;
        HasBecomeStable = IsStable && !WasStable;
        HasBecomeNotGrounded = !IsGrounded && WasStable;
        HasBecomeUnStable = !IsStable && WasStable;

        WasStable = IsStable;
        WasGrounded = IsGrounded;

    }
    /// <summary>
    /// 在模拟结束后触发相关的事件
    /// </summary>
    void UpdatePostSimulationFlags()
    {
        Vector3 prevLocalVelocity = LocalInputVelocity;
        if (HasBecomeGrounded)
            OnGroundedStateEnter?.Invoke(prevLocalVelocity);
        if (HasBecomeNotGrounded)
            OnGroundedStateExit?.Invoke();
        if (HasBecomeStable)
            OnStableStateEnter?.Invoke(prevLocalVelocity);
        if (HasBecomeUnStable)
            OnStableStateExit?.Invoke();
        if (forceNotGroundedFrames != 0)
            forceNotGroundedFrames--;
        forceNotGroundedFlag = false;
        inheritVelocityFlag = false;
    }
    /// <summary>
    /// 更新相关的计时器，如在地面上的时间等
    /// </summary>
    /// <param name="dt"></param>
    void UpdateTimer(float dt)
    {
        if (IsStable)
        {
            StableElapsedTime += dt;
            UnStableElapsedTime = 0f;
        }
        else
        {
            StableElapsedTime = 0f;
            UnStableElapsedTime += dt;
        }
        if (IsGrounded)
        {
            NotGroundedTime = 0f;
            GroundedTime += dt;
        }
        else
        {
            NotGroundedTime += dt;
            GroundedTime = 0f;
        }
    }

    #region Dynamic Ground
    /// <summary>
    /// 是否受到动态地面影响
    /// </summary>
    /// <returns></returns>
    bool IsAllowedToFollowRiggidbodyReference()
    {
        if (!supportDynamicGround || !IsStable || GroundObject == null)
        {
            return false;
        }
        if (!CustomUtilities.BelongsToLayerMask(GroundObject.layer, dynamicGroundLayerMask))
            return false;

        if (characterCollisionInfo.groundRigidbody3D == null)
            return false;
        return true;
    }
    /// <summary>
    /// 设置动态地面的相关数据
    /// </summary>
    /// <param name="position"></param>
    void SetDynamicGroundData(Vector3 position)
    {
        if (!IsAllowedToFollowRiggidbodyReference())
        {
            return;
        }
        preSimulationFroundRotation = GroundRotation;
        groundToCharacter = position - GroundPosition;
    }
    /// <summary>
    /// 让角色跟着动态地面运动,内部执行方法
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="dt"></param>
    void ApplyGroundMovement(ref Vector3 position, ref Quaternion rotation, float dt)
    {
        Quaternion deltaRotation = GroundRotation * Quaternion.Inverse(preSimulationFroundRotation);
        position = GroundPosition + (deltaRotation * groundToCharacter);
        if (rotateForwardDirection)
        {
            Vector3 forward = deltaRotation * Forward;
            forward = Vector3.ProjectOnPlane(forward, Up);
            forward.Normalize();

            rotation = Quaternion.LookRotation(forward, Up);
        }
    }
    /// <summary>
    /// 更新当前地面的速度
    /// </summary>
    void UpdateGroundVelocity()
    {
        PreviousGroundVelocity = GroundVelocity;
        GroundVelocity = GetGroundPointVelocity(GroundContactPoint);
    }
    /// <summary>
    /// 处理动态地面上的移动
    /// </summary>
    /// <param name="dt"></param>
    void ProcessDynamicGroundMovement(float dt)
    {
        if (!IsAllowedToFollowRiggidbodyReference())
        {
            return;
        }
        IgnoreGroundResponse();

        Vector3 targetPosition = Position;
        Quaternion targetRotation = Rotation;
        ApplyGroundMovement(ref targetPosition, ref targetRotation, dt);
        // 降落在动态地面上时，通过从角色速度中删除平台速度来模拟无限摩擦。
        if (!WasGrounded)
        {
            Vector3 planerVelocityOnPlatform = Vector3.Project(PlanarVelocity, GetGroundPointVelocity(GroundContactPoint));
            PlanarVelocity -= planerVelocityOnPlatform;
        }
        if (!IsGroundAOneWayPlatform && GroundDeltaVelocity.magnitude > maxGroundVelocityChange)
        {
            float upToDynamicGroundVelocityAngle = Vector3.Angle(Vector3.Normalize(GroundVelocity), Up);
            if (upToDynamicGroundVelocityAngle < 45f)
                ForceNotGrounded();

            Vector3 characterVelocity = PreviousGroundVelocity;

            Velocity = characterVelocity;
            Position += CustomUtilities.Multiply(characterVelocity, dt);
            Rotation = targetRotation;
        }
        else
        {
            Position = targetPosition;
            Vector3 position = Position;
            bool overlapDetected = RemovePenetration(ref position);
            Position = position;
            if (!overlapDetected)
            {
                Rotation = targetRotation;
            }
        }
    }

    void ProcessInheritedVelocity()
    {
        if (!forceNotGroundedFlag || !inheritVelocityFlag)
            return;
        Vector3 loaclGroundVelocity = transform.InverseTransformVectorUnscaled(GroundVelocity);
        Vector3 planarGroundVelocity = Vector3.ProjectOnPlane(GroundVelocity, Up);
        Vector3 verticalGroundVelocity = Vector3.Project(GroundVelocity, Up);
        Vector3 inheritedGroundVelocity = Vector3.zero;
        if (planarGroundVelocity.magnitude >= inheritedGroundPlannarlVelocityThreshold)
        {
            inheritedGroundVelocity += CustomUtilities.Multiply(planarGroundVelocity, inheritedGroundPlanarVelocityMultiplier);

        }
        if (verticalGroundVelocity.magnitude >= inheritedGroundVerticalVelocityThreshold)
        {
            //这可以防止角色无法跳跃的边缘情况（下降平台）
            if (LocalVelocity.y > -loaclGroundVelocity.y)
                inheritedGroundVelocity += CustomUtilities.Multiply(verticalGroundVelocity, inheritedGroundPlanarVelocityMultiplier);
        }
        Velocity += inheritedGroundVelocity;
        GroundVelocity = Vector3.zero;
        PreviousGroundVelocity = Vector3.zero;
    }
    #endregion

    /// <summary>
    /// ================暂时未知===========================================================================
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected bool RemovePenetration(ref Vector3 position)
    {
        int overlapIteration = 0;
        bool iterationOverlapDetected = false;

        bool overlapDetected = false;
        var rotation = Rotation;
        do
        {
            iterationOverlapDetected = ColliderComponent.ComputePenetration(ref position, ref rotation, _removePenetrationAction);
            overlapDetected |= iterationOverlapDetected;
            overlapIteration++;

        } while (overlapIteration < 2 && iterationOverlapDetected);
        return overlapDetected;
    }

    void RemovePenetrationAction(ref Vector3 bodyPosition, ref Quaternion bodyRotation, Transform otherColliderTransform, Vector3 penetrationDirection, float penetrationDistance)
    {
        if (CustomUtilities.BelongsToLayerMask(otherColliderTransform.gameObject.layer, oneWayPlatformsLayerMask))
        {
            PhysicsComponent.IgnoreCollision(otherColliderTransform, true);
            return;
        }
        Vector3 separation = penetrationDirection * penetrationDistance;

        if (IsStable)
            separation = Vector3.ProjectOnPlane(separation, GroundStableNormal).normalized * penetrationDistance;

        CustomUtilities.AddMagnitude(ref separation, CharacterConstants.SkinWidth);
        bodyPosition += separation;
    }
    /// <summary>
    /// 模拟完以后更新速度
    /// </summary>
    void PostSimulationVelocityUpdate()
    {
        if (IsStable)
        {
            switch (stablePostSimulationVelocity)
            {
                case CharacterVelocityMode.UseInputVelocity:
                    Velocity = InputVelocity;
                    break;
                case CharacterVelocityMode.UsePreSimulationVelocity:
                    Velocity = PreSimulationVelocity;
                    if (WasStable)
                        PlanarVelocity = CustomUtilities.Multiply(Vector3.Normalize(PlanarVelocity), Velocity.magnitude);
                    break;
                case CharacterVelocityMode.UsePostSimulationVelocity:
                    if (WasStable)
                        PlanarVelocity = CustomUtilities.Multiply(Vector3.Normalize(PlanarVelocity), Velocity.magnitude);
                    break;
            }
            UpdateGroundVelocity();
        }
        else
        {
            switch (unstablePostSimulationVelocity)
            {
                case CharacterVelocityMode.UseInputVelocity:
                    Velocity = InputVelocity;
                    break;
                case CharacterVelocityMode.UsePreSimulationVelocity:
                    Velocity = PreSimulationVelocity;
                    break;
                case CharacterVelocityMode.UsePostSimulationVelocity:
                    break;
            }
        }

        if (IsGrounded)
            LastGroundedVelocity = Velocity;
    }
    bool IgnoreGroundResponse()
    {
        for (int i = 0; i < Contacts.Count; i++)
        {
            Contact contact = Contacts[i];
            if (!contact.isRigidbody || !contact.isKinematicRigidbody)
                continue;
            if (contact.collider3D.attachedArticulationBody == GroundRigidbody3D)
            {
                Velocity = PreSimulationVelocity;
                return true;
            }
        }
        return false;
    }

    #region Events
    public event Action<Contact> OnHeadHit;

    public event Action<Contact> OnWallHit;

    public event Action<Vector3> OnGroundedStateEnter;

    public event Action OnGroundedStateExit;

    public event Action OnNewGroundEnter;

    public event Action<Vector3> OnStableStateEnter;

    public event Action OnStableStateExit;

    #endregion
    /// <summary>
    /// 获得地面的速度
    /// </summary>
    public Vector3 GroundVelocity { get; private set; }
    /// <summary>
    /// 获取地面（刚体）的先前速度。
    /// </summary>
    public Vector3 PreviousGroundVelocity { get; private set; }
    /// <summary>
    /// 地面的速度差（与上一帧的速度之差）
    /// </summary>
    public Vector3 GroundDeltaVelocity => GroundVelocity - PreviousGroundVelocity;
    /// <summary>
    /// 地面的加速度
    /// </summary>
    public Vector3 GroundAcceleration => (GroundVelocity - PreviousGroundVelocity) / Time.fixedDeltaTime;
    /// <summary>
    /// 地面是否在上升
    /// </summary>
    public bool IsGroundAscending => transform.InverseTransformVectorUnscaled(Vector3.Project(CustomUtilities.Multiply(GroundVelocity, Time.deltaTime), Up)).y > 0;
    /// <summary>
    /// 处理角色移动
    /// </summary>
    /// <param name="dt"></param>
    void ProcessVelocity(float dt)
    {
        Vector3 position = Position;
        if (IsStable)
            ProcessStableMovement(dt, ref position);
        else
            ProcessUnstableMovement(dt, ref position);

        Velocity = (position - Position) / dt;
    }
    /// <summary>
    /// 处理角色在稳定地面上的移动
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="position"></param>
    void ProcessStableMovement(float dt, ref Vector3 position)
    {
        ApplyWeight(GroundVelocity);
        VerticalVelocity = Vector3.zero;
        Vector3 displacement = CustomUtilities.ProjectOnTangent(
            CustomUtilities.Multiply(Velocity, dt),
            GroundStableNormal,
            Up);

        StableCollideAndSlide(ref position, displacement, false);
        SetDynamicGroundData(position);
        if (!IsStable)
        {
            groundRigidbodyComponent = null;
        }
    }
    /// <summary>
    /// 处理角色在非稳定地面上的移动
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="position"></param>
    void ProcessUnstableMovement(float dt, ref Vector3 position)
    {
        ProcessInheritedVelocity();
        Vector3 displacement = CustomUtilities.Multiply(Velocity, dt);
        UnstableCollideAndSlide(ref position, displacement, dt);
    }

    protected override void UpdateDynamicRootMotionPosition()
    {
        Vector3 rootMotionVelocity = Animator.deltaPosition / Time.deltaTime;
        switch (rootMotionVelocityType)
        {
            case RootMotionVelocityType.SetVelocity:
                Velocity = rootMotionVelocity;
                break;
            case RootMotionVelocityType.SetPlanarVelocity:
                PlanarVelocity = rootMotionVelocity;
                break;
            case RootMotionVelocityType.SetVerticalVelocity:
                VerticalVelocity = rootMotionVelocity;
                break;
        }

    }
    /// <summary>
    /// 根据给定的高度锚点（也称为大小参考）将旧大小与新大小进行比较后，获取新位置值。
    /// </summary>
    /// <param name="size"></param>
    /// <param name="heightAnchorRatio"></param>
    /// <returns></returns>
    Vector3 GetSizeoffsetPosition(Vector2 size, float heightAnchorRatio)
    {
        float verticalOffset = (BodySize.y - size.y) * heightAnchorRatio;
        Vector3 testPosition = Position + CustomUtilities.Multiply(Up, verticalOffset);
        return testPosition;
    }

    public void SetSize(Vector2 size, SizeReferenceType sizeReferenceType)
    {
        float heightAnchorRatio = 0f;
        switch (sizeReferenceType)
        {
            case SizeReferenceType.Top:
                heightAnchorRatio = 1f;
                break;
            case SizeReferenceType.Center:
                heightAnchorRatio = 0.5f;
                break;
            default:
                break;
        }

        Position = GetSizeoffsetPosition(size, heightAnchorRatio);
        BodySize = size;
        SetColliderSize();
    }

    public bool CheckSize(Vector3 position, Vector2 size)
    {
        HitInfoFilter filter = new HitInfoFilter(ObstaclesWithoutOWPLayerMask, true, true);
        return CharacterCollisions.CheckBodySize(size, position, in filter, _collisionHitFilter);
    }

    public bool CheckSize(Vector2 size)
    {
        HitInfoFilter filter = new HitInfoFilter(
        ObstaclesWithoutOWPLayerMask,
            true,
              true
        );
        return CharacterCollisions.CheckBodySize(size, Position, in filter, _collisionHitFilter);
    }

    public bool CheckAndInterpolateSize(Vector2 targetSize, float lerpFactor, SizeReferenceType sizeReferenceType)
    {
        bool validSize = CheckSize(targetSize);
        if (validSize)
        {
            Vector2 size = Vector2.Lerp(BodySize, targetSize, lerpFactor);
            SetSize(size, sizeReferenceType);
        }
        return validSize;
    }

    public bool CheckAndInterpolateHeight(float targetHeight, float lerpFactor, SizeReferenceType sizeReferenceType) => CheckAndInterpolateSize(new Vector2(DefaultBodySize.x, targetHeight), lerpFactor, SizeReferenceType.Bottom);
    /// <summary>
    /// 设置碰撞体大小
    /// </summary>
    void SetColliderSize()
    {
        float verticalOffset = IsStable ? Mathf.Max(StepOffset, CharacterConstants.ColliderMinBottomOffset) : 0f;
        float radius = BodySize.x / 2f;
        float height = BodySize.y - verticalOffset;

        ColliderComponent.Size = new Vector2(radius * 2f, height);
        ColliderComponent.Offset = CustomUtilities.Multiply(Vector2.up, verticalOffset + height / 2f);
    }

    public void SweepAndTeleport(Vector3 destination, in HitInfoFilter filter)
    {
        Vector3 displacement = destination - Position;
        CollisionInfo collisionInfo = CharacterCollisions.CastBody(Position, displacement, 0f, in filter, false, _collisionHitFilter);
        Position += collisionInfo.displacement;
    }
    /// <summary>
    /// 如果可能，强制角色接地（isGrounded = true）。检测距离包括降压距离。
    /// </summary>
    public void ForceGrounded()
    {
        if (!CanEnterGroundedState)
            return;
        HitInfoFilter filter = new HitInfoFilter(ObstaclesLayerMask, false, true, oneWayPlatformsLayerMask);
        CollisionInfo collisionInfo = CharacterCollisions.CheckForGround(Position, BodySize.y * 0.8f, stepDownDistance, filter, _collisionHitFilter);
        if (collisionInfo.hitInfo.hit)
        {
            float slopeAngle = Vector3.Angle(Up, GetGroundSlopeNormal(collisionInfo));
            if (slopeAngle <= slopeLimit)
            {
                Position += collisionInfo.displacement;
                SetGroundInfo(collisionInfo);
                SetDynamicGroundData(Position);
            }
        }
    }
    /// <summary>
    /// 获得地面坡度法线
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    public Vector3 GetGroundSlopeNormal(CollisionInfo collisionInfo)
    {
        float contactSlopeAngle = Vector3.Angle(Up, collisionInfo.hitInfo.normal);
        if (collisionInfo.isAnEdge)
        {
            if (contactSlopeAngle < slopeLimit && collisionInfo.edgeUpperSlopeAngle <= slopeLimit && collisionInfo.edgeLowerSlopeAngle <= slopeLimit)
                return Up;
            else if (collisionInfo.edgeUpperSlopeAngle <= slopeLimit)
                return collisionInfo.edgeUpperNormal;
            else if (collisionInfo.edgeLowerSlopeAngle <= slopeLimit)
                return collisionInfo.edgeLowerNormal;
            else
                return collisionInfo.hitInfo.normal;
        }
        else
        {
            return collisionInfo.hitInfo.normal;
        }
    }

    /// <summary>
    ///由地面探测算法计算的最后一个垂直位移（PostGroundProbingPosition - PreGroundProbingPosition）。
    /// </summary>
    public Vector3 GroundProbingDisplacement { get; private set; }

    /// <summary>
    /// 地面探测算法之前的最后一个刚体位置。
    /// </summary>
    public Vector3 PreGroundProbingPosition { get; private set; }

    /// <summary>
    /// 地面探测算法后的最后一个刚体位置。
    /// </summary>
    public Vector3 PostGroundProbingPosition { get; private set; }
    /// <summary>
    /// 判断是否是稳定地面
    /// </summary>
    /// <param name="groundTransform"></param>
    /// <returns></returns>
    bool EvaluateGroundStability(Transform groundTransform)
    {
        if (!CustomUtilities.BelongsToLayerMask(groundTransform.gameObject.layer, stableLayerMask))
        {
            return false;
        }
        if (!allowCharactersAsStableSurfaces && groundTransform.TryGetComponent(out CharacterActor characterActor))
            return false;
        return true;
    }
    /// <summary>
    /// 检测地面
    /// </summary>
    /// <param name="dt"></param>
    void ProbeGround(float dt)
    {
        Vector3 position = Position;
        HitInfoFilter sweepFilter = new HitInfoFilter(ObstaclesLayerMask, false, true);
        HitInfoFilter overlapFilter = new HitInfoFilter(ObstaclesWithoutOWPLayerMask, false, true);

        CollisionInfo collisionInfo = CharacterCollisions.CheckForGround(position, StepOffset, stepDownDistance, in sweepFilter, _collisionHitFilter);
        //如果检测到没有地板
        if (!collisionInfo.hitInfo.hit)
        {
            ForceNotGrounded();
            ProcessInheritedVelocity();
            return;
        }

        float slopeAngle = Vector3.Angle(Up, GetGroundSlopeNormal(collisionInfo));
        bool isGroundStable = slopeAngle <= slopeLimit && EvaluateGroundStability(collisionInfo.hitInfo.transform);

        position += collisionInfo.displacement;
        if (edgeCompensation && IsAStableEdge(collisionInfo))
        {
            Vector3 compensation = Vector3.Project((collisionInfo.hitInfo.point - position), Up);
            position += compensation;
        }

        //仅在检测到步长时执行重叠测试（垂直位移组件>某个阈值）
        float verticalDisplcementComponent = transform.InverseTransformDirection(position - Position).y;
        bool overlapCheck = false;

        if (verticalDisplcementComponent > CharacterConstants.SkinWidth)
        {
            overlapCheck = CharacterCollisions.CheckOverlap(position, StepOffset, in overlapFilter, _collisionHitFilter);
        }

        bool badStepDetected = false;
        badStepDetected |= !isGroundStable;
        badStepDetected |= overlapCheck;

        if (badStepDetected)
        {
            if (preventBadSteps)
            {
                if (WasGrounded)
                {
                    Vector3 dynamicGroundDispalement = CustomUtilities.Multiply(GroundVelocity, dt);
                    Vector3 initialPosition = preSimulationPosition + dynamicGroundDispalement;
                    position = initialPosition;
                    Vector3 unstableDisplacement = CustomUtilities.ProjectOnTangent(CustomUtilities.Multiply(InputVelocity, dt), GroundStableNormal, Up);

                    StableCollideAndSlide(ref position, unstableDisplacement, true);
                }
            }

            collisionInfo = CharacterCollisions.CheckForGroundRay(position, sweepFilter, _collisionHitFilter);
            SetGroundInfo(collisionInfo);
        }
        else
        {
            SetGroundInfo(collisionInfo);
        }
        if (IsStable)
            Position = position;
    }
    /// <summary>
    /// 强制字符放弃接地状态（isGrounded = false）。即放弃吸附地面
    /// 如果想让我们的角色在Y轴上位移则必须调用该方法
    /// </summary>
    /// <param name="ignoreGroundContactFrames">为了防止角色在强制不接地调用后立即重新进入接地状态而消耗的 FixedUpdate 帧数。</param>
    public void ForceNotGrounded(int ignoreGroundContactFrames = 3)
    {
        forceNotGroundedFrames = ignoreGroundContactFrames;
        inheritVelocityFlag = IsAllowedToFollowRiggidbodyReference();
        UpdateStabilityFlags();

        ResetGroundInfo();

        forceNotGroundedFlag = true;
    }
    /// <summary>
    /// 判断是否是个稳定地面的边缘
    /// </summary>
    /// <param name="collisionInfo"></param>
    /// <returns></returns>
    bool IsAStableEdge(CollisionInfo collisionInfo)
    {
        return collisionInfo.isAnEdge && collisionInfo.edgeUpperSlopeAngle <= slopeLimit;
    }
    bool CollisionHitFilter(Transform hitTransform)
    {
        var go = hitTransform.gameObject;
        if (!CheckOneWayPlatformLayerMask(go))
        {
            if (!CharacterCollisions.PhysicsComponent.CheckCollisionsWith(go))
                return false;
        }
        return true;
    }
    /// <summary>
    /// 稳定地面的物理模拟
    /// </summary>
    /// <param name="position"></param>
    /// <param name="displacement"></param>
    /// <param name="useFullBody"></param>
    protected void StableCollideAndSlide(ref Vector3 position, Vector3 displacement, bool useFullBody)
    {
        Vector3 groundPlaneNormal = GroundStableNormal;
        Vector3 slidingPlaneNormal = Vector3.zero;

        HitInfoFilter filter = new HitInfoFilter(ObstaclesLayerMask, false, true, oneWayPlatformsLayerMask);
        int iteration = 0;
        while (iteration < CharacterConstants.MaxSlideIterations)
        {
            iteration++;
            CollisionInfo collisionInfo = CharacterCollisions.CastBody(position, displacement, useFullBody ? 0f : StepOffset, in filter, false, _collisionHitFilter);

            if (collisionInfo.hitInfo.hit)
            {
                if (CheckOneWayPlatformLayerMask(collisionInfo))
                {
                    PhysicsComponent.IgnoreCollision(collisionInfo.hitInfo, true);
                    position += displacement;
                    break;
                }

                //Physics interaction ==========================

                if (CanPushDynamicRigidbodies)
                {
                    if (collisionInfo.hitInfo.IsRigidbody)
                    {
                        if (collisionInfo.hitInfo.IsDynamicRigidbody)
                        {
                            bool belongsToGroundRigidbody = false;

                            if (GroundCollider3D != null)
                                if (GroundCollider3D.attachedRigidbody != null)
                                    if (GroundCollider3D.attachedRigidbody == collisionInfo.hitInfo.rigidbody3D)
                                        belongsToGroundRigidbody = true;

                            if (!belongsToGroundRigidbody)
                            {
                                bool canPushThisObject = CustomUtilities.BelongsToLayerMask(collisionInfo.hitInfo.layer, pushableRigidbodyLayerMask);
                                if (canPushThisObject)
                                {
                                    //使用剩余位移。
                                    position += displacement;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (slideOnWalls)
                {
                    position += collisionInfo.displacement;
                    displacement -= collisionInfo.displacement;
                    UpdateCollideAndSlideData(collisionInfo, ref slidingPlaneNormal, ref groundPlaneNormal, ref displacement);
                }
                else
                {
                    if (!wallCollision)
                        position += collisionInfo.displacement;
                    break;
                }
            }
            else
            {
                position += displacement;
                break;
            }
        }
    }
    /// <summary>
    /// 判断当前的地面是否是单项平台
    /// </summary>
    public bool IsGroundAOneWayPlatform
    {
        get
        {
            if (GroundObject == null)
                return false;
            return CustomUtilities.BelongsToLayerMask(GroundObject.layer, oneWayPlatformsLayerMask);
        }
    }
    /// <summary>
    /// 检查gameobject是否是单项平台
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public bool CheckOneWayPlatformLayerMask(GameObject gameObject)
    {
        return CustomUtilities.BelongsToLayerMask(gameObject.layer, oneWayPlatformsLayerMask);
    }
    /// <summary>
    /// 检测是否为单项平台
    /// </summary>
    /// <param name="collisionInfo"></param>
    /// <returns></returns>
    public bool CheckOneWayPlatformLayerMask(CollisionInfo collisionInfo)
    {
        return CustomUtilities.BelongsToLayerMask(collisionInfo.hitInfo.layer, oneWayPlatformsLayerMask);
    }

    public bool CheckOneWayPlatformCollision(Vector3 contactPoint, Vector3 characterPosition)
    {
        Vector3 contactPointToBottom = GetBottomCenter(characterPosition) - contactPoint;
        float collisionAngle = Vector3.Angle(Up, contactPointToBottom);

        return collisionAngle <= 0.5f * oneWayPlayformsValidArc;
    }
    public bool CanEnterGroundedState => !alwaysNotGrounded && forceNotGroundedFrames == 0;

    /// <summary>
    /// 不稳定碰撞和滑动
    /// </summary>
    /// <param name="position"></param>
    /// <param name="displacement"></param>
    /// <param name="dt"></param>
    protected void UnstableCollideAndSlide(ref Vector3 position, Vector3 displacement, float dt)
    {
        HitInfoFilter filter = new HitInfoFilter(
            ObstaclesLayerMask,
            forceNotGroundedFrames != 0,
            true,
            oneWayPlatformsLayerMask
            );
        Vector3 slidePlaneANormal = Vector3.zero;
        Vector3 slidePlaneBNormal = Vector3.zero;

        int iteration = 0;
        while (iteration < CharacterConstants.MaxSlideIterations || displacement == Vector3.zero)
        {
            iteration++;
            CollisionInfo collisionInfo = CharacterCollisions.CastBody(position, displacement, 0f, in filter, false, _collisionHitFilter);
            if (collisionInfo.hitInfo.hit)
            {
                float slopeAngle = Vector3.Angle(Up, collisionInfo.hitInfo.normal);
                bool stableHit = slopeAngle <= slopeLimit;
                bool bottomCollision = slopeAngle < 90f;

                if (CheckOneWayPlatformLayerMask(collisionInfo))
                {
                    //如果是单项平台就忽略他 
                    PhysicsComponent.IgnoreCollision(collisionInfo.hitInfo, true);
                    //检查角色是否会用胶囊的底部撞击平台。
                    Vector3 nextPosition = position + collisionInfo.displacement;
                    bool isValidOWP = CheckOneWayPlatformCollision(collisionInfo.hitInfo.point, nextPosition);

                    if (isValidOWP)
                    {
                        position += collisionInfo.displacement;
                        SetGroundInfo(collisionInfo);
                        SetDynamicGroundData(position);
                        Position = position;

                        displacement -= collisionInfo.displacement;
                        displacement = Vector3.ProjectOnPlane(displacement, collisionInfo.hitInfo.normal);
                        position += displacement;
                    }
                    else
                    {
                        position += displacement;
                    }
                    break;

                }
                if (collisionInfo.hitInfo.IsRigidbody)
                {
                    if (collisionInfo.hitInfo.IsKinematicRigidbody)
                    {
                        position += displacement;
                        break;
                    }
                    if (CanPushDynamicRigidbodies && collisionInfo.hitInfo.IsDynamicRigidbody)
                    {
                        bool canPushThisObject = CustomUtilities.BelongsToLayerMask(collisionInfo.hitInfo.layer, pushableRigidbodyLayerMask);
                        if (canPushThisObject)
                        {
                            position += displacement;
                            break;

                        }
                    }
                }
                //Fall back to this
                position += collisionInfo.displacement;
                displacement -= collisionInfo.displacement;

                if (slidePlaneANormal == Vector3.zero)
                {
                    if (preventtUnstableClimbing && bottomCollision && !stableHit)
                    {
                        bool isUpwardsDisplacement = transform.InverseTransformVectorUnscaled(Vector3.Project(displacement, Up)).y > 0f;
                        if (isUpwardsDisplacement)
                        {
                            displacement = Vector3.Project(displacement, Up);
                        }
                        else
                        {
                            displacement = Vector3.ProjectOnPlane(displacement, collisionInfo.hitInfo.normal);
                        }
                    }
                    else
                    {
                        displacement = Vector3.ProjectOnPlane(displacement, collisionInfo.hitInfo.normal);
                    }
                    //
                    slidePlaneANormal = collisionInfo.hitInfo.normal;
                }
                else if (slidePlaneANormal == Vector3.zero)
                {
                    slidePlaneBNormal = collisionInfo.hitInfo.normal;
                    Vector3 displacementDirection = Vector3.Cross(slidePlaneANormal, slidePlaneBNormal);
                    displacementDirection.Normalize();
                    displacement = Vector3.Project(displacement, displacementDirection);
                }
            }
            else
            {
                position += displacement;
                break;
            }
        }
    }
    /// <summary>
    /// 检测不稳定地板
    /// </summary>
    /// <param name="position"></param>
    /// <param name="dt"></param>
    void UnstableProbeGround(ref Vector3 position, float dt)
    {
        if (!CanEnterGroundedState)
        {
            unstableGroundContactTime = 0f;
            PredictedGround = null;
            PredictedGroundDistance = 0f;
            ResetGroundInfo();
            return;
        }

        HitInfoFilter groundCheckFilter = new HitInfoFilter(ObstaclesLayerMask, false, true);
        CollisionInfo collisionInfo = CharacterCollisions.CheckForGround(position, StepOffset, CharacterConstants.GroundPredictionDistance, in groundCheckFilter, _collisionHitFilter);
        if (collisionInfo.hitInfo.hit)
        {
            PredictedGround = collisionInfo.hitInfo.transform.gameObject;
            PredictedGroundDistance = collisionInfo.displacement.magnitude;
            if (CheckOneWayPlatformLayerMask(collisionInfo))
                PhysicsComponent.IgnoreCollision(collisionInfo.hitInfo, true);
            bool validForGroundCheck = PredictedGroundDistance <= CharacterConstants.GroundCheckDistance;
            if (validForGroundCheck)
            {
                unstableGroundContactTime += dt;
                if (CanPerformUnstableGroundDetection(in collisionInfo.hitInfo))
                {
                    position += collisionInfo.displacement;
                    SetGroundInfo(collisionInfo);
                }
            }
            else
            {
                unstableGroundContactTime = 0f;
                ResetGroundInfo();
            }
        }
        else
        {
            unstableGroundContactTime = 0f;
            PredictedGround = null;
            PredictedGroundDistance = 0f;

            ResetGroundInfo();
        }
        GroundProbingDisplacement = Vector3.zero;
    }
    protected bool CanPerformUnstableGroundDetection(in HitInfo hitInfo)
    {
        if (detectGroundWhileAscending)
            return true;
        else
            return LocalVelocity.y <= 0f || unstableGroundContactTime >= CharacterConstants.MaxUnstableGroundContactTime || hitInfo.IsRigidbody;
    }

    public GameObject PredictedGround { get; private set; }

    public float PredictedGroundDistance { get; private set; }

    void SetGroundInfo(CollisionInfo collisionInfo)
    {
        ProcessNewGround(collisionInfo.hitInfo.transform);
        characterCollisionInfo.SetGroundInfo(collisionInfo, this);
        SetStableState(collisionInfo);
    }
    void SetStableState(CollisionInfo collisionInfo)
    {
        IsGrounded = collisionInfo.hitInfo.hit;
        IsStable = false;
        if (!IsGrounded || !EvaluateGroundStability(characterCollisionInfo.groundObject.transform))
            return;

        if (WasStable)
        {
            IsStable = characterCollisionInfo.groundSlopeAngle <= slopeLimit;
        }
        else
        {
            if (useStableEdgeWhenLanding)
            {
                IsStable = characterCollisionInfo.groundSlopeAngle <= slopeLimit;
            }
            else
            {
                float contactSlopeAnlge = Vector3.Angle(Up, characterCollisionInfo.groundContactNormal);
                IsStable = contactSlopeAnlge <= slopeLimit;
            }
        }
    }
    void ResetGroundInfo()
    {
        characterCollisionInfo.ResetGroundInfo();
        IsGrounded = false;
        IsStable = false;
    }
    void ProcessNewGround(Transform newGroundTransform)
    {
        bool isThisANewGround = newGroundTransform != GroundTransform;
        if (isThisANewGround)
        {
            groundRigidbodyComponent = groundRigidbodyComponents.GetOrRegisterValue(newGroundTransform);
            OnNewGroundEnter?.Invoke();
        }
    }
    bool UpdateCollideAndSlideData(CollisionInfo collisionInfo, ref Vector3 slidingPlaneNormal, ref Vector3 groundPlanNormal, ref Vector3 displacement)
    {
        Vector3 normal = collisionInfo.hitInfo.normal;
        if (collisionInfo.contactSlopeAngle > slopeLimit || !EvaluateGroundStability(collisionInfo.hitInfo.transform))
        {
            if (slidingPlaneNormal != Vector3.zero)
            {
                bool acuteAngleBetweenWalls = Vector3.Dot(normal, slidingPlaneNormal) > 0f;
                if (acuteAngleBetweenWalls)
                {
                    displacement = CustomUtilities.DeflectVector(displacement, groundPlanNormal, normal);
                }
                else
                {
                    displacement = Vector3.zero;
                }
            }
            else
            {
                displacement = CustomUtilities.DeflectVector(displacement, groundPlanNormal, normal);
            }
            slidingPlaneNormal = normal;
        }
        else
        {
            displacement = CustomUtilities.ProjectOnTangent(displacement, normal, Up);
            groundPlanNormal = normal;
            slidingPlaneNormal = Vector3.zero;
        }
        return displacement == Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (CharacterBody == null)
            CharacterBody = GetComponent<CharacterBody>();

        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);

        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 origin = CustomUtilities.Multiply(Vector3.up, stepUpDistance);
        Gizmos.DrawWireCube(
            origin,
            new Vector3(1.1f * CharacterBody.BodySize.x, 0.02f, 1.1f * CharacterBody.BodySize.x)
        );

        Gizmos.matrix = Matrix4x4.identity;
    }
}

public enum CharacterActorState
{
    /// <summary>
    /// 未在地面
    /// </summary>
    NotGrounded,
    /// <summary>
    /// 静态地面
    /// </summary>
    StableGrounded,
    /// <summary>
    /// 动态地面
    /// </summary>
    UnstableGrounded
}
public enum CharacterVelocityMode
{
    /// <summary>
    /// 使用输入速度
    /// </summary>
    UseInputVelocity,
    /// <summary>
    /// 使用模拟前速度
    /// </summary>
    UsePreSimulationVelocity,
    /// <summary>
    /// 使用模拟后速度
    /// </summary>
    UsePostSimulationVelocity
}
public enum SizeReferenceType
{
    /// <summary>
    /// 顶部
    /// </summary>
    Top,
    /// <summary>
    /// 中心
    /// </summary>
    Center,
    /// <summary>
    /// 底部
    /// </summary>
    Bottom
}