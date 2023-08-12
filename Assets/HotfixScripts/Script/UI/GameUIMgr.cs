using UnityEngine;
using UnityEngine.EventSystems;

public class GameUIMgr : WindowRoot
{
    private string activePanle = string.Empty;
    GameObject statePanel;
    public override void Start()
    {
        WindowsManager.Instance.DisableWindow<GameUIMgr>();
        GetUIEvnetListener("Network").PointerClick += OpenNetworkPanel;
    }

    private void OpenNetworkPanel(PointerEventData eventData)
    {
        if (activePanle == "Network")
        {
            activePanle = string.Empty;
            WindowsManager.Instance.DisableWindow<NetPanel>();
        }
        else
        {
            activePanle = "Network";
            WindowsManager.Instance.EnableWindow<NetPanel>();
        }
    }

    public override void Update()
    {
    }

}
