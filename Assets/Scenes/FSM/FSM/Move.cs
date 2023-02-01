using UnityEngine;

public class Move : FsmNodeBase, IFsmNode
{
    Transform obj;
    public ContextSolver solver;
    Vector2 dircetion = Vector2.zero;
    public Move(FiniteStateMachine fsm, AiData data) : base(fsm, data)
    {
        solver = new ContextSolver(data);
    }

    public string name
    {
        get { return "Move"; }
    }


    public void Enter(object token)
    {
        if (obj == null)
            obj = token as Transform;
        aiData.reachedLastTarget = true;
        Debug.Log("进入了 walk");
    }

    public void Exit()
    {
        //Nextpos = Vector2.zero;
        Debug.Log("退出了 walk");
    }

    public void Update(GameObject go)
    {
        dircetion = solver.GetDirectionToMove();
        Debug.DrawLine(aiData.me.position, (Vector2)aiData.me.position + dircetion, Color.cyan);
        if (dircetion.x < 0)
        {
            obj.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            obj.localScale = new Vector3(-1, 1, 1);
        }

        if ((go.transform.position - aiData.currentTarget.position).sqrMagnitude < 0.04)
        {
            if (aiData.enemy != null)
            {
                Debug.Log("进入战斗状态");
                fsm.ChangeNode("Attack", aiData.enemy.GetComponent<TestAi>());
            }
            else
            {
                Debug.Log("进入站立状态");
                fsm.ChangeNode("Idle", obj);
            }
        }
        else
        {
            go.transform.position +=aiData.CombatNumberBox.Speed.Value * Time.deltaTime * (Vector3)dircetion;
        }
    }
}
