using Assets;
using Audio;
using UIWindow;
using UnityEngine;
using UnityEngine.EventSystems;
public class GameUIMgr : UIWindowBase
{
    [SerializeField]
    public AudioData audios;
    #region AudioName
    [Header("UI��Ч")]
    [SerializeField]
    private string click;
    #endregion
    //---------------------------------------------------------------------
    private void OpenNetworkPanel(PointerEventData eventData)
    {
        CloseOtherPanel();
        var ui = ResourcesManager.Instance.LoadAsset<GameObject>("ui", "NetworkPanel.prefab");
    }
    private void OpenBagPanel(PointerEventData eventData)
    {
        CloseOtherPanel();
        var ui = ResourcesManager.Instance.LoadAsset<GameObject>("ui", "BagPanel.prefab");
    }
    private void OpenEquipmentUI(PointerEventData eventData)
    {
        CloseOtherPanel();
        var ui = ResourcesManager.Instance.LoadAsset<GameObject>("ui", "EquipmentPanel.prefab");
    }
    private void CloseOtherPanel()
    {
        while (!UIManager.Instance.IsTopWindow<GameUIMgr>())
        {
            var ui = UIManager.Instance.GetTopWindow();
            UIManager.Instance.CloseUI(ui.GetType());
        }
    }
    public override void OnUpdate()
    {
        if (UIManager.Instance.IsTopWindow<GameUIMgr>())
        {
            // if (UIInput.cancel.Started)
            // {
            //     CharacterBrain.DisableUIInput();
            //     UIManager.Instance.CloseUI(GetType());
            // }
        }
    }
}
