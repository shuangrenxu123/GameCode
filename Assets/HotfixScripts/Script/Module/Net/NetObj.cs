using Animancer;
using PlayerInfo;
using UnityEngine;

public class NetObj : MonoBehaviour
{
    public string id;

    [Header("网络位置纠偏")]
    [SerializeField, Min(0f)]
    private float interpolationThresholdA = 0.35f;

    [SerializeField, Min(0f)]
    private float teleportThresholdB = 3f;

    [SerializeField, Min(0.01f)]
    private float interpolationDuration = 0.12f;

    [SerializeField]
    private bool clearVelocityWhenTeleport = true;

    private bool isInterpolating;
    private float interpolationTimer;
    private Vector3 interpolationStartPosition = Vector3.zero;
    private Vector3 interpolationTargetPosition = Vector3.zero;
    private PhysicsActor physicsActor;

    #region movement
    Vector2 lastMove = Vector2.zero;
    #endregion

    #region Animator

    private AnimatorHelper animacer;
    private LinearMixerTransition currentAnimator;
    [SerializeField]
    CCAnimatorConfig config;

    #endregion

    private void Awake()
    {
        physicsActor = GetComponentInParent<PhysicsActor>();
    }

    private void Start()
    {
        animacer = new AnimatorHelper(GetComponentInChildren<AnimancerComponent>());
        currentAnimator = config.linearMixerAnimators["NormalMove"];
        animacer.Play(currentAnimator);
    }
    public void SyncData(DefaultNetWorkPackage data)
    {
        if (data.MsgId == 1)
        {
            SyncPostion(data);
        }
    }
    private void SyncPostion(DefaultNetWorkPackage arg0)
    {
        var state = arg0.MsgObj as CharacterState;
        if (state == null)
        {
            return;
        }

        lastMove = new Vector2(state.MovementX, state.MovementY);
        currentAnimator.State.Parameter = state.MovementY;

        Vector3 netPosition = NetWorkUtility.ToUnityV3(state.Position);
        float distance = Vector3.Distance(GetCurrentPosition(), netPosition);
        float interpolationThreshold = Mathf.Max(0f, interpolationThresholdA);
        float teleportThreshold = Mathf.Max(interpolationThreshold, teleportThresholdB);

        if (distance > teleportThreshold)
        {
            TeleportTo(netPosition);
            return;
        }

        if (distance > interpolationThreshold)
        {
            BeginInterpolation(netPosition);
        }
        else
        {
            StopInterpolation();
        }
    }
    private void Update()
    {
        if (!isInterpolating)
        {
            return;
        }

        interpolationTimer += Time.deltaTime;
        float duration = Mathf.Max(0.01f, interpolationDuration);
        float t = Mathf.Clamp01(interpolationTimer / duration);
        SetCurrentPosition(Vector3.Lerp(interpolationStartPosition, interpolationTargetPosition, t));

        if (t >= 1f)
        {
            StopInterpolation();
        }
    }

    private void Reset()
    {
        interpolationStartPosition = interpolationTargetPosition = Vector3.zero;
        interpolationTimer = 0f;
        isInterpolating = false;
    }

    private void OnValidate()
    {
        if (teleportThresholdB < interpolationThresholdA)
        {
            teleportThresholdB = interpolationThresholdA;
        }
    }

    private Vector3 GetCurrentPosition()
    {
        return physicsActor != null ? physicsActor.Position : transform.position;
    }

    private void SetCurrentPosition(Vector3 position)
    {
        if (physicsActor != null)
        {
            physicsActor.Position = position;
            return;
        }

        transform.position = position;
    }

    private void BeginInterpolation(Vector3 targetPosition)
    {
        interpolationStartPosition = GetCurrentPosition();
        interpolationTargetPosition = targetPosition;
        interpolationTimer = 0f;
        isInterpolating = true;
    }

    private void StopInterpolation()
    {
        isInterpolating = false;
        interpolationTimer = 0f;
    }

    private void TeleportTo(Vector3 targetPosition)
    {
        StopInterpolation();

        if (physicsActor != null)
        {
            physicsActor.Teleport(targetPosition, transform.rotation);
            if (clearVelocityWhenTeleport)
            {
                physicsActor.Velocity = Vector3.zero;
            }
            return;
        }

        transform.position = targetPosition;
    }

}
