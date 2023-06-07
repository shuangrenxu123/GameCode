using UnityEngine;

public interface IFsmNode
{
    string name { get; }
    void Enter(object token);
    void Update(GameObject go);
    void Exit();
}
