using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : Interactable
{
    WeaponItemData weapon;
    public override void Interact(Player playerManager)
    {
        //Debug.Log($"与物体{weapon.Name}产生了交互");
        Destroy(gameObject);
    }
}
