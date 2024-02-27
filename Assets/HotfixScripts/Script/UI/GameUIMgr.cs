using Audio;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameUIMgr : WindowRoot
{
    private string activePanle = string.Empty;
    [SerializeField]
    public AudioData audios;
    #region AudioName
    [Header("UI“Ù–ß")]
    [SerializeField]
    private string click;
    #endregion
    public override void Start()
    {
        WindowsManager.Instance.DisableWindow<GameUIMgr>();
        GetUIEvnetListener("Network").PointerClick += OpenNetworkPanel;
        //GetUIEvnetListener("Settings").PointerClick += OpenAudioPanel;
    }
    //---------------------------------------------------------------------
    private void OpenNetworkPanel(PointerEventData eventData)
    {
        //AudioManager.Instance.PlayAudio(audios.GetClip(click),AudioLayer.Sound);
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
