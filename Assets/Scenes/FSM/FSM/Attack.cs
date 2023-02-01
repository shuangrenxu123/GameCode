using UnityEngine;

public class Attack : FsmNodeBase, IFsmNode
{
    private float timer;                //�������
    private float time;
    public TestAi target;
    public string name { get { return "Attack"; } }

    public Attack(float timer, FiniteStateMachine fsm, AiData data) : base(fsm, data)
    {
        this.timer = timer;
    }

    public void Enter(object token)
    {
        Debug.Log("�����˹���״̬");
        target = token as TestAi;
        time = float.MaxValue;
    }

    public void Exit()
    {
        Debug.Log("�˳�����״̬");
    }

    public void Update(GameObject go)
    {
        try
        {
            if ((go.transform.position - target.transform.position).sqrMagnitude <= 0.4)
            {
                if (time >= timer)
                {
                    target.kouxue(10, out bool isDead);
                    if (!isDead)
                        time = 0;
                    else
                        fsm.ChangeNode("Move", null);
                }
                else
                {
                    time += Time.deltaTime;
                }
            }
            else
            {
                go.transform.position = Vector3.MoveTowards(go.transform.position, target.transform.position, 4 * Time.deltaTime);
            }
        }
        catch
        {
            fsm.ChangeNode("Move", null);
        }
    }
}
