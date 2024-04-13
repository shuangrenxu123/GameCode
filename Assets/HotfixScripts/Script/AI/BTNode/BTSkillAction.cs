using BT;
using UnityEngine;
using UnityEngine.Timeline;

public class BTSkillAction : BTAction
{
    private TimelineAsset skill;
    //private SkillSystem skillSystem;
    private SkillTrigger runner;
    private CharacterActor actor;
    public BTSkillAction(SkillTrigger skill, TimelineAsset skillname, string name ) : base(name)
    {
        this.skill = skillname;
        runner = skill;
        //this.skillSystem = skill;
        
    }

    protected override void Enter()
    { 
        base.Enter();
        runner.LoadConfig(skill);
        actor = database.GetData<CharacterActor>("actor");
        actor.SetUpRootMotion(true,true);
    }

    protected override BTResult Execute()
    {
        runner.OnUpdate();

        if (runner.isFinish == true)
        {
            runner.OnReset();
            return BTResult.Success;
        }
        else
        {
            return BTResult.Running;
        }
    }
    protected override void Exit()
    {
        base.Exit();
        actor.UseRootMotion = false;
    }
}
