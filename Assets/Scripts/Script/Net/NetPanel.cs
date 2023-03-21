using NetWork;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetPanel : WindowRoot
{
    [SerializeField]
    private GameObject Ping;
    public Player player;
    public override void Start()
    {
        Ping = GetUI("ping");
        var text = GetUI("ip").GetComponent<TMP_InputField>().text;
        GetUI("Conn").GetComponent<Button>().onClick.AddListener(() =>
        {
            NetWorkManager.Instance.ConnectServer(text,
                int.Parse(GetUI("port").GetComponent<TMP_InputField>().text));
        });
        GetUI("id").GetComponent<TMP_InputField>().onValueChanged.AddListener(
            i => player.id = i
            );
        GetUI("close").GetComponent<Button>().onClick.AddListener(() =>
        {
            NetWorkManager.Instance.DisConnectServer();
        });
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
