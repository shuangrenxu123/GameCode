using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    private Enemy enemy;
    public SkillSystem(Enemy enemy)
    {

        this.enemy = enemy;

    }
    private Dictionary<string, SkillAbility> skills = new Dictionary<string, SkillAbility>();
    public void AddSkill(SkillData data,GameObject prefab)
    {
        skills.Add(data.skillName,new SkillAbility(data,prefab));
    }
    public GameObject GenerateSkill(string name)
    {
         var go = skills[name].CreateExecution(enemy.transform.position);
         return go;
    }
}
