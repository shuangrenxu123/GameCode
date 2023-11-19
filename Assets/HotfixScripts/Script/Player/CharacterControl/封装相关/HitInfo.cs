using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct HitInfo
{
    public readonly Vector3 normal;

    public readonly Vector3 point;

    public readonly Vector3 direction;

    public readonly float distance;

    public readonly bool hit;

    public readonly Transform transform;

    public readonly Collider2D collider2D;

    public readonly Collider collider3D;

    public readonly Rigidbody2D rigidbody2D;

    public readonly Rigidbody rigidbody3D;

    public readonly int layer;

    public HitInfo(ref RaycastHit raycastHit, Vector3 castDirection) : this()
    {
        if (raycastHit.collider == null)
            return;

        hit = true;
        point = raycastHit.point;
        normal = raycastHit.normal;
        distance = raycastHit.distance;
        direction = castDirection;

        collider3D = raycastHit.collider;

        rigidbody3D = collider3D.attachedRigidbody;
        transform = collider3D.transform;
        layer = transform.gameObject.layer;
    }

    public HitInfo(ref RaycastHit2D raycastHit, Vector3 castDirection) : this()
    {
        if (raycastHit.collider == null)
            return;

        hit = true;
        point = raycastHit.point;
        normal = raycastHit.normal;
        distance = raycastHit.distance;
        direction = castDirection;

        collider2D = raycastHit.collider;

        rigidbody2D = collider2D.attachedRigidbody;
        transform = collider2D.transform;
        layer = transform.gameObject.layer;
    }
    public bool Is2D => collider2D != null;
    public bool IsRigidbody => rigidbody2D != null || rigidbody3D != null;

    public bool IsKinematicRigidbody
    {
        get
        {
            if (rigidbody2D != null)
                return rigidbody2D.isKinematic;
            else if (rigidbody3D != null)
                return rigidbody3D.isKinematic;

            return false;
        }
    }

    public bool IsDynamicRigidbody
    {
        get
        {
            if (rigidbody2D != null)
                return rigidbody2D.bodyType == RigidbodyType2D.Dynamic;
            else if (rigidbody3D != null)
                return !rigidbody3D.isKinematic;

            return false;
        }
    }
}
public readonly struct HitInfoFilter
{
    public readonly LayerMask collisionLayerMask;
    public readonly bool ignoreRigidbodies;
    public readonly bool ignoreTriggers;
    public readonly LayerMask ignorePhysicsLayerMask;
    public readonly float minimumDistance;
    public readonly float maximumDistance;

    public HitInfoFilter(LayerMask collisionLayerMask, bool ignoreRigidbodies, bool ignoreTriggers, int ignoredLayerMask = 0, float minimumDistance = 0f, float maximumDistance = Mathf.Infinity)
    {
        this.collisionLayerMask = collisionLayerMask;
        this.ignoreRigidbodies = ignoreRigidbodies;
        this.ignoreTriggers = ignoreTriggers;
        this.ignorePhysicsLayerMask = ignoredLayerMask;
        this.minimumDistance = minimumDistance;
        this.maximumDistance = maximumDistance;
    }



}
