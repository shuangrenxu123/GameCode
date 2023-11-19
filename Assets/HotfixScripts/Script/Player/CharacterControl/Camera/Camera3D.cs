using UnityEngine;

[DefaultExecutionOrder(110)]
public class Camera3D : MonoBehaviour
{

    [Header("输入设置相关")]

    [SerializeField]
    InputHandlerSettings inputHandlerSettings = new InputHandlerSettings();

    [SerializeField]
    string axes = "Camera";

    [SerializeField]
    string zoomAxis = "Camera Zoom"; //zoom ： 变焦

    [Header("Target")]

    [Tooltip("摄像机所对焦的点。默认情况下应该为我们所控制的角色的Graphics节点")]
    [SerializeField]
    Transform targetTransform = null;

    [SerializeField]
    Vector3 offsetFromHead = Vector3.zero;

    [Tooltip("The interpolation speed used when the height of the character changes.")]
    [SerializeField]
    float heightLerpSpeed = 10f;

    [Header("View")]

    public CameraMode cameraMode = CameraMode.ThirdPerson;

    [Header("第一人称")]

    public bool hideBody = true;

    [SerializeField]
    GameObject bodyObject = null;

    [Header("Yaw")]

    public bool updateYaw = true;

    public float yawSpeed = 180f;


    [Header("Pitch")]

    public bool updatePitch = true;

    [SerializeField]
    float initialPitch = 45f;

    public float pitchSpeed = 180f;

    [Range(1f, 85f)]
    public float maxPitchAngle = 80f;

    [Range(1f, 85f)]
    public float minPitchAngle = 80f;


    [Header("缩放")]
    public bool updateRoll = false;


    [Header("Zoom (Third person)")]

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


    [Header("Collision")]

    public bool collisionDetection = true;
    public bool collisionAffectsZoom = false;
    public float detectionRadius = 0.5f;
    public LayerMask layerMask = 0;
    public bool considerKinematicRigidbodies = true;
    public bool considerDynamicRigidbodies = true;

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────


    CharacterActor characterActor = null;
    Rigidbody characterRigidbody = null;

    float currentDistanceToTarget;
    float smoothedDistanceToTarget;

    float deltaYaw = 0f;
    float deltaPitch = 0f;
    float deltaZoom = 0f;

    Vector3 lerpedCharacterUp = Vector3.up;

    Transform viewReference = null;
    Renderer[] bodyRenderers = null;
    RaycastHit[] hitsBuffer = new RaycastHit[10];
    RaycastHit[] validHits = new RaycastHit[10];
    Vector3 characterPosition = default(Vector3);
    float lerpedHeight;

    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson,
    }


    public void ToggleCameraMode()
    {
        cameraMode = cameraMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
    }



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
            Debug.Log("character actor为空或为启用");
            return false;
        }

        characterRigidbody = characterActor.GetComponent<Rigidbody>();

        inputHandlerSettings.Initialize(gameObject);

        GameObject referenceObject = new GameObject("Camera reference");
        viewReference = referenceObject.transform;

        if (bodyObject != null)
            bodyRenderers = bodyObject.GetComponentsInChildren<Renderer>();

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

        Vector2 cameraAxes = inputHandlerSettings.InputHandler.GetVector2(axes);

        if (updatePitch)
            deltaPitch = -cameraAxes.y;

        if (updateYaw)
            deltaYaw = cameraAxes.x;

        if (updateZoom)
            deltaZoom = -inputHandlerSettings.InputHandler.GetFloat(zoomAxis);

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

    void HandleBodyVisibility()
    {
        if (cameraMode == CameraMode.FirstPerson)
        {
            if (bodyRenderers != null)
                for (int i = 0; i < bodyRenderers.Length; i++)
                {
                    if (bodyRenderers[i].GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)bodyRenderers[i];
                        if (skinnedMeshRenderer != null)
                            skinnedMeshRenderer.forceRenderingOff = hideBody;
                    }
                    else
                    {
                        bodyRenderers[i].enabled = !hideBody;
                    }
                }

        }
        else
        {
            if (bodyRenderers != null)
                for (int i = 0; i < bodyRenderers.Length; i++)
                {
                    if (bodyRenderers[i] == null)
                        continue;

                    if (bodyRenderers[i].GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)bodyRenderers[i];
                        if (skinnedMeshRenderer != null)
                            skinnedMeshRenderer.forceRenderingOff = false;
                    }
                    else
                    {
                        bodyRenderers[i].enabled = true;
                    }


                }
        }
    }

    void UpdateCamera(float dt)
    {
        // Body visibility ---------------------------------------------------------------------
        HandleBodyVisibility();

        // Rotation -----------------------------------------------------------------------------------------
        lerpedCharacterUp = targetTransform.up;

        // Rotate the reference based on the lerped character up vector 
        Quaternion deltaRotation = Quaternion.FromToRotation(previousLerpedCharacterUp, lerpedCharacterUp);
        previousLerpedCharacterUp = lerpedCharacterUp;

        viewReference.rotation = deltaRotation * viewReference.rotation;



        // Yaw rotation -----------------------------------------------------------------------------------------        
        viewReference.Rotate(lerpedCharacterUp, deltaYaw * yawSpeed * dt, Space.World);

        // Pitch rotation -----------------------------------------------------------------------------------------            

        float angleToUp = Vector3.Angle(viewReference.forward, lerpedCharacterUp);


        float minPitch = -angleToUp + (90f - minPitchAngle);
        float maxPitch = 180f - angleToUp - (90f - maxPitchAngle);

        float pitchAngle = Mathf.Clamp(deltaPitch * pitchSpeed * dt, minPitch, maxPitch);
        viewReference.Rotate(Vector3.right, pitchAngle);

        // Roll rotation -----------------------------------------------------------------------------------------    
        if (updateRoll)
        {
            viewReference.up = lerpedCharacterUp;
        }

        // Position of the target -----------------------------------------------------------------------
        characterPosition = targetTransform.position;

        lerpedHeight = Mathf.Lerp(lerpedHeight, characterActor.BodySize.y, heightLerpSpeed * dt);
        Vector3 targetPosition = characterPosition + targetTransform.up * lerpedHeight + targetTransform.TransformDirection(offsetFromHead);
        viewReference.position = targetPosition;

        Vector3 finalPosition = viewReference.position;

        // ------------------------------------------------------------------------------------------------------
        if (cameraMode == CameraMode.ThirdPerson)
        {
            currentDistanceToTarget += deltaZoom * zoomInOutSpeed * dt;
            currentDistanceToTarget = Mathf.Clamp(currentDistanceToTarget, minZoom, maxZoom);

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
        }


        transform.position = finalPosition;
        transform.rotation = viewReference.rotation;

    }

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


}
