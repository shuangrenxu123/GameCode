using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName =("Skill/Data"))]
public class SkillData :ScriptableObject
{
    public int skillId;
    public string skillName;
    public string skillDescription;
    public SkillType Type;
    public SkillAffectTargetType AffectTargetType;
    public SkillTargetSelectType TargetSelectType;
    public GameObject SkillPrefab;
}
