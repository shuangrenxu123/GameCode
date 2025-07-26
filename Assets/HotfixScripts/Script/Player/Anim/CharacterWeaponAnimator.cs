using System;
using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "HumanWeaponAnims", menuName = "AnimationConfig/HumanWeaponAnims")]
public class CharacterWeaponAnimator : ScriptableObject
{
    public SerializedDictionary<WeaponType, WeaponAttackAnimationData> animators;
}

public enum WeaponType
{
    /// <summary>
    /// 拳套
    /// </summary>
    Gloves = 0,
    /// <summary>
    /// 直剑
    /// </summary>
    StraightSword = 1,
    /// <summary>
    /// 斧子
    /// </summary>
    Axe = 2,

}