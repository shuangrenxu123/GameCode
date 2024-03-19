using SKUnityToolkit.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using VInspector;
public class PickUpItem : Interactable
{
    [SerializeField]
    List<SerializableDictionary<ItemData, int>> ItemDataConfig;
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
            foreach (var key in item)
            {
                playerManager.Inventory.AddItem(key.Key, key.Value);
            }
        }
        Destroy(gameObject);
    }
}
