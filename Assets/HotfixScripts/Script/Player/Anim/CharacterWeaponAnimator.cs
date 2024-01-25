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
    [Tooltip("����������ر�������ʱ�䣬XΪ����ʱ�䣬YΪ����ʱ��")]
    Vector2 ComboTime;
    Vector2 HitTime;
    #endregion
}
enum WeaponType
{
    /// <summary>
    /// ֱ��
    /// </summary>
    StraightSword,
    /// <summary>
    /// ��ͨ��
    /// </summary>
    Axe,

}