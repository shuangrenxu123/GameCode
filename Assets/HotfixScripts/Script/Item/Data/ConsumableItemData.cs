using UnityEngine;
[CreateAssetMenu(menuName = "Items/consumableItem")]
// 消耗品
public class ConsumableItemData : ItemData
{
    [Header("相关信息")]
    public int MaxItemAmount;
    [Header("Animtions")]
    public string consumeAnimation;
    public string UsageFailedAnimation;
    public bool isInteracting;
    public GameObject modelPrefab;
}
