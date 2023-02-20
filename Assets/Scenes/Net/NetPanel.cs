using NetWork;
using PlayerInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetPanel : WindowRoot
{
    [SerializeField]
    private GameObject Ping;
    public override void Start()
    {
        Ping = GetUI("ping");
        var text = GetUI("ip").GetComponent<TMP_InputField>().text;
        GetUI("Conn").GetComponent<Button>().onClick.AddListener(() => {
            NetWorkManager.Instance.ConnectServer(text,
                int.Parse(GetUI("port").GetComponent<TMP_InputField>().text));
        });
        GetUI("send").GetComponent<Button>().onClick.AddListener(() =>
        {
            NetWorkManager.Instance.SendMessage(1,
                new PlayerInfo.move()
                {
                    Id = "xu",
                    Position = new PlayerInfo.vector3(),
                    Rotation = new PlayerInfo.vector3(),
                    Velocity= new PlayerInfo.vector3(),
                }
                );
        });
        GetUI("close").GetComponent<Button>().onClick.AddListener(() =>
        {
            NetWorkManager.Instance.DisConnectServer();
        });
        GetUI("id").GetComponent<TMP_InputField>().onValueChanged.AddListener(e=> GameObject.Find("Cube")
        .GetComponent<Player>().id = e);
    }


    public override void Update()
    {
    }

    public override void UpdateWindow() 
    {
        
    }
    public void SetPingValue(float value)
    {
        Ping.GetComponent<TMP_Text>().text = ((int)value).ToString();
    }

}
