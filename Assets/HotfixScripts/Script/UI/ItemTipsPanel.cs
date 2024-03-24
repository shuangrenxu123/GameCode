using System;
using System.Collections;
using System.Collections.Generic;
using UIWindow;
using UnityEngine;

public class ItemTipsPanel : UIWindowBase
{
    Player Player;
    [SerializeField]
    GameObject prefab;
    private void Start()
    {
        Player =FindObjectOfType<Player>();
        Player.Inventory.OnItemAdd += OnPlayerAddItem;
    }

    private void OnPlayerAddItem(ItemData data, int num)
    {
        
    }
}