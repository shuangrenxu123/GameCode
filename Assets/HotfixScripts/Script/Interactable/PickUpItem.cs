using UnityEngine;

public class PickUpItem : Interactable
{
    [SerializeField]
    ItemData ItemDataConfig;
    public override void Interact(Player playerManager)
    {
        Debug.Log($"与物体{name}产生了交互");
        //todo 将物品添加到背包中
        Destroy(gameObject);
    }
}
