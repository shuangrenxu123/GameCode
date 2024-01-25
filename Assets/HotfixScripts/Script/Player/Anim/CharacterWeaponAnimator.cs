using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponAniamtor", menuName = "AniamtorConfig/WeaponAniamtor")]
public class CharacterWeaponAnimator : ScriptableObject
{
    public List<WeaponAnimator> animators;
}

[Serializable]
public class WeaponAnimator
{
    [SerializeField]
    WeaponType type;
    [SerializeField]
    public List<ClipTransition> lightAttackAnimator_OH;
    [SerializeField]
    public List<ClipTransition> heavyAttackAnimator_OH;
    [SerializeField]
    public List<ClipTransition> lightAttackAnimator_TH;
    [SerializeField]
    public List<ClipTransition> heavyAttackAnimator_TH;
    #region Events
    [Header("Evnets")]
    [Tooltip("开启连击与关闭连击的时间，X为开启时间，Y为结束时间")]
    Vector2 ComboTime;
    Vector2 HitTime;
    #endregion
}
enum WeaponType
{
    /// <summary>
    /// 直剑
    /// </summary>
    StraightSword,
    /// <summary>
    /// 普通斧
    /// </summary>
    Axe,

}