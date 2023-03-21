using NetWork;
using PlayerInfo;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StateSyncMgr : MonoBehaviour
{
    public Player player;
    public Dictionary<string, NetObj> netObjs = new();
    public GameObject prefab;
    [HideInInspector]
    //多久更新一次ping值，s
    private float time = 1f;
    //定时器
    private float timer = 0;
    private void Start()
    {
        NetWorkManager.Instance.handle.AddListener(SyncGameObjectState);
    }
    private void Update()
    {
        SendPing();
    }

    void SendPing()
    {
        if (timer >= time && NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            NetWorkManager.Instance.SendMessage(player.id ,0, new ping()
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
    void SyncGameObjectState(DefaultNetWorkPackage package)
    {
        if(package.MsgId == 0 && package.SenderId == player.id)
        {
            SetPingValue(package);
            return;
        }
        if (package.SenderId == player.id)
            return;
        if (netObjs.ContainsKey(package.SenderId))
        {
            netObjs[package.SenderId].SyncData(package);
        }
        else
        {
            var go = InstantiateNetObject(package.SenderId);
            netObjs.Add(package.SenderId,go);
        }

    }
    private void SetPingValue(DefaultNetWorkPackage data)
    {
        var obj = data.Msgobj as ping;
        if (obj != null)
        {
            WindowsManager.Instance.GetUiWindow<NetPanel>().SetPingValue(timer / 2 * 1000);
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
