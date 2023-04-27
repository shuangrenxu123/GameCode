using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSkillAction : BTAction
{
    private string skillname;
    private SkillSystem skillSystem;
    public BTSkillAction(SkillSystem skill,string skillname,string name) : base(name)
    {
        this.skillname = skillname;
        this.skillSystem = skill;
    }
    protected override BTResult Execute()
    {
        skillSystem.GenerateSkill(skillname);
        return BTResult.Success;
    }
}
