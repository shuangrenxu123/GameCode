using System.Collections.Generic;
using TMPro;
using UIWindow;
using UnityEngine;
using UnityEngine.EventSystems;

public class BagPanel : UIWindowBase, IUIWindow
{
    PlayerInventory inventory;
    List<BagSlot> slots;
    GameObject slotParent;
    public BagSlot currentSelectSlot { get; private set; }

    [SerializeField]
    TMP_Text ItemName;

    [SerializeField]
    TMP_Text ItemDescription;
    [SerializeField]
    BagSelectPanel selectPanel;
    void UpdateBagSlot()
    {
        int index = 0;
    }
    public void OnClick(PointerEventData eventData)
    {
        if (currentSelectSlot != null && currentSelectSlot.ItemData != null)
        {
            ItemName.text = currentSelectSlot.ItemData.Name;
            ItemDescription.text = currentSelectSlot.ItemData.Description;
            //var temp = Instantiate(selectPanel,currentSelectSlot.transform);

            var temp = UIManager.Instance.OpenUI<BagSelectPanel>(selectPanel);
            if (temp != null)
            {
                temp.OnDeleteEvent += () =>
                {
                    raycaster.interactable = true;
                    raycaster.blocksRaycasts = true;
                };
                temp.SetPanel(currentSelectSlot.transform.position);
                raycaster.interactable = false;
                raycaster.blocksRaycasts = false;
            }

        }
    }
    public void SetSelectSlot(BagSlot slot)
    {
        currentSelectSlot = slot;
    }
    public void ClearSelectSlot()
    {
        currentSelectSlot = null;
    }

    public override void OnCreate()
    {
        slotParent = GetUIGameObject("slot");
        slots = new(30);
        for (var i = 0; i < slotParent.transform.childCount; i++)
        {
            slots.Add(slotParent.transform.GetChild(i).GetComponent<BagSlot>());
        }
        var player = Player.Instance;
        inventory = player.Inventory;
        UpdateBagSlot();

    }

    public override void OnUpdate()
    {
        if (UIManager.Instance.IsTopWindow<BagPanel>())
        {
            if (UIInput.cancel.Started)
            {
                UIManager.Instance.CloseUI(GetType());
            }
        }
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
