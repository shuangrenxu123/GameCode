using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagSlot : MonoBehaviour, IUIElement
{
    [SerializeField]
    private TMP_Text num;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Image bg;
    public ItemData ItemData { get; private set; }
    UIEventListener listen;
    BagPanel bagpanel;
    private void Start()
    {
        listen = gameObject.GetOrAddComponent<UIEventListener>();
        bagpanel = GetComponentInParent<BagPanel>();
        listen.PointerClick += OnClick;
        listen.PointerEnter += OnEnter;
        listen.PointerExit += OnExit;
    }

    private void OnExit(PointerEventData eventData)
    {
        bagpanel.ClearSelectSlot();
        bg.color = Color.white;
    }

    private void OnEnter(PointerEventData data)
    {
        bagpanel.SetSelectSlot(this);
        bg.color = Color.red;
    }

    private void OnClick(PointerEventData eventData)
    {
        bagpanel.OnClick(eventData);   
    }

    public void SetItemData(ItemData item,int num=1)
    {
        ItemData = item;
        icon.sprite = item.Icon;
        icon.gameObject.SetActive(true);
        UpdateNumText(num);
        this.num.gameObject.SetActive(true);
    }
    public void UpdateNumText(int num = 1)
    {
        this.num.text = num.ToString();
    }



    public IUIElement Right { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IUIElement Left { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IUIElement Up { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IUIElement Down { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


}
