using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    private Dictionary<string, SkillAbility> skills = new Dictionary<string, SkillAbility>();
    public void AddSkill(SkillData data,GameObject prefab)
    {
        skills.Add(data.skillName,new FIreSkill(data,prefab));
    }
    public void GenerateSkill(string name)
    {
        skills[name].CreateExecution();
    }
}
