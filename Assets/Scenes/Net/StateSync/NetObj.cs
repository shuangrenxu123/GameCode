using PlayerInfo;
using UnityEngine;

public class NetObj : MonoBehaviour
{
    public string id;
    MotionState lastMotionState;
    [HideInInspector]
    private float syncDelta = 1;
    private float smoothTick;
    //public float last
    Vector3 forcastPosition = Vector3.zero;
    Vector3 startPosition = Vector3.zero;
    Vector3 velocity = Vector3.zero;

    private void Start()
    {
        lastMotionState = new MotionState();
        lastMotionState.lastMotionTime = float.MinValue;
    }

    public void SyncPostion(DefaultNetWorkPackage arg0)
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
        }
    }
    private void FixedUpdate()
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
    }

}
