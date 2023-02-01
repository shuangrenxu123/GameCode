using BT;
using UnityEngine;
public class MoveAttackAI : BTTree
{
    private static string DESTINATION = "Destination";
    private static string ORC_NAME = "Orc";
    private static string GOBLIN_NAME = "Goblin";

    public float speed;
    public float sightToOrc;
    public float sightGoblin;
    protected override void Init()
    {
        //1.初始化框架
        base.Init();
        //2.创建一个根节点，在这是优先选择器
        root = new BTPrioritySelector();
        //3.创建先决条件
        CheckInSight cheakOrc = new CheckInSight(sightToOrc, ORC_NAME);
        CheckInSight cheakGoblin = new CheckInSight(sightGoblin, GOBLIN_NAME);
        //4.创建Action节点
        BTParallel run = new BTParallel(ParallelFunction.Or);
        run.AddChild(new DoRun(DESTINATION, speed));
        FindEscapeDestination findDes = new FindEscapeDestination(ORC_NAME, DESTINATION, sightToOrc);
        FindToTargetDestination findTarget = new FindToTargetDestination(GOBLIN_NAME, DESTINATION, 1f);

        //构建逃跑节点
        BTParallel escape = new BTParallel(ParallelFunction.Or, cheakOrc);
        escape.AddChild(findDes);
        escape.AddChild(run);
        root.AddChild(escape);

        BTSequence fight = new BTSequence(cheakGoblin);
        {
            BTParallel parallel = new BTParallel(ParallelFunction.Or);
            {
                parallel.AddChild(findTarget);

                parallel.AddChild(run);     // Reuse Run
            }
            fight.AddChild(parallel);
        }
        root.AddChild(fight);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, sightGoblin);
    }
}
