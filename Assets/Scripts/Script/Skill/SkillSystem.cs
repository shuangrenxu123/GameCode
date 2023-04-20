using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    private Dictionary<string, SkillData> skills = new Dictionary<string, SkillData>();
    public void GenerateSkill(string name)
    {
        foreach (var i in skills)
        {
            if(i.Key == i.Value.name)
            {
                
            }
        }
    }
}
