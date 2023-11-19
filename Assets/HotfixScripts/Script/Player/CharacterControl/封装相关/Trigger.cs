using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 包含从冲突消息（“Enter ”和“停留”）收集的联系人信息的结构。
/// IEquatable用于判断，它可以避免一些装箱与拆箱问题
/// </summary>
public struct Trigger : IEquatable<Trigger>, IEquatable<Collider>, IEquatable<Collider2D>
{
    public bool firstContact;

    public Collider2D collider2D;
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
    public Trigger(Collider2D collider, float fixedTime) : this()
    {
        collider2D = collider;
        this.fixedTime = fixedTime;
        firstContact = true;
        gameObject = collider.gameObject;
        transform = collider.transform;
    }
    public override int GetHashCode()
    {
        return gameObject.GetHashCode();
    }
    public void Set(bool firstContact,Collider2D collider)
    {
        if (firstContact)
            fixedTime = Time.fixedTime;
        this.firstContact = firstContact;
        collider2D = collider;
        gameObject= collider.gameObject;
        transform = collider.transform;
    }
    public bool Equals(Trigger other)
    {
        return gameObject== other.gameObject;
    }

    public bool Equals(Collider other)
    {
        if (other == null) return false;
        return collider3D == other;
    }

    public bool Equals(Collider2D other)
    {
        if(other == null) return false;
        return collider2D == other;
    }

    public static bool operator ==(Trigger a, Collider b) => a.Equals(b);
    public static bool operator !=(Trigger a, Collider b) => !a.Equals(b);

    public static bool operator ==(Trigger a, Collider2D b) => a.Equals(b);
    public static bool operator !=(Trigger a, Collider2D b) => !a.Equals(b);

    public static bool operator ==(Trigger a, Trigger b) => a.Equals(b);
    public static bool operator !=(Trigger a, Trigger b) => !a.Equals(b);
}
