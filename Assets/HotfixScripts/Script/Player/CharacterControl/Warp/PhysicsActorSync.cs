using UnityEngine;

/// <summary>
/// 该类用于actor移动时的位移插值
/// </summary>
[RequireComponent(typeof(PhysicsActor))]
[DefaultExecutionOrder(int.MaxValue)]
public class PhysicsActorSync : MonoBehaviour
{
    PhysicsActor physicsActor = null;
    private void Awake()
    {
        physicsActor = GetComponent<PhysicsActor>();
    }
    private void FixedUpdate()
    {
        if (!physicsActor.enabled)
            return;
        // 此指令在其他任何指令之后运行，并确保刚体数据始终与插值数据（physicsActor）“同步”。
        physicsActor.SyncBody();
    }
}
