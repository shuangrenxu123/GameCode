using NetWork;
using PlayerInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetObj : MonoBehaviour
{
    public bool isplayer = false;
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
        if(state != null)
        {
            transform.position = NetWorkUtility.ToUnityV3(state.Position);
            transform.eulerAngles = NetWorkUtility.ToUnityV3(state.Rotation);
        }
        else
        {
            Debug.Log("接收到的消息为空");
        }
        syncDelta = Time.time - lastMotionState.lastMotionTime;
        lastMotionState.lastMotionTime = Time.time;
        forcastPosition = NetWorkUtility.ToUnityV3(state.Position)+ NetWorkUtility.ToUnityV3(state.Velocity) * syncDelta;
        startPosition = transform.position;
        smoothTick = syncDelta;
        transform.rotation = Quaternion.Euler(NetWorkUtility.ToUnityV3(state.Rotation));

    }
    private void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (!isplayer)
        {
            if (smoothTick > 0)
            {
                transform.position = startPosition + (forcastPosition - startPosition) * (1 - smoothTick / syncDelta);
                smoothTick -= Time.deltaTime;
                //
                //transform.rotation = Quaternion.Lerp(Quaternion.Euler(transform.eulerAngles),
                //                          Quaternion.Euler(m_syncPlayerState.rotation), syncDelta);
            }
            else
            {
                transform.position += velocity * Time.deltaTime;
            }
        }
        else if(NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            SendPos();
        }
    }
    private void SendPos()
    {
        move state = new move()
        {
            Id = id,
            Position = NetWorkUtility.ToProtoBufV3(transform.position - transform.forward),
            Rotation = NetWorkUtility.ToProtoBufV3(transform.eulerAngles),
            Velocity = NetWorkUtility.ToProtoBufV3(Vector3.zero)
        };

        NetWorkManager.Instance.SendMessage(1,state);
    }

}
    public struct MotionState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 velocity;
        public float lastMotionTime;
    }
