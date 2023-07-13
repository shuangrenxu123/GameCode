using UnityEngine;
[CreateAssetMenu(menuName = "Items/Armor item")]
public class ArmorItemData : ItemData
{
    [Header("idle Animatinos")]
    public string idle;
    [Header("Defense Animations")]
    public string armorAnimator;
}
