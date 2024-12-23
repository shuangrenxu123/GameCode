using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(110)]
public class Camera3D : MonoBehaviour
{


    [SerializeField]
    CharacterBrain characterBrain;
    [SerializeField]
    InputHandlerSettings inputHandlerSettings => characterBrain.CameraInputHandlerSettings;

    [SerializeField]
    string axes = "Camera";

    [SerializeField]
    string zoomAxis = "Camera Zoom"; //zoom �� �佹


    [Tooltip("��������Խ��ĵ㡣Ĭ�������Ӧ��Ϊ���������ƵĽ�ɫ��Graphics�ڵ�")]
    [SerializeField]
    Transform targetTransform = null;
    [SerializeField]
    StateManger stataManager;
    [SerializeField]
    Vector3 offsetFromHead = Vector3.zero;

    [Tooltip("The interpolation speed used when the height of the character changes.")]
    [SerializeField]
    float heightLerpSpeed = 10f;


    public bool updateYaw = true;

    public float yawSpeed = 180f;


    [SerializeField] float lockDistance = 20f;
    [SerializeField] float lockEnemyMaxDistance = 30f;
    [SerializeField] float lockEnemyCameraMoveSpeed = 10f;
    [SerializeField] string lockEnemyTag = "Enemy";
    [SerializeField] Vector3 lockOffsetPosition;
    private bool lockFlag = false;
    public LayerMask lockMask;
    Transform nearestLockOnTarget;
    bool lockbutton;
    public Transform currentLockOnTarget;


    public bool updatePitch = true;

    [SerializeField]
    float initialPitch = 45f;

    public float pitchSpeed = 180f;

    [Range(1f, 85f)]
    public float maxPitchAngle = 80f;

    [Range(1f, 85f)]
    public float minPitchAngle = 80f;


    public bool updateZoom = true;

    [Min(0f)]
    [SerializeField]
    float distanceToTarget = 5f;

    [Min(0f)]
    public float zoomInOutSpeed = 40f;

    [Min(0f)]
    public float zoomInOutLerpSpeed = 5f;

    [Min(0f)]
    public float minZoom = 2f;

    [Min(0.001f)]
    public float maxZoom = 12f;


    public bool collisionDetection = true;
    public bool collisionAffectsZoom = false;
    public float detectionRadius = 0.5f;
    public LayerMask layerMask = 0;
    public bool considerKinematicRigidbodies = true;
    public bool considerDynamicRigidbodies = true;


    CharacterActor characterActor = null;
    Rigidbody characterRigidbody = null;

    float currentDistanceToTarget;
    float smoothedDistanceToTarget;

    float deltaYaw = 0f;
    float deltaPitch = 0f;
    float deltaZoom = 0f;

    Vector3 lerpedCharacterUp = Vector3.up;

    Transform viewReference = null;
    RaycastHit[] hitsBuffer = new RaycastHit[10];
    RaycastHit[] validHits = new RaycastHit[10];
    Vector3 characterPosition = default(Vector3);
    float lerpedHeight;

    void OnValidate()
    {
        initialPitch = Mathf.Clamp(initialPitch, -minPitchAngle, maxPitchAngle);
    }

    void Awake()
    {
        Initialize(targetTransform);
    }

    public bool Initialize(Transform targetTransform)
    {
        if (targetTransform == null)
            return false;

        characterActor = targetTransform.GetComponentInBranch<CharacterActor>();

        if (characterActor == null || !characterActor.isActiveAndEnabled)
        {
            Debug.Log("character actorΪ�ջ�Ϊ����");
            return false;
        }

        characterRigidbody = characterActor.GetComponent<Rigidbody>();


        GameObject referenceObject = new GameObject("Camera reference");
        viewReference = referenceObject.transform;

        return true;
    }

    void OnEnable()
    {
        if (characterActor == null)
            return;

        characterActor.OnTeleport += OnTeleport;
    }

    void OnDisable()
    {
        if (characterActor == null)
            return;

        characterActor.OnTeleport -= OnTeleport;
    }

