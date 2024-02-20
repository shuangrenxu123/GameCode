using UnityEngine;

public class PickUpItem : Interactable
{
    [SerializeField]
    ItemData ItemDataConfig;
    public override void Interact(Player playerManager)
    {
        if (ItemDataConfig == null)
        {
            Debug.LogError("物品没有配置相关数据");
            return;
        }
        Debug.Log($"与物体{name}产生了交互");
        playerManager.Inventory.AddItem(ItemDataConfig);
        Destroy(gameObject);
    }
}
