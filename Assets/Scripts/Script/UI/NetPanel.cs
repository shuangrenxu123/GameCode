using NetWork;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetPanel : WindowRoot
{
    private GameObject id;
    private TMP_InputField ip;
    private TMP_InputField port;
    private Player player;
    public override void Start()
    {
        gameObject.SetActive(false);
        ip = GetUIGameObject("ip").GetComponent<TMP_InputField>();
        port = GetUIGameObject("port").GetComponent<TMP_InputField>();

        id = GetUIGameObject("id");
        id.GetComponent<TMP_InputField>().onValueChanged.AddListener(SetIDValue);

        GetUIGameObject("conn").GetComponent<Button>().onClick.AddListener(ConnSer);
        GetUIGameObject("close").GetComponent<Button>().onClick.AddListener(CloseSer);

        player = FindObjectOfType<Player>();
    }

    private void CloseSer()
    {
        NetWorkManager.Instance.DisConnectServer();
    }

    private void ConnSer()
    {
        NetWorkManager.Instance.ConnectServer(ip.text,int.Parse(port.text));
    }

    private void SetIDValue(string arg0)
    {
        player.id = arg0;
        Debug.Log(player.id);
    }

    public override void Update()
    {
    }

    public override void UpdateWindow()
    {

    }
    public void SetPingValue(float value)
    {
    }

}
