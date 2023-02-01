using NetWork;
using PlayerInfo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StateSyncMgr : MonoBehaviour
{

    public GameObject prefab;
    private void Start()
    {
        NetWorkManager.Instance.handle.AddListener(Login);
        NetWorkManager.Instance.handle.AddListener(MoveObject);
    }

    void Login(DefaultNetWorkPackage package)
    {
        if (package.MsgId == 1)
        {
        Debug.Log(package.MsgId);
        Instantiate(prefab,new Vector3(0,0,0),Quaternion.identity).transform.GetChild(0).
            GetComponent<TextMeshPro>().text = package.MsgId.ToString();
        }
    }
    void MoveObject(DefaultNetWorkPackage package)
    {
        if (package.MsgId == 2)
        {
            
        }
    }
}
