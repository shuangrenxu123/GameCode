using UnityEngine;

public class CheckProcess : IProcessNode
{
    public string name { get; set; }

    public void Enter()
    {
        Debug.Log("�����˼������");
    }

    public void Exit()
    {
        Debug.Log("�˳��˼������");
    }

    public void Update()
    {
        Debug.Log("������̸���");
    }
}
