using System;
using UnityEngine;

/// <summary>
/// �����ӳ�ͻ��Ϣ����Enter ���͡�ͣ�������ռ�����ϵ����Ϣ�Ľṹ��
/// IEquatable�����жϣ������Ա���һЩװ�����������
/// </summary>
public struct Trigger : IEquatable<Trigger>, IEquatable<Collider>
{
    public bool firstContact;
    public Collider collider3D;
    public GameObject gameObject;
    public Transform transform;

    float fixedTime;
    public Trigger(Collider collider, float fixedTime) : this()
    {
        collider3D = collider;
        this.fixedTime = fixedTime;
        firstContact = true;
        gameObject = collider.gameObject;
        transform = collider.transform;
    }

    public override int GetHashCode()
    {
        return gameObject.GetHashCode();
    }
    public bool Equals(Trigger other)
    {
        return gameObject == other.gameObject;
    }

    public bool Equals(Collider other)
    {
        if (other == null) return false;
        return collider3D == other;
    }

    public static bool operator ==(Trigger a, Collider b) => a.Equals(b);
    public static bool operator !=(Trigger a, Collider b) => !a.Equals(b);

    public static bool operator ==(Trigger a, Collider2D b) => a.Equals(b);
    public static bool operator !=(Trigger a, Collider2D b) => !a.Equals(b);

    public static bool operator ==(Trigger a, Trigger b) => a.Equals(b);
    public static bool operator !=(Trigger a, Trigger b) => !a.Equals(b);
}
