using PlayerInfo;
using UnityEngine;

public class NetObj : MonoBehaviour
{
    public string id;
    MotionState lastMotionState;
    [HideInInspector]
    private float syncDelta = 1;
    private float smoothTick;
    public NetAnimator anim;
    Vector3 forcastPosition = Vector3.zero;
    Vector3 startPosition = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    Vector2 lastMove = Vector2.zero;

    private void Start()
    {
        lastMotionState = new MotionState();
        lastMotionState.lastMotionTime = float.MinValue;
        anim =GetComponentInChildren<NetAnimator>();
    }
    public void SyncData(DefaultNetWorkPackage data)
    {
        if(data.MsgId == 1)
        {
            SyncPostion(data);
        }
        ///相关的动画事件，如后滚之类的
        else if(data.MsgId == 2)
        {
            SyncOtherAnim(data);
        }
    }
    private void SyncPostion(DefaultNetWorkPackage arg0)
    {
        var state = (move)arg0.Msgobj;
        if (state != null)
        {
            syncDelta = Time.time - lastMotionState.lastMotionTime;
            lastMotionState.lastMotionTime = Time.time;
            forcastPosition = NetWorkUtility.ToUnityV3(state.Position) + NetWorkUtility.ToUnityV3(state.Velocity) * syncDelta;
            startPosition = transform.position;
            smoothTick = syncDelta;
            transform.rotation = Quaternion.Euler(NetWorkUtility.ToUnityV3(state.Rotation));
            //lastMove = new Vector2(state.V, state.H);
            anim.UpdateAnimatorValues(state.V, state.H);
        }
    }
    private void SyncOtherAnim(DefaultNetWorkPackage arg0)
    {
        var animName = (Action)arg0.Msgobj;
        if(animName != null)
        {
            Debug.Log(animName.Actionname);
            anim.PlayTargetAnimation(animName.Actionname);
        }
    }
    private void Update()
    {
        if (smoothTick > 0)
        {
            transform.position = startPosition + (forcastPosition - startPosition) * (1 - smoothTick / syncDelta);
            smoothTick -= Time.deltaTime;
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
        //anim.UpdateAnimatorValues(lastMove.x, lastMove.y);
    }

}
