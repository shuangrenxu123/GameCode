using System;
using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "HumanWeaponAnims", menuName = "AnimationConfig/HumanWeaponAnims")]
public class CharacterWeaponAnimator : ScriptableObject
{
    public SerializedDictionary<WeaponType, WeaponAnimator> animators;
}

[CreateAssetMenu(fileName = "weaponAnimations", menuName = "AnimationConfig/weaponAnim")]
public class WeaponAnimator : ScriptableObject
{
    public WeaponType type;
    public List<TimelineAsset> lightAttackAnimator_OH;
    public List<TimelineAsset> heavyAttackAnimator_OH;
    public List<TimelineAsset> lightAttackAnimator_TH;
    public List<TimelineAsset> heavyAttackAnimator_TH;
    public List<TimelineAsset> RunLightAttackAnimator_OH;
    public List<TimelineAsset> RunLightAttackAnimator_TH;
    public List<TimelineAsset> RunHeavyAttackAnimator_TH;
    public List<TimelineAsset> RunHeavyAttackAnimator_OH;
    public TimelineAsset frontAttack;
    public TimelineAsset BackStab;
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