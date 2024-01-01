using UnityEngine;

/// <summary>
/// ��������actor�ƶ�ʱ��λ�Ʋ�ֵ
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
        // ��ָ���������κ�ָ��֮�����У���ȷ����������ʼ�����ֵ���ݣ�physicsActor����ͬ������
        physicsActor.SyncBody();
    }
}
