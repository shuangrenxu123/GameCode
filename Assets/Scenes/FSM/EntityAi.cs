using System.Collections.Generic;

public class EntityAi : FiniteStateMachine
{
    HealthPoint hp;
    //Animator anim;
    public override void Init()
    {
        //anim = GetComponent<Animator>();
        AiData data = GetComponent<AiData>();
        AddNode(new Move(this, data), new List<string> { "Idle", "Attack" });
        AddNode(new Idle(this, data), new List<string> { "Move", "Attack" });
        //AddNode(new Attack(0.5f,this, data), new List<string> { "Idle", "Move" });
        currNode = GetNode("Move");
        currNode.Enter(this.gameObject.transform);

        data.CombatNumberBox.Speed.SetBase(1);
    }
}
