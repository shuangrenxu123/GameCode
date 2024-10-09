
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;
public class PickUpItem : Interactable
{
    [SerializeField]
    struct IDWithCount
    {
        public ItemData data;
        public int count;
    }
    [SerializeField]
    List<IDWithCount> ItemDataConfig;
    public override void Interact(Player playerManager)
    {
        if (ItemDataConfig == null)
        {
            Debug.LogError("物品没有配置相关数据");
            return;
        }
        Debug.Log($"与物体{name}产生了交互");
        foreach (var item in ItemDataConfig)
        {

            
                playerManager.Inventory.AddItem(item.data, item.count);
            
        }
        Destroy(gameObject);
    }
}
