using NetWork;
using PlayerInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTranform : MonoBehaviour
{
    [Header("同步频率")]
    public int Count;
    public Player player;
    public Animator anim;
    private void Start()
    {
        InvokeRepeating("SendPosition",0,1f/Count);
    }
    public void SendPosition()
    {
        if (NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            NetWorkManager.Instance.SendMessage(
                player.id,
                1,
                new move()
                {
                    Position = NetWorkUtility.ToProtoBufV3(transform.position),
                    Rotation = NetWorkUtility.ToProtoBufV3(transform.rotation.eulerAngles),
                    Velocity = NetWorkUtility.ToProtoBufV3(Vector3.zero),
                    H = anim.GetFloat("Horizontal"),
                    V= anim.GetFloat("Verical"),
                }
                );
        }
    }

    public void SendAction(string actionName)
    {
        if (NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            NetWorkManager.Instance.SendMessage(
                player.id,
                2,
                new Action()
                {
                    Actionname = actionName
                }
            );
        }
    }
}
