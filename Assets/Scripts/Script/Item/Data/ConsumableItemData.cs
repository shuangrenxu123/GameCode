using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="Items/consumableItem")]
public class ConsumableItemData : ItemData
{
    [Header("相关信息")]
    public int MaxItemAmount;
    [Header("Animtions")]
    public string consumeAnimation;
    public string UsageFailedAnimation;
    public bool isInteracting;
}
