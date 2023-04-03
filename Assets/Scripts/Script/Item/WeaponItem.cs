using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/weapon item")]
public class WeaponItem : Item
{
    [Header("Attack Animations")]
    public string Light_Attack_1;
    public string Light_Attack_2;
    public string Run_Attack_1;

    public string Heavy_Attack_1;
}
