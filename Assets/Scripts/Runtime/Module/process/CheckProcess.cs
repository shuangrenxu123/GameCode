using UnityEngine;

public class CheckProcess : IProcessNode
{
    public string name { get; set; }

    public void Enter()
    {
        Debug.Log("进入了检查流程");
    }

    public void Exit()
    {
        Debug.Log("退出了检查流程");
    }

    public void Update()
    {
        Debug.Log("检查流程更新");
    }
}
