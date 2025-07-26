using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// 派生的武器攻击动画配置表
/// </summary>
[CreateAssetMenu(fileName = "weaponAnimationClip", menuName = "AnimationConfig/weaponAnimClip")]
public class WeaponAttackAnimation : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField]
    List<AnimationClip> anim;
#endif
    public SerializedDictionary<string, AnimationClip>
        attackAnimations;
    public WeaponType type;
    void OnValidate()
    {
        attackAnimations.Clear();

        foreach (var a in anim)
        {
            attackAnimations.Add(a.name, a);
        }
    }
}
