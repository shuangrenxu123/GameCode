using Character.Player;
using UIWindow;
using UnityEngine;

public class ItemTipsPanel : UIWindowBase
{
    Player Player;
    [SerializeField]
    GameObject prefab;
    private void Start()
    {
        Player = Player.Instance;
        Player.Inventory.OnItemAdd += OnPlayerAddItem;
    }

    private void OnPlayerAddItem(ItemData data, int num)
    {

    }
}