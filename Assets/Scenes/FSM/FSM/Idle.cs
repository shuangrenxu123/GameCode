using UnityEngine;

public class Idle : FsmNodeBase, IFsmNode
{
    public string name { get { return "Idle"; } }
    public float duration;
    public float timer = 0;
    Transform obj;
    public Idle(FiniteStateMachine fsm,  AiData data) : base(fsm, data)
    {
    }

    public void Enter(object token)
    {
        duration = Random.Range(1, 5);
        timer = 0;
        obj = token as Transform;
        Debug.Log("进入了状态:Idle");
    }

    public void Exit()
    {
        Debug.Log("退出了状态 Idle");
    }

    public void Update(GameObject go)
    {
        if (timer < duration)
        {
            timer += Time.deltaTime;
        }
        else
        {
            go.GetComponent<FiniteStateMachine>().ChangeNode("Move", obj);
        }
    }
}