    void Start()
    {
        characterPosition = targetTransform.position;

        previousLerpedCharacterUp = targetTransform.up;
        lerpedCharacterUp = previousLerpedCharacterUp;


        currentDistanceToTarget = distanceToTarget;
        smoothedDistanceToTarget = currentDistanceToTarget;

        viewReference.rotation = targetTransform.rotation;
        viewReference.Rotate(Vector3.right, initialPitch);

        lerpedHeight = characterActor.BodySize.y;
    }

    void Update()
    {
        if (targetTransform == null)
        {
            this.enabled = false;
            return;
        }
        UpdateInputValue();
        // An input axis value (e.g. mouse x) usually gets accumulated over time. So, the higher the frame rate the smaller the value returned.
        // In order to prevent inconsistencies due to frame rate changes, the camera movement uses a fixed delta time, instead of the old regular
        // delta time.
        float dt = Time.fixedDeltaTime;

        UpdateCamera(dt);
    }
    void OnTeleport(Vector3 position, Quaternion rotation)
    {
        viewReference.rotation = rotation;
        transform.rotation = viewReference.rotation;

        lerpedCharacterUp = characterActor.Up;
        previousLerpedCharacterUp = lerpedCharacterUp;

    }


    Vector3 previousLerpedCharacterUp = Vector3.up;

    /// <summary>
    /// ���½�ɫ����
    /// </summary>
    void UpdateInputValue()
    {
        Vector2 cameraAxes = inputHandlerSettings.InputHandler.GetVector2(axes);

        if (updatePitch)
            deltaPitch = -cameraAxes.y;

        if (updateYaw)
            deltaYaw = cameraAxes.x;

        if (updateZoom)
            deltaZoom = -inputHandlerSettings.InputHandler.GetFloat(zoomAxis);

        lockbutton = inputHandlerSettings.InputHandler.GetBool("Lock");

    }
    void UpdateCamera(float dt)
    {

        //�۽��ƶ�����ɫ��
        characterPosition = targetTransform.position;

        lerpedHeight = Mathf.Lerp(lerpedHeight, characterActor.BodySize.y, heightLerpSpeed * dt);
        Vector3 targetPosition = characterPosition + targetTransform.up * lerpedHeight + targetTransform.TransformDirection(offsetFromHead);
        viewReference.position = targetPosition;

        Vector3 finalPosition = viewReference.position;

        UpdateRotation(dt);
        // ------------------------------------------------------------------------------------------------------

        //currentDistanceToTarget += deltaZoom * zoomInOutSpeed * dt;
        //currentDistanceToTarget = Mathf.Clamp(currentDistanceToTarget, minZoom, maxZoom);
        currentDistanceToTarget = maxZoom;
        smoothedDistanceToTarget = Mathf.Lerp(smoothedDistanceToTarget, currentDistanceToTarget, zoomInOutLerpSpeed * dt);
        Vector3 displacement = -viewReference.forward * smoothedDistanceToTarget;

        if (collisionDetection)
        {
            bool hit = DetectCollisions(ref displacement, targetPosition);

            if (collisionAffectsZoom && hit)
            {
                currentDistanceToTarget = smoothedDistanceToTarget = displacement.magnitude;
            }
        }

        finalPosition = targetPosition + displacement;

        if (lockbutton)
        {
            lockFlag = HandleLockOn();
        }

        if (lockFlag)
        {
            updatePitch = updateYaw = false;
            HandleCamreaLock(dt);
        }
        transform.position = finalPosition;
        transform.rotation = viewReference.rotation;

    }
    #region Rotation

    void UpdateRotation(float dt)
    {
        // Rotation -----------------------------------------------------------------------------------------
        lerpedCharacterUp = targetTransform.up;

        // Rotate the reference based on the lerped character up vector 
        Quaternion deltaRotation = Quaternion.FromToRotation(previousLerpedCharacterUp, lerpedCharacterUp);
        previousLerpedCharacterUp = lerpedCharacterUp;

        viewReference.rotation = deltaRotation * viewReference.rotation;

        // ��ֱ��ת -----------------------------------------------------------------------------------------        
        viewReference.Rotate(lerpedCharacterUp, deltaYaw * yawSpeed * dt, Space.World);

        // Pitch rotation -----------------------------------------------------------------------------------------            

        float angleToUp = Vector3.Angle(viewReference.forward, lerpedCharacterUp);


        float minPitch = -angleToUp + (90f - minPitchAngle);
        float maxPitch = 180f - angleToUp - (90f - maxPitchAngle);

        float pitchAngle = Mathf.Clamp(deltaPitch * pitchSpeed * dt, minPitch, maxPitch);
        viewReference.Rotate(Vector3.right, pitchAngle);

    }
    #endregion

