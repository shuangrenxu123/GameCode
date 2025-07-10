using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
[RequireComponent(typeof(CharacterBody))]
[DefaultExecutionOrder(10)]
public class CharacterActor : PhysicsActor
{
    #region ï¿½ï¿½Ø±ï¿½ï¿½ï¿?

    public LayerMask oneWayPlatformsLayerMask = 0;

    [Range(0, 179f)]
    public float oneWayPlayformsValidArc = 175;
    public float slopeLimit = 55f;
    public LayerMask stableLayerMask = -1;
    public bool allowCharactersAsStableSurfaces = true;

    public bool preventtUnstableClimbing = true;
    public bool preventBadSteps = true;

    public float stepUpDistance = 0.5f;

    public float stepDownDistance = 0.5f;

    public bool alwaysNotGrounded = false;

    [Condition("alwaysNotGrounded", ConditionAttribute.ConditionType.IsFalse)]
    public bool forceGroundedAtStart = true;

    public bool useGroundTrigger = true;

    public bool edgeCompensation = false;

    public bool useStableEdgeWhenLanding = true;

    public bool detectGroundWhileAscending = false;


    public bool supportDynamicGround = true;
    public LayerMask dynamicGroundLayerMask = -1;

    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public bool rotateForwardDirection = true;

    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float maxGroundVelocityChange = 30f;

    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float inheritedGroundPlannarlVelocityThreshold = 2f;

    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float inheritedGroundPlanarVelocityMultiplier = 1f;
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]

    public float inheritedGroundVerticalVelocityThreshold = 2f;
    [Condition("supportDynamicGround", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public float inheritedGroundVerticalVelocityMultiplier = 1f;
    public bool slideOnWalls = true;

    [SerializeField]
    bool resetVelocityOnTeleport = true;

    public CharacterVelocityMode stablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;
    public CharacterVelocityMode unstablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;
    public bool constraintRotation = true;

    [Condition("constraintRotation", ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.VisibilityType.NotEditable)]
    public Transform upDirectionReference = null;


    [Condition(
    new string[] { "constraintRotation", "upDirectionReference" },
    new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsNull },
    new float[] { 0f, 0f },
    ConditionAttribute.VisibilityType.Hidden)]
    public Vector3 constraintUpDirection = Vector3.up;


    [Condition(
     new string[] { "constraintRotation", "upDirectionReference" },
     new ConditionAttribute.ConditionType[] { ConditionAttribute.ConditionType.IsTrue, ConditionAttribute.ConditionType.IsNotNull },
     new float[] { 0f, 0f },
     ConditionAttribute.VisibilityType.Hidden)]
    public VerticalAlignmentSettings.VerticalReferenceMode upDirectionReferenceMode = VerticalAlignmentSettings.VerticalReferenceMode.Away;

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

    public CharacterBody CharacterBody { get; private set; }
    public override RigidbodyComponent RigidbodyComponent => CharacterBody.RigidbodyComponent;

    public ColliderComponent ColliderComponent => CharacterBody.ColliderComponent;

    public PhysicsComponent PhysicsComponent => CharacterCollisions.PhysicsComponent;

    protected CharacterCollisionInfo characterCollisionInfo = new CharacterCollisionInfo();
    public CharacterActorState CurrentState
    {
        get
        {
            if (IsGrounded)
                return IsStable ? CharacterActorState.StableGrounded
                : CharacterActorState.UnstableGrounded;
            else
                return CharacterActorState.NotGrounded;
        }
    }
    public CharacterActorState PreviousState
    {
        get
        {
            if (WasGrounded)
                return WasStable ? CharacterActorState.StableGrounded
                : CharacterActorState.UnstableGrounded;
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
    public void SetUpRootMotion(bool updateRootPosition = true, RootMotionVelocityType rootMotionVelocityType = RootMotionVelocityType.SetVelocity,
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
    ///ï¿½ï¿½È¡ï¿½ï¿½ï¿½ï¿½ï¿½Ð¹Ø½ï¿½É«ï¿½ï¿½×²ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ï¢ï¿½Ä½á¹¹ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ö·ï¿½ï¿½ï¿½ï¿½Ô£ï¿½ï¿½ï¿½ï¿½ï¿? IsGroundedï¿½ï¿½IsStableï¿½ï¿½GroundObject ï¿½È£ï¿½
    ///ï¿½ï¿½ï¿½Ô´ï¿½ï¿½ï¿½ï¿½Ö½á¹¹ï¿½Ð»ï¿½Ã¡ï¿?
    /// </summary>
    public CharacterCollisionInfo CharacterCollisionInfo => characterCollisionInfo;

    /// <summary>
    /// ï¿½ï¿½É«ï¿½Ç·ï¿½ï¿½Ú±ï¿½Ôµï¿½ï¿½
    /// </summary>
    public bool IsOnEdge => characterCollisionInfo.isOnEdge;
    /// <summary>
    /// ï¿½ï¿½Ôµï¿½Ç¶ï¿½
    /// </summary>
    public float EdgeAngle => characterCollisionInfo.edgeAngle;
    /// <summary>
    /// ï¿½Ç·ï¿½ï¿½Úµï¿½ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    public bool IsGrounded { get; private set; }
    /// <summary>
    /// ï¿½ï¿½È¡ï¿½ï¿½É«ï¿½ï¿½UPï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½È¶ï¿½ï¿½ï¿½ï¿½ï¿½Ö®ï¿½ï¿½Ä½Ç¶È¡ï¿?
    /// </summary>
    public float GroundSlopeAngle => characterCollisionInfo.groundSlopeAngle;
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ÃµÄ½Ó´ï¿½ï¿½ã¡£
    /// </summary>
    public Vector3 GroundContactPoint => characterCollisionInfo.groundContactPoint;
    public Vector3 GroundContactNormal => characterCollisionInfo.groundContactNormal;

    public Vector3 GroundStableNormal => IsStable ? characterCollisionInfo.groundStableNormal : Up;

    public GameObject GroundObject => characterCollisionInfo.groundObject;
    public Transform GroundTransform => GroundObject != null ? GroundObject.transform : null;
    public Collider GroundCollider3D => characterCollisionInfo.groundCollider3D;

    public Rigidbody GroundRigidbody3D => characterCollisionInfo.groundRigidbody3D;
    #endregion
    #region Wall
    public bool WallCollision => characterCollisionInfo.wallCollision;
    public float WallAngle => characterCollisionInfo.wallAngle;
    public Contact WallContact => characterCollisionInfo.wallContact;
    #endregion
    #region head
    /// <summary>
    /// ï¿½ï¿½È¡ï¿½ï¿½É«ï¿½Äµï¿½Ç°ï¿½È¶ï¿½ï¿½ï¿½×´Ì¬ï¿½ï¿½ï¿½È¶ï¿½ï¿½Ôµï¿½ï¿½Ú¡ï¿½ï¿½Óµï¿½+ï¿½Â¶È½ï¿½<=ï¿½Â¶È¼ï¿½ï¿½Þ¡ï¿½ï¿½ï¿½
    /// </summary>
    public bool IsStable { get; private set; }
    public bool HeadCollision => characterCollisionInfo.headCollision;
    public float HaedAngle => characterCollisionInfo.headAngle;
    public Contact HeadContact => characterCollisionInfo.headContact;
    public bool IsOnUnstableGround => IsGrounded && characterCollisionInfo.groundSlopeAngle > slopeLimit;
    /// <summary>
    /// ï¿½ï¿½È¡ï¿½ï¿½Ç°ï¿½Äµï¿½ï¿½ï¿½×´Ì¬
    /// </summary>
    public bool WasGrounded { get; private set; }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ò»Ö¡ï¿½ï¿½ï¿½È¶ï¿½×´Ì¬
    /// </summary>
    public bool WasStable { get; private set; }
    /// <summary>
    /// Ò»ï¿½ï¿½ï¿½ï¿½ï¿½Ô£ï¿½Ö¸Ê¾ï¿½ï¿½É«ï¿½ï¿½ï¿½ï¿½Ò»ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ú¼ï¿½ï¿½Ç·ï¿½ï¿½Ñ½ÓµØ¡ï¿½
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
    /// ï¿½ï¿½È¡/ï¿½ï¿½ï¿½ï¿½Í¶Ó°ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ï·ï¿½ï¿½ï¿½ï¿½Î³Éµï¿½Æ½ï¿½ï¿½ï¿½ÏµÄ¸ï¿½ï¿½ï¿½ï¿½Ù¶È¡ï¿½
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

    #region Unityï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
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
        //ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½×²
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
    /// ï¿½ï¿½Ê±ï¿½ï¿½ï¿½ï¿½×¢ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Â¼ï¿?
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        OnTeleport += OnTeleportMethod;

    }
    /// <summary>
    /// ï¿½ï¿½Ê±ï¿½ï¿½ï¿½ï¿½È¡ï¿½ï¿½ï¿½ï¿½×¢ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Â¼ï¿½
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        OnTeleport -= OnTeleportMethod;
    }
    #endregion
    /// <summary>
    /// ï¿½ï¿½ï¿½Íºï¿½ï¿½ï¿½Â¼ï¿½ï¿½ï¿½ï¿½ï¿½Ê±Ö»ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ù¶ï¿½
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
    /// ï¿½Úµï¿½ï¿½ï¿½Ó´ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ê©ï¿½ï¿½ï¿½ï¿½ï¿½ï¿?
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
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ó´ï¿½ï¿½ï¿½ï¿½ï¿½Ï¢
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
    /// ï¿½ï¿½È¡ï¿½ï¿½ï¿½Âµï¿½Triggerï¿½ï¿½
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
    /// ï¿½ï¿½È¡ï¿½ï¿½ï¿½ï¿½FixUpdateï¿½ï¿½ï¿½ï¿½Ö®Ç°ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ù¶ï¿½Öµï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½È·ï¿½ï¿½
    /// </summary>
    public Vector3 InputVelocity { get; private set; }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Õ¼ï¿½ï¿½Âµï¿½ï¿½Ù¶ï¿½Ó¦ï¿½Ãµï¿½ï¿½ï¿½ï¿½Ø¿Õ¼ï¿½ï¿½ï¿?
    /// </summary>
    public Vector3 LocalInputVelocity => transform.InverseTransformDirection(InputVelocity);
    /// <summary>
    /// Ä£ï¿½ï¿½Ç°ï¿½ï¿½ï¿½Ù¶ï¿½
    /// </summary>
    public Vector3 PreSimulationVelocity { get; private set; }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½Ä£ï¿½ï¿½ï¿½ï¿½ï¿½Ù¶ï¿½
    /// </summary>
    public Vector3 PostSimulationVelocity { get; private set; }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½á¹©ï¿½ï¿½ï¿½Ù¶ï¿?
    /// </summary>
    public Vector3 ExternalVelocity { get; private set; }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½×ª
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
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ç½ï¿½ï¿½ï¿½ï¿½×²ï¿½Â¼ï¿½ï¿½ï¿½Øµï¿½ï¿½ï¿½ï¿½Ð½Ó´ï¿½ï¿½Äµã¡?
    /// </summary>
    public List<Contact> WallContacts { get; private set; } = new List<Contact>(10);
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Í·ï¿½ï¿½ï¿½ï¿½×²ï¿½Â¼ï¿½ï¿½ï¿½Øµï¿½ï¿½ï¿½ï¿½Ð½Ó´ï¿½ï¿½ã¡?
    /// </summary>
    public List<Contact> HeadContacts { get; private set; } = new List<Contact>(10);
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½×²ï¿½ï¿½ØµÄ½Ó´ï¿½ï¿½ï¿½
    /// </summary>
    public List<Contact> GroundContacts { get; private set; } = new List<Contact>(10);

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½×²ï¿½ï¿½Ï¢.ï¿½ï¿½ï¿½ï¿½ï¿½Ðµï¿½ï¿½ï¿½×²ï¿½ï¿½Ï¢ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
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
            Contact headContact = HeadContacts[0];
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
    /// ï¿½ï¿½ï¿½ï¿½ï¿½È¶ï¿½ï¿½ï¿½ï¿½ï¿½Âµï¿½Flags
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
    /// ï¿½ï¿½Ä£ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ó´¥·ï¿½ï¿½ï¿½Øµï¿½ï¿½Â¼ï¿½
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
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ØµÄ¼ï¿½Ê±ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Úµï¿½ï¿½ï¿½ï¿½Ïµï¿½Ê±ï¿½ï¿½ï¿½
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
    /// ï¿½Ç·ï¿½ï¿½Üµï¿½ï¿½ï¿½Ì¬ï¿½ï¿½ï¿½ï¿½Ó°ï¿½ï¿½
    /// </summary>
    /// <returns></returns>
    bool IsAllowedToFollowRigidbodyReference()
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
    /// ï¿½ï¿½ï¿½Ã¶ï¿½Ì¬ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    /// <param name="position"></param>
    void SetDynamicGroundData(Vector3 position)
    {
        if (!IsAllowedToFollowRigidbodyReference())
        {
            return;
        }
        preSimulationFroundRotation = GroundRotation;
        groundToCharacter = position - GroundPosition;
    }
    /// <summary>
    /// ï¿½Ã½ï¿½É«ï¿½ï¿½ï¿½Å¶ï¿½Ì¬ï¿½ï¿½ï¿½ï¿½ï¿½Ë¶ï¿½,ï¿½Ú²ï¿½Ö´ï¿½Ð·ï¿½ï¿½ï¿½
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
    /// ï¿½ï¿½ï¿½Âµï¿½Ç°ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ù¶ï¿?
    /// </summary>
    void UpdateGroundVelocity()
    {
        PreviousGroundVelocity = GroundVelocity;
        GroundVelocity = GetGroundPointVelocity(GroundContactPoint);
    }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ì¬ï¿½ï¿½ï¿½ï¿½ï¿½Ïµï¿½ï¿½Æ¶ï¿½
    /// </summary>
    /// <param name="dt"></param>
    void ProcessDynamicGroundMovement(float dt)
    {
        if (!IsAllowedToFollowRigidbodyReference())
        {
            return;
        }
        IgnoreGroundResponse();

        Vector3 targetPosition = Position;
        Quaternion targetRotation = Rotation;
        ApplyGroundMovement(ref targetPosition, ref targetRotation, dt);
        // ï¿½ï¿½ï¿½ï¿½ï¿½Ú¶ï¿½Ì¬ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ê±ï¿½ï¿½Í¨ï¿½ï¿½ï¿½Ó½ï¿½É«ï¿½Ù¶ï¿½ï¿½ï¿½É¾ï¿½ï¿½Æ½Ì¨ï¿½Ù¶ï¿½ï¿½ï¿½Ä£ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ä¦ï¿½ï¿½ï¿½ï¿½
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
            //ï¿½ï¿½ï¿½ï¿½Ô·ï¿½Ö¹ï¿½ï¿½É?ï¿½Þ·ï¿½ï¿½ï¿½Ô¾ï¿½Ä±ï¿½Ôµï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Â½ï¿½Æ½Ì?ï¿½ï¿½
            if (LocalVelocity.y > -loaclGroundVelocity.y)
                inheritedGroundVelocity += CustomUtilities.Multiply(verticalGroundVelocity, inheritedGroundPlanarVelocityMultiplier);
        }
        Velocity += inheritedGroundVelocity;
        GroundVelocity = Vector3.zero;
        PreviousGroundVelocity = Vector3.zero;
    }
    #endregion

    /// <summary>
    /// ================ï¿½ï¿½Ê±Î´Öª===========================================================================
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
    /// Ä£ï¿½ï¿½ï¿½ï¿½ï¿½Ôºï¿½ï¿½ï¿½ï¿½ï¿½Ù¶ï¿?
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
    /// ï¿½ï¿½Ãµï¿½ï¿½ï¿½ï¿½ï¿½Ù¶ï¿½
    /// </summary>
    public Vector3 GroundVelocity { get; private set; }
    /// <summary>
    /// ï¿½ï¿½È¡ï¿½ï¿½ï¿½æ£¨ï¿½ï¿½ï¿½å£©ï¿½ï¿½ï¿½ï¿½Ç°ï¿½Ù¶È¡ï¿½
    /// </summary>
    public Vector3 PreviousGroundVelocity { get; private set; }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ù¶È²î£?ï¿½ï¿½ï¿½ï¿½Ò»Ö¡ï¿½ï¿½ï¿½Ù¶ï¿½Ö®ï¿½î£©
    /// </summary>
    public Vector3 GroundDeltaVelocity => GroundVelocity - PreviousGroundVelocity;
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½Ä¼ï¿½ï¿½Ù¶ï¿?
    /// </summary>
    public Vector3 GroundAcceleration => (GroundVelocity - PreviousGroundVelocity) / Time.fixedDeltaTime;
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½Ç·ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    public bool IsGroundAscending => transform.InverseTransformVectorUnscaled(Vector3.Project(CustomUtilities.Multiply(GroundVelocity, Time.deltaTime), Up)).y > 0;

    void ProcessVelocity(float dt)
    {
        Vector3 position = Position;
        if (IsStable)
            ProcessStableMovement(dt, ref position);
        else
            ProcessUnstableMovement(dt, ref position);

        Velocity = (position - Position) / dt;
    }

    void ProcessStableMovement(float dt, ref Vector3 position)
    {
        ApplyWeight(GroundContactPoint);
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
    /// ï¿½ï¿½ï¿½Ý¸ï¿½ï¿½ï¿½ï¿½Ä¸ß¶ï¿½Ãªï¿½ã£¨Ò²ï¿½ï¿½Îªï¿½ï¿½Ð¡ï¿½Î¿ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½É´ï¿½Ð¡ï¿½ï¿½ï¿½Â´ï¿½Ð¡ï¿½ï¿½ï¿½Ð±È½Ïºó£¬»ï¿½È¡ï¿½ï¿½Î»ï¿½ï¿½Öµï¿½ï¿½
    /// </summary>
    /// <param name="size"></param>
    /// <param name="heightAnchorRatio"></param>
    /// <returns></returns>
    Vector3 GetSizeOffsetPosition(Vector2 size, float heightAnchorRatio)
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

        Position = GetSizeOffsetPosition(size, heightAnchorRatio);
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

    public bool CheckAndInterpolateHeight(float targetHeight, float lerpFactor, SizeReferenceType sizeReferenceType)
        => CheckAndInterpolateSize(new Vector2(DefaultBodySize.x, targetHeight), lerpFactor, SizeReferenceType.Bottom);
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½×²ï¿½ï¿½ï¿½Ð?
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
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ü£ï¿½Ç¿ï¿½Æ½ï¿½É?ï¿½ÓµØ£ï¿½isGrounded = trueï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ñ¹ï¿½ï¿½ï¿½ë¡?
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
    /// ï¿½ï¿½Ãµï¿½ï¿½ï¿½ï¿½Â¶È·ï¿½ï¿½ï¿?
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
    ///ï¿½Éµï¿½ï¿½ï¿½Ì½ï¿½ï¿½ï¿½ã·¨ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ò»ï¿½ï¿½ï¿½ï¿½Ö±Î»ï¿½Æ£ï¿½PostGroundProbingPosition - PreGroundProbingPositionï¿½ï¿½ï¿½ï¿½
    /// </summary>
    public Vector3 GroundProbingDisplacement { get; private set; }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½Ì½ï¿½ï¿½ï¿½ã·¨Ö®Ç°ï¿½ï¿½ï¿½ï¿½ï¿½Ò»ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Î»ï¿½Ã¡ï¿?
    /// </summary>
    public Vector3 PreGroundProbingPosition { get; private set; }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½Ì½ï¿½ï¿½ï¿½ã·¨ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ò»ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Î»ï¿½Ã¡ï¿½
    /// </summary>
    public Vector3 PostGroundProbingPosition { get; private set; }
    /// <summary>
    /// ï¿½Ð¶ï¿½ï¿½Ç·ï¿½ï¿½ï¿½ï¿½È¶ï¿½ï¿½ï¿½ï¿½ï¿½
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
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    /// <param name="dt"></param>
    void ProbeGround(float dt)
    {
        Vector3 position = Position;
        HitInfoFilter sweepFilter = new HitInfoFilter(ObstaclesLayerMask, false, true);
        HitInfoFilter overlapFilter = new HitInfoFilter(ObstaclesWithoutOWPLayerMask, false, true);

        CollisionInfo collisionInfo = CharacterCollisions.CheckForGround(position, StepOffset, stepDownDistance, in sweepFilter, _collisionHitFilter);
        //ï¿½ï¿½ï¿½ï¿½ï¿½âµ½Ã»ï¿½ÐµØ°ï¿½
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

        //ï¿½ï¿½ï¿½Ú¼ï¿½âµ½ï¿½ï¿½ï¿½ï¿½Ê±Ö´ï¿½ï¿½ï¿½Øµï¿½ï¿½ï¿½ï¿½Ô£ï¿½ï¿½ï¿½Ö±Î»ï¿½ï¿½ï¿½ï¿½ï¿½>Ä³ï¿½ï¿½ï¿½ï¿½Öµï¿½ï¿½
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
    /// Ç¿ï¿½ï¿½ï¿½Ö·ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Óµï¿½×´Ì¬ï¿½ï¿½isGrounded = falseï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ÇµÄ½ï¿½É?ï¿½ï¿½Yï¿½ï¿½ï¿½ï¿½Î»ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ã¸Ã·ï¿½ï¿½ï¿½
    /// </summary>
    /// <param name="ignoreGroundContactFrames">Îªï¿½Ë·ï¿½Ö¹ï¿½ï¿½É«ï¿½ï¿½Ç¿ï¿½Æ²ï¿½ï¿½ÓµØµï¿½ï¿½Ãºï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Â½ï¿½ï¿½ï¿½Óµï¿½×´Ì?ï¿½ï¿½ï¿½ï¿½ï¿½Äµï¿½ FixedUpdate Ö¡ï¿½ï¿½ï¿½ï¿½</param>
    public void ForceNotGrounded(int ignoreGroundContactFrames = 3)
    {
        forceNotGroundedFrames = ignoreGroundContactFrames;
        inheritVelocityFlag = IsAllowedToFollowRigidbodyReference();
        UpdateStabilityFlags();

        ResetGroundInfo();

        forceNotGroundedFlag = true;
    }
    /// <summary>
    /// ï¿½Ð¶ï¿½ï¿½Ç·ï¿½ï¿½Ç¸ï¿½ï¿½È¶ï¿½ï¿½ï¿½ï¿½ï¿½Ä±ï¿½Ô?
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
    /// ï¿½È¶ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ä£ï¿½ï¿?
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
            //ï¿½ï¿½ï¿½å·¢ï¿½ï¿½ï¿½ï¿½ï¿½ß¼ï¿½ï¿½ï¿½Ï°ï¿½ï¿½ï¿?
            CollisionInfo collisionInfo = CharacterCollisions.CastBody(position, displacement, useFullBody ? 0f : StepOffset, in filter, false, _collisionHitFilter);
            //ï¿½ï¿½ï¿½Ä¿ï¿½ï¿½Î»ï¿½ï¿½Ã»ï¿½ï¿½ï¿½Ï°ï¿½ï¿½ï¿½Ëµï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ö±ï¿½Ó¹ï¿½È¥ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ò?ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ß¶ï¿½ï¿½ï¿?
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
                                    //Ê¹ï¿½ï¿½Ê£ï¿½ï¿½Î»ï¿½Æ¡ï¿½
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
                    if (!WallCollision)
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
    /// ï¿½Ð¶Ïµï¿½Ç°ï¿½Äµï¿½ï¿½ï¿½ï¿½Ç·ï¿½ï¿½Çµï¿½ï¿½ï¿½Æ½Ì¨
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
    /// ï¿½ï¿½ï¿½gameobjectï¿½Ç·ï¿½ï¿½Çµï¿½ï¿½ï¿½Æ½Ì¨
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public bool CheckOneWayPlatformLayerMask(GameObject gameObject)
    {
        return CustomUtilities.BelongsToLayerMask(gameObject.layer, oneWayPlatformsLayerMask);
    }
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½Ç·ï¿½Î?ï¿½ï¿½ï¿½ï¿½Æ½Ì¨
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
    /// ï¿½ï¿½ï¿½È¶ï¿½ï¿½ï¿½×²ï¿½Í»ï¿½ï¿½ï¿½
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
                    //ï¿½ï¿½ï¿½ï¿½Çµï¿½ï¿½ï¿½Æ½Ì?ï¿½Íºï¿½ï¿½ï¿½ï¿½ï¿½ 
                    PhysicsComponent.IgnoreCollision(collisionInfo.hitInfo, true);
                    //ï¿½ï¿½ï¿½ï¿½É«ï¿½Ç·ï¿½ï¿½ï¿½Ã½ï¿½ï¿½ÒµÄµ×²ï¿½×²ï¿½ï¿½Æ½Ì?ï¿½ï¿½
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
    /// ï¿½ï¿½â²»ï¿½È¶ï¿½ï¿½Ø°ï¿?
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
    /// Î´ï¿½Úµï¿½ï¿½ï¿½
    /// </summary>
    NotGrounded,
    /// <summary>
    /// ï¿½ï¿½Ì¬ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    StableGrounded,
    /// <summary>
    /// ï¿½ï¿½Ì¬ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    UnstableGrounded
}
public enum CharacterVelocityMode
{
    /// <summary>
    /// Ê¹ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ù¶ï¿½
    /// </summary>
    UseInputVelocity,
    /// <summary>
    /// Ê¹ï¿½ï¿½Ä£ï¿½ï¿½Ç°ï¿½Ù¶ï¿½
    /// </summary>
    UsePreSimulationVelocity,
    /// <summary>
    /// Ê¹ï¿½ï¿½Ä£ï¿½ï¿½ï¿½ï¿½Ù¶ï¿?
    /// </summary>
    UsePostSimulationVelocity
}
public enum SizeReferenceType
{
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    Top,
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    Center,
    /// <summary>
    /// ï¿½×²ï¿½
    /// </summary>
    Bottom
}