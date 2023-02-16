using NetWork;
using PlayerInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StateSyncMgr : MonoBehaviour
{
    public NetObj me;
    public List<NetObj> netObjs= new List<NetObj>();
    public GameObject prefab;
    [HideInInspector]
    //多久更新一次ping值，s
    private float time = 1;
    //定时器
    private float timer = 0;
    private float pingValue = 0;
    private void Start()
    {
        NetWorkManager.Instance.handle.AddListener(MoveObject);
    }
    private void Update()
    {
        if (timer >= time && NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            timer = 0;
            SendPing();
        }
        else
        {
            timer += Time.deltaTime;
        }
        pingValue += Time.deltaTime;
        //WindowsManager.Instance.GetUiWindow<NetPanel>().SetPingValue(pingValue / 2 * 1000, pingValue * 1000);

    }

    void SendPing()
    {
        pingValue = 0;
        NetWorkManager.Instance.SendMessage(0, new ping()
        {
            Id = me.id,
        });
    }
    void MoveObject(DefaultNetWorkPackage package)
    {
        if ((package.Msgobj as ping) != null)
        {
            WindowsManager.Instance.GetUiWindow<NetPanel>().SetPingValue(pingValue/2 * 1000,pingValue*1000);
            return;
        }
        var msg = package.Msgobj as move;

        if (msg == null)
            return;
        NetObj obj = netObjs.Find(x => x.id == msg.Id);
        if (msg.Id == me.id)
            return;
        if (obj != null)
        {
            obj.SyncPostion(package);
        }
        else
        {
            var go =  InstantiateNetObject(msg.Id);
            netObjs.Add(go);
        }

    }
    private NetObj InstantiateNetObject(string name = "")
    {
        var go = Instantiate(prefab,Vector3.zero,Quaternion.identity);
        go.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = name;
        go.GetComponent<NetObj>().id = name;
        return go.GetComponent<NetObj>();
    }
}
