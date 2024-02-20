using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagSlot : MonoBehaviour, IUIElement
{
    [SerializeField]
    private TMP_Text num;
    [SerializeField]
    private Image icon;
    public ItemData ItemData { get; private set; }
    public void SetItemData(ItemData item)
    {
        ItemData = item;
        icon.sprite = item.Icon;
        icon.gameObject.SetActive(true);
        UpdateNumText();
        num.gameObject.SetActive(true);
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
