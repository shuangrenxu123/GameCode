using UnityEngine.EventSystems;

public class GameUIMgr : WindowRoot
{
    private string activePanle = string.Empty;

    public override void Start()
    {
        WindowsManager.Instance.DisableWindow<GameUIMgr>();
        GetUIEvnetListener("Network").PointerClick += OpenNetworkPanel;
        GetUIEvnetListener("Audio").PointerClick += OpenAudioPanel;
    }
    //---------------------------------------------------------------------
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

    private void OpenAudioPanel(PointerEventData eventData)
    {

    }

    public override void Update()
    {
    }

}
