using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagPanel : WindowRoot
{
    Dictionary<int,BagSlot> slots;
    int endSoltIndex = 0;
    GameObject slotParent;
    private void Awake()
    {
        slots = new(30);
    }
    public override void Start()
    {
        WindowsManager.Instance.DisableWindow<BagPanel>();
        slotParent = GetUIGameObject("slot");
        int childCount = slotParent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var go = GetUIEvnetListener(slotParent.transform.GetChild(i).name);
            go.PointerEnter += OnPointEnter;
            go.PointerExit += OnPointExit;
            go.PointerClick += OnPointClick;
        }
    }
    public void AddItem(ItemData item,int num = 1)
    {
        if (slots.ContainsKey(item.id))
        {
            slots[item.id].num.text = num.ToString();
        }
        else
        {
            slots.Add(item.id, slotParent.transform.GetChild(endSoltIndex).GetComponent<BagSlot>());
            var slot = slots[item.id];
            slot.icon.sprite = item.Icon;
            slot.icon.gameObject.SetActive(true);
            slot.num.text = num.ToString();
            slot.num.gameObject.SetActive(true);
            endSoltIndex += 1;
        }
    }
    private void OnPointClick(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerEnter.name);
    }

    private void OnPointExit(PointerEventData eventData)
    {
        eventData.pointerEnter.GetComponent<Image>().color = Color.white;
    }

    private void OnPointEnter(PointerEventData eventData)
    {
        eventData.pointerEnter.GetComponent<Image>().color = Color.red;
    }
    public override void Update()
    {

    }

}
