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
        [Tooltip("����������ر�������ʱ�䣬XΪ����ʱ�䣬YΪ����ʱ��")]
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
    /// ֱ��
    /// </summary>
    StraightSword = 1,
    /// <summary>
    /// ��ͨ��
    /// </summary>
    Axe = 2,

}