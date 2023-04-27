using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FIreSkill : SkillAbility
{
    public FIreSkill(SkillData data,GameObject prefab):base(data,prefab)
    {
        
    }

    public override void CreateExecution()
    {
        GameObject.Instantiate(SkillPrefab);
    }
}