    bool DetectCollisions(ref Vector3 displacement, Vector3 lookAtPosition)
    {
        int hits = Physics.SphereCastNonAlloc(
            lookAtPosition,
            detectionRadius,
            Vector3.Normalize(displacement),
            hitsBuffer,
            currentDistanceToTarget,
            layerMask,
            QueryTriggerInteraction.Ignore
        );

        // Order the results
        int validHitsNumber = 0;
        for (int i = 0; i < hits; i++)
        {
            RaycastHit hitBuffer = hitsBuffer[i];

            Rigidbody detectedRigidbody = hitBuffer.collider.attachedRigidbody;

            // Filter the results ---------------------------
            if (hitBuffer.distance == 0)
                continue;

            if (detectedRigidbody != null)
            {
                if (considerKinematicRigidbodies && !detectedRigidbody.isKinematic)
                    continue;

                if (considerDynamicRigidbodies && detectedRigidbody.isKinematic)
                    continue;

                if (detectedRigidbody == characterRigidbody)
                    continue;
            }

            //----------------------------------------------            
            validHits[validHitsNumber] = hitBuffer;
            validHitsNumber++;
        }

        if (validHitsNumber == 0)
            return false;


        float distance = Mathf.Infinity;
        for (int i = 0; i < validHitsNumber; i++)
        {
            RaycastHit hitBuffer = validHits[i];

            if (hitBuffer.distance < distance)
                distance = hitBuffer.distance;
        }

        displacement = CustomUtilities.Multiply(Vector3.Normalize(displacement), distance);


        return true;
    }
    #region lock
    bool HandleLockOn()
    {
        bool result = FindLockObject();
        if (result)
        {
            stataManager.HandleLock();
        }
        return result;
    }
    void HandleCamreaLock(float dt)
    {
        Vector3 direction = currentLockOnTarget.position - viewReference.position;
        if (direction.sqrMagnitude >= lockEnemyMaxDistance * lockEnemyMaxDistance)
        {
            currentLockOnTarget = null;
            stataManager.HandleLock();
            lockFlag = false;
            updatePitch = updateYaw = true;
            return;
        }
        direction.Normalize();
        var targetRotation = Quaternion.LookRotation(direction);
        viewReference.rotation = targetRotation;
    }
    bool FindLockObject()
    {
        List<Enemy> avilableTargets = new List<Enemy>();
        float shortTargetDistance = Mathf.Infinity;

        Collider[] colliders = Physics.OverlapSphere(targetTransform.position, lockDistance, lockMask);
        Debug.Log("colliders :" + colliders.Length);
        for (int i = 0; i < colliders.Length; i++)
        {
            Enemy character = colliders[i].GetComponent<Enemy>();
            if (character != null)
            {
                var lockTargetDirection = Vector3.ProjectOnPlane((character.transform.position - targetTransform.position), targetTransform.up);
                float distanceFormTargetSqr = lockTargetDirection.sqrMagnitude;
                lockTargetDirection.Normalize();
                //������ǿ�����˷�����������ĽǶ�
                float viewableAngle = Vector3.Angle(lockTargetDirection, targetTransform.forward);
                if (character.transform.root != targetTransform.transform.root && viewableAngle > -60
                    && viewableAngle < 60 && distanceFormTargetSqr <= lockDistance * lockDistance)
                {
                    avilableTargets.Add(character);
                }
            }
        }
        Debug.Log("TargetCount :" + avilableTargets.Count);
        for (int i = 0; i < avilableTargets.Count; i++)
        {
            if (avilableTargets[i].tag != lockEnemyTag)
                continue;
            float distance = Vector3.Distance(targetTransform.position, avilableTargets[i].transform.position);
            if (distance < shortTargetDistance)
            {
                shortTargetDistance = distance;
                nearestLockOnTarget = avilableTargets[i].transform;
            }
        }
        if (nearestLockOnTarget != null)
        {
            currentLockOnTarget = nearestLockOnTarget;
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
