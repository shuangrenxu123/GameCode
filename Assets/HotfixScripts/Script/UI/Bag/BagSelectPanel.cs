using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BagSelectPanel : UIWindowBase
{
    private ItemData currentItem;
    [SerializeField] private TMP_Text Equipment;
    [SerializeField] private TMP_Text use;
    [SerializeField] private TMP_Text drop;
    public Transform panel;
    private PlayerInventory inventory;
    public override void OnCreate()
    {
        base.OnCreate();
        inventory = FindObjectOfType<Player>().Inventory;
        var panel = UIManager.Instance.GetUIWindow<BagPanel>();
        currentItem = panel.currentSelectSlot.ItemData;

        GetUIEvnetListener("Equipment").PointerClick += EquipmentClick;
        GetUIEvnetListener("Use").PointerClick += UseClick;
        GetUIEvnetListener("Drop").PointerClick += DropClick;

        switch (currentItem.Type)
        {
            case ItemType.Consumable:
                Equipment.gameObject.SetActive(false);
                break;
            case ItemType.Equip:
            case ItemType.Weapon:
                use.gameObject.SetActive(false);
                break;
            case ItemType.other:
                use.gameObject.SetActive(false);
                Equipment.gameObject.SetActive(false);
                break;
        }

    }


    public void SetPanel(Vector3 position)
    {
        panel.position = position;
    }
    private void DropClick(PointerEventData eventdata)
    {
        UIManager.Instance.CloseUI<BagSelectPanel>();
    }

    private void UseClick(PointerEventData eventdata)
    {
        inventory.UseItem(currentItem as ConsumableItemData);
        UIManager.Instance.CloseUI<BagSelectPanel>();
        UIManager.Instance.CloseUI<BagPanel>();
        UIManager.Instance.CloseUI<GameUIMgr>();
        CharacterBrain.DisableUIInput();
    }

    private void EquipmentClick(PointerEventData eventdata)
    {
        var equipe= currentItem as EquipItemData;
        equipe.Equip(inventory);

        UIManager.Instance.CloseUI<BagSelectPanel>();
    }

    public override void OnUpdate()
    {
        if (UIManager.Instance.IsTopWindow<BagSelectPanel>())
        {
            if (UIInput.cancel.Started)
            {
                UIManager.Instance.CloseUI(GetType());
            }
        }
    }
}
