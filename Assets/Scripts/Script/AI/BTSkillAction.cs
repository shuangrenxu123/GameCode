using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSkillAction : BTAction
{
    private string skillname;
    private SkillSystem skillSystem;
    private SkillTrigger runner;
    public float SkillCD;
    public BTSkillAction(SkillSystem skill,string skillname, string name, float skillCD) : base(name)
    {
        this.skillname = skillname;
        this.skillSystem = skill;
        SkillCD = skillCD;
    }
    protected override BTResult Execute()
    {
        var time = database.GetData<float>(skillname);
        if(time < SkillCD)
        {
            return BTResult.Failed;
        }
        if (runner == null)
        {
            runner = skillSystem.GenerateSkill(skillname).GetComponent<SkillTrigger>();
        }
        if(runner.isFinish == true)
        {
            database.SetData<float>(skillname,0f);
            GameObject.Destroy(runner.gameObject);
            return BTResult.Success;
        }
        else
        {
            return BTResult.Running;
        }   
    }
    public override void Clear()
    {
        base.Clear();
        runner = null;
    }
}
