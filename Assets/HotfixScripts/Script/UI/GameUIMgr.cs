using Audio;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameUIMgr : UIWindow
{
    private string activePanle = string.Empty;
    [SerializeField]
    public AudioData audios;
    #region AudioName
    [Header("UI“Ù–ß")]
    [SerializeField]
    private string click;
    #endregion
    //---------------------------------------------------------------------
    private void OpenNetworkPanel(PointerEventData eventData)
    {
        //AudioManager.Instance.PlayAudio(audios.GetClip(click),AudioLayer.Sound);
        if (activePanle == "Network")
        {
            activePanle = string.Empty;
           // UIManager.Instance.DisableWindow<NetPanel>();
        }
        else
        {
            activePanle = "Network";
            //UIManager.Instance.EnableWindow<NetPanel>();
        }
    }

    private void OpenAudioPanel(PointerEventData eventData)
    {

    }
    public override void OnCreate()
    {
        
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnDelete()
    {
        
    }

    public override void OnFocus()
    {
        
    }

    public override void OnFocusOtherUI()
    {
       
    }
}
