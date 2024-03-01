using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagPanel : UIWindow
{
    Dictionary<int, BagSlot> slots;
    int endSoltIndex = 0;
    GameObject slotParent;
    BagSlot currentSelectSlot;

    [SerializeField]
    TMP_Text ItemName;

    [SerializeField]
    TMP_Text ItemDescription;
    [SerializeField]
    GameObject selectPanel;
    private void Awake()
    {
        slots = new(30);
    }
    public  void Start()
    {
        //WindowsManager.Instance.DisableWindow<BagPanel>();
        slotParent = GetUIGameObject("slot");
    }
    public void AddItem(ItemData item, int num = 1)
    {
        if (slots.ContainsKey(item.id))
        {
            slots[item.id].UpdateNumText(num);
        }
        else
        {
            slots.Add(item.id, slotParent.transform.GetChild(endSoltIndex).GetComponent<BagSlot>());
            var slot = slots[item.id];
            slot.SetItemData(item,num);
            endSoltIndex += 1;
        }
    }
    public void OnClick(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerEnter.name);
        if (currentSelectSlot != null && currentSelectSlot.ItemData != null)
        {
            ItemName.text = currentSelectSlot.ItemData.Name;
            ItemDescription.text = currentSelectSlot.ItemData.Description;
            var temp = Instantiate(selectPanel,currentSelectSlot.transform);
            temp.transform.parent = transform;
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
