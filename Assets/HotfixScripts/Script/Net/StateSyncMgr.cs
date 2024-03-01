using Network;
using PlayerInfo;
using System.Collections.Generic;
using UnityEngine;

public class StateSyncMgr : MonoBehaviour
{
    public Player player;
    public Dictionary<string, NetObj> netObjs = new();
    public GameObject prefab;
    [HideInInspector]
    //多久更新一次ping值，单位s
    private float time = 1f;
    //定时器
    private float timer = 0;
    private void Start()
    {
        NetWorkManager.Instance.RegisterHandle(1, SyncGameObjectState);
        NetWorkManager.Instance.RegisterHandle(0, Heartbeat);
        NetWorkManager.Instance.RegisterHandle(4, OnClientJoined);
        EventManager.Instance.AddListener("ConnectServerSuccess",SendPlayerState);
    }

    private void SendPlayerState(object message)
    {
        var package = new Login()
        {
            Armid = 0,
            Bodyid = 0,
            Handid = 0,
            Headid = 0,
            LeftWeaponid = 0,
            RightWeaponid = 0,
            Legid = 0,
            Trousers = 0,
        };
        NetWorkManager.Instance.SendMessage(player.id,4, package);
    }

    private void OnClientJoined(DefaultNetWorkPackage package)
    {
        Debug.Log("有玩家加入了房间:"+package.SenderId);

        if(package.SenderId==player.id || netObjs.ContainsKey(package.SenderId))
        {
            return;
        }

        var go = InstantiateNetObject(package.SenderId);
        netObjs.Add(package.SenderId, go);
        
    }

    private void Update()
    {
        SendPing();
    }

    void SendPing()
    {
        if (timer >= time && NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            NetWorkManager.Instance.SendMessage(player.id, 0, new ping()
            {
                Timer = Time.time.ToString(),
            });
            timer -= time;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
    void Heartbeat(DefaultNetWorkPackage package)
    {
        if(package.SenderId == player.id)
        {
            SetPingValue(package);
            return;
        }
    }
    /// <summary>
    /// 同步坐标位置
    /// </summary>
    /// <param name="package"></param>
    void SyncGameObjectState(DefaultNetWorkPackage package)
    {
        if (package.SenderId == player.id)
            return;
        if (netObjs.ContainsKey(package.SenderId))
        {
            netObjs[package.SenderId].SyncData(package);
        }
        else
        {
            var go = InstantiateNetObject(package.SenderId);
            netObjs.Add(package.SenderId, go);

        }

    }
    private void SetPingValue(DefaultNetWorkPackage data)
    {
        var obj = data.Msgobj as ping;
        if (obj != null)
        {
            UIManager.Instance.GetUIWindow<PingPanel>().SetPingValue((int)(timer / 2 * 1000));
            return;
        }
    }
    private NetObj InstantiateNetObject(string name = "")
    {
        var go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        //go.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = name;
        go.AddComponent<NetObj>().id = name;
        return go.GetComponent<NetObj>();
    }
}
