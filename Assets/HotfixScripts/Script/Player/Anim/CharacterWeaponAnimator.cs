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
    [Serializable]
    public class Clip
    {
        public ClipTransition clip;

        [Header("Evnets")]
        [Tooltip("开启连击与关闭连击的时间，X为开启时间，Y为结束时间")]
        public Vector2 ComboTime;
        public Vector2 HitTime;
    }
    public WeaponType type;
    public List<Clip> lightAttackAnimator_OH;
    public List<Clip> heavyAttackAnimator_OH;
    public List<Clip> lightAttackAnimator_TH;
    public List<Clip> heavyAttackAnimator_TH;
    public List<Clip> RunlightAttackAnimator_OH;
    public List<Clip> RunlightAttackAnimator_TH;
    public List<Clip> RunHeavyAttackAnimator_TH;
    public List<Clip> RunHeavyAttackAnimator_OH;
    public Clip frontAttack;
    public Clip Backstab;
}
public enum WeaponType
{
    None = 0,
    /// <summary>
    /// 直剑
    /// </summary>
    StraightSword = 1,
    /// <summary>
    /// 普通斧
    /// </summary>
    Axe = 2,

}