using NetWork;
using PlayerInfo;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public string id;
    public CombatEntity entity;
    public PlayerController controller;
    //网络同步部分
    MotionState lastMotionState;
    /// <summary>
    /// 位置差。当大于该值时才会发送坐标
    /// </summary>
    public float deadReckoningThreshold = 1f;
    /// <summary>
    /// 预测位置，即在其他客户端中的位置
    /// </summary>
    private Vector3 drPosition = Vector3.zero;
    private void Awake()
    {
        entity = GetComponent<CombatEntity>();
        controller = GetComponent<PlayerController>();
    }
    void Start()
    {
        entity.Init(1000);
        InvokeRepeating("SendPosition", time: 0f, 0.05f);
    }
    private void FixedUpdate()
    {
        //drPosition = lastMotionState.Position + lastMotionState.velocity * (Time.time - lastMotionState.lastMotionTime);
        //if ((drPosition - transform.position).sqrMagnitude > deadReckoningThreshold)
        //{
        //    SendPosition();
        //}
    }
    public void SyncPlayerLastMotionState(DefaultNetWorkPackage package)
    {
        var state = (move)package.Msgobj;
        if (state != null)
        {
            lastMotionState.lastMotionTime = Time.time;
            lastMotionState.Position = NetWorkUtility.ToUnityV3(state.Position);
            lastMotionState.velocity = NetWorkUtility.ToUnityV3(state.Velocity);
        }
    }
    void Update()
    {

    }
    private void SendPosition()
    {
        if (NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            move state = new move()
            {
                Id = id,
                Position = NetWorkUtility.ToProtoBufV3(transform.position),
                Rotation = NetWorkUtility.ToProtoBufV3(transform.eulerAngles),
                Velocity = NetWorkUtility.ToProtoBufV3(Vector3.zero),
            };
            NetWorkManager.Instance.SendMessage(1, state);
        }
    }
}
public struct MotionState
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 velocity;
    public float lastMotionTime;
}
