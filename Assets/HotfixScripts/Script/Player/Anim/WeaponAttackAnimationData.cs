using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// 派生的武器攻击动画配置表
/// </summary>
[CreateAssetMenu(fileName = "weaponAnimations", menuName = "AnimationConfig/weaponAnim")]
public class WeaponAttackAnimationData : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField]
    List<TimelineAsset> anim;
#endif
    public SerializedDictionary<string, SingleAttackActionAnimationConfig>
        attackAnimations;
    public WeaponType type;
    void OnValidate()
    {
        foreach (var a in anim)
        {
            if (!attackAnimations.ContainsKey(a.name))
                attackAnimations.Add(a.name, new(a));
        }
    }
}

/// <summary>
/// 单条攻击动画的配置
/// </summary>
[System.Serializable]
public class SingleAttackActionAnimationConfig
{
    public bool isLoop;
    public float inputTime;
    public TimelineAsset timelineAsset;
    public SingleAttackActionAnimationConfig(TimelineAsset timelineAsset)
    {
        this.timelineAsset = timelineAsset;
    }
}
