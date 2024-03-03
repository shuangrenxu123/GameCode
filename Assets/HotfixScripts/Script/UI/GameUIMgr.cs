using Audio;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameUIMgr : UIWindowBase
{
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
        CloseOtherPanel();
        UIManager.Instance.OpenUI<NetPanel>(Resources.Load<NetPanel>("NetworkPanel"));
    }
    private void OpenBagPanel(PointerEventData eventData)
    {
        CloseOtherPanel();
        UIManager.Instance.OpenUI<BagPanel>(Resources.Load<BagPanel>("BagPanel"));
    }
    private void CloseOtherPanel()
    {
        UIManager.Instance.CloseUI<BagPanel>();
        UIManager.Instance.CloseUI<AudioPanel>();
        UIManager.Instance.CloseUI<NetPanel>();
    }
    public override void OnCreate()
    {
        GetUIEvnetListener("Bag").PointerClick += OpenBagPanel;
        GetUIEvnetListener("Network").PointerClick += OpenNetworkPanel;
    }
    public override void OnUpdate()
    {
        if (UIManager.Instance.IsTopWindow<GameUIMgr>())
        {
            if (UIInput.cancel.Started)
            {
                CharacterBrain.DisableUIInput();
                UIManager.Instance.CloseUI(GetType());
            }
        }
    }

    public override void OnDelete()
    {
        CloseOtherPanel();
    }

    public override void OnFocus()
    {
        
    }

    public override void OnFocusOtherUI()
    {
       
    }
}
