using Audio;
using UIWindow;
using UnityEngine;
using UnityEngine.EventSystems;
using Resources = UnityEngine.Resources;
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
        UIManager.Instance.OpenUI<NetPanel>(UnityEngine.Resources.Load<NetPanel>("NetworkPanel"));
    }
    private void OpenBagPanel(PointerEventData eventData)
    {
        CloseOtherPanel();
        UIManager.Instance.OpenUI<BagPanel>(UnityEngine.Resources.Load<BagPanel>("BagPanel"));
    }
    private void OpenEquipmentUI(PointerEventData eventData)
    {
        CloseOtherPanel();
        UIManager.Instance.OpenUI<EquipmentPanel>(UnityEngine.Resources.Load<EquipmentPanel>("EquipmentPanel"));
    }
    private void CloseOtherPanel()
    {
        while (!UIManager.Instance.IsTopWindow<GameUIMgr>())
        {
            var ui = UIManager.Instance.GetTopWindow();
            UIManager.Instance.CloseUI(ui.GetType());
        }
    }
    public override void OnCreate()
    {
        GetUIEvnetListener("Bag").PointerClick += OpenBagPanel;
        GetUIEvnetListener("Network").PointerClick += OpenNetworkPanel;
        GetUIEvnetListener("Equipment").PointerClick += OpenEquipmentUI;
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
}
