using UnityEngine.UI;

public class PlayerInventoryPanel : WindowRoot
{

    Image left;
    Image right;
    Image Prop;

    private void Awake()
    {
        left = GetUIGameObject("leftWeapon").transform.GetChild(0).GetComponent<Image>();
        right = GetUIGameObject("rightWeapon").transform.GetChild(0).GetComponent<Image>();
        Prop = GetUIGameObject("Prop").transform.GetChild(0).GetComponent<Image>();
        Init();
    }
    public override void Start()
    {
    }

    public override void Update()
    {

    }
    void Init()
    {
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
        Prop.gameObject.SetActive(false);
    }
    public void UpdateRightWeapon(ItemData data)
    {
        if (data == null)
        {
            right.gameObject.SetActive(false);
        }
        else
        {
            right.gameObject.SetActive(true);
            right.sprite = data.Icon;
        }
    }
    public void UpdateLeftWeapon(ItemData data)
    {
        if (data == null)
        {
            left.gameObject.SetActive(false);
        }
        else
        {
            left.gameObject.SetActive(true);
            left.sprite = data.Icon;
        }
    }
    public void UpdateProp(ConsumableItem item)
    {
        if (item == null)
        {
            Prop.gameObject.SetActive(false);
        }
        else
        {
            Prop.sprite = item.data.Icon;
            Prop.gameObject.SetActive(true);
        }
    }
}
