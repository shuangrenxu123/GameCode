using System;
using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Utilities;

[Serializable]
public class NormalAnimationClipDic : UnitySerializedDictionary<string, ClipTransition> { }

[CreateAssetMenu(fileName = "animations", menuName = "Animator")]
public class CCAnimatorConfig : ScriptableObject
{
    [SerializedDictionary("ClipName", "Clip")]
    public SerializedDictionary<string, LinearMixerTransition> linearMixerAnimators;
    [SerializedDictionary("ClipName", "Clip")]
    public SerializedDictionary<string, ClipTransition> clipAnimators;

    [SerializedDictionary("ClipName", "Clip")]
    public SerializedDictionary<string, ClipTransition> climbAnimators;

    public NormalAnimationClipDic injIryAnimators;

    public MixerTransition2D LockMovement;

    public SerializedDictionary<WeaponType, WeaponAttackAnimation> weaponAnimators;
}
