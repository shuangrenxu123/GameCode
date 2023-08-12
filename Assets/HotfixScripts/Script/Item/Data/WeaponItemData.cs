using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/weapon item")]
public class WeaponItemData : ItemData
{
    [Header("Idle Animations")]
    public string TH_Idle;
    public string OH_Idle;
    [Header("Attack Animations")]
    public List<string> OH_light_attack_Animations;
    public List<string> OH_Heavy_attack_Animations;
    public string Run_Attack_1;
    public List<string> TH_light_attack_animations;
    public string OH_Step_attck_animation;
    [Header("Attack Enemy Audio")]
    public List<string> light_attack_audios;
    public string Run_audio_1;
    public string Heavy_audio_1;

    public List<string> attackToEnemy_audios;
}
