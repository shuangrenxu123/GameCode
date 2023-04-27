using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkillAbility
{
    public GameObject SkillPrefab;
    public SkillData data;

    public SkillAbility(SkillData data,GameObject p)
    {
        SkillPrefab = p;
        this.data = data;
    }
    public virtual void CreateExecution() { }

}
