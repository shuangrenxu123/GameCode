using System;
using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Utilities;

[Serializable]
public class NormalAnimationClipDic : UnitySerializedDictionary<string, ClipTransition> { }
[Serializable]
public class LinearAnimationClipDic : UnitySerializedDictionary<string, LinearMixerTransition> { }

[CreateAssetMenu(fileName = "animations", menuName = "Animator")]
public class CCAnimatorConfig : ScriptableObject
{
    [SerializedDictionary("ClipName", "Clip")]
    public LinearAnimationClipDic linearMixerAnimators;
    [SerializedDictionary("ClipName", "Clip")]
    public SerializedDictionary<string, ClipTransition> clipAnimators;

    [SerializedDictionary("ClipName", "Clip")]
    public SerializedDictionary<string, ClipTransition> climbAnimators;

    public NormalAnimationClipDic injIryAnimators;

    public NormalAnimationClipDic DeathAnimators;

    public NormalAnimationClipDic DeathLoopAnimators;

    public MixerTransition2D LockMovement;


    public SerializedDictionary<WeaponType, WeaponAttackAnimation> weaponAnimators;
}
