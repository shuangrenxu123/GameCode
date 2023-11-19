using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 该类是对物体接触点的一个封装
/// </summary>
public readonly struct Contact
{
    public readonly bool firstContact;
    public readonly Vector3 point;
    public readonly Vector3 normal;
    public readonly Collider2D collider2D;
    public readonly Collider collider3D;
    public readonly bool isRigidbody;
    public readonly bool isKinematicRigidbody;
    public readonly Vector3 relativeVelocity;
    public readonly Vector3 pointVelocity;
    public readonly GameObject gameObject;

    public Contact(bool firstContact,ContactPoint2D contact,Collision2D collision) :this()
    {
        this.firstContact = firstContact;
        this.point = contact.point;
        this.normal = contact.normal;
        this   .gameObject= collider2D.gameObject;
        this.collider2D = contact.collider;
        var contactRigidbody = this.collider2D.attachedRigidbody;
        this.relativeVelocity = collision.relativeVelocity;


        if (this.isRigidbody = contactRigidbody != null)
        {
            this.isKinematicRigidbody = contactRigidbody.isKinematic;
            this.pointVelocity = contactRigidbody.GetPointVelocity(this.point);
        }
    }
    public Contact(bool firstContact, ContactPoint contact, Collision collision) : this()
    {
        this.firstContact = firstContact;
        this.collider3D = contact.otherCollider;
        this.point = contact.point;
        this.normal = contact.normal;
        this.gameObject = this.collider3D.gameObject;

        var contactRigidbody = this.collider3D.attachedRigidbody;

        this.relativeVelocity = collision.relativeVelocity;

        if (this.isRigidbody = contactRigidbody != null)
        {
            this.isKinematicRigidbody = contactRigidbody.isKinematic;
            this.pointVelocity = contactRigidbody.GetPointVelocity(this.point);
        }
    }

    public Contact(Vector3 point, Vector3 normal, Vector3 pointVelocity, Vector3 relativeVelocity) : this()
    {
        this.point = point;
        this.normal = normal;
        this.pointVelocity = pointVelocity;
        this.relativeVelocity = relativeVelocity;
    }
}
