using Animancer;
using PlayerInfo;
using UnityEngine;

public class NetObj : MonoBehaviour
{
    public string id;
    MotionState lastMotionState;
    [HideInInspector]
    private float syncDelta = 1;
    private float smoothTick;
    #region movement
    Vector3 targetPosition = Vector3.zero;
    Vector3 startPosition = Vector3.zero;

    Quaternion tartgetRotation;
    Quaternion startRotation;

    Vector3 velocity = Vector3.zero;
    Vector2 lastMove = Vector2.zero;
    #endregion

    #region Animator

    private AnimatorHelper animacer;
    private LinearMixerTransition currentAnimator;
    [SerializeField]
    CCAnimatorConfig config;

    #endregion
    private void Start()
    {
        lastMotionState = new MotionState();
        lastMotionState.lastMotionTime = float.MinValue;
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
        ///相关的动画事件，如后滚之类的
        else if (data.MsgId == 2)
        {
            SyncOtherAnim(data);
        }
    }
    private void SyncPostion(DefaultNetWorkPackage arg0)
    {
        var state = (move)arg0.MsgObj;
        if (state != null)
        {
            syncDelta = Time.time - lastMotionState.lastMotionTime;
            lastMotionState.lastMotionTime = Time.time;
            targetPosition = NetWorkUtility.ToUnityV3(state.Position) + NetWorkUtility.ToUnityV3(state.Velocity) * syncDelta;
            startPosition = transform.position;
            smoothTick = syncDelta;
            tartgetRotation = Quaternion.Euler(NetWorkUtility.ToUnityV3(state.Rotation));
            lastMove = new Vector2(state.V, state.H);
            currentAnimator.State.Parameter = state.V;

            velocity = (targetPosition - startPosition) / smoothTick;
        }
    }
    private void SyncOtherAnim(DefaultNetWorkPackage arg0)
    {
        var animName = (Action)arg0.MsgObj;
        if (animName != null)
        {
            animacer.Play(config.clipAnimators[animName.Actionname]);
        }
    }
    private void Update()
    {
        if (smoothTick > 0)
        {
            if ((targetPosition - startPosition).magnitude > StateSyncMgr.MaxDisplacement)
            {
                smoothTick = 0;
                transform.SetPositionAndRotation(targetPosition, tartgetRotation);
                velocity = Vector3.zero;
                return;
            }
            float timer = Time.deltaTime / smoothTick;

            transform.SetPositionAndRotation(Vector3.Lerp(startPosition, targetPosition, timer), Quaternion.Slerp(startRotation, tartgetRotation, timer));

            smoothTick -= Time.deltaTime;
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
    }

    private void Reset()
    {
        startPosition = targetPosition = Vector3.zero;
        startRotation = tartgetRotation = Quaternion.identity;
        smoothTick = 0;

    }

}