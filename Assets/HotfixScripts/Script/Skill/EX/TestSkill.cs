using UnityEngine;

public class TestSkill : SkillTrigger
{
    public override void Start()
    {
        base.Start();
        //runner.LoadConfig("test", this);
    }
    public override void OnStart()
    {
        Debug.Log("test");
    }
}
