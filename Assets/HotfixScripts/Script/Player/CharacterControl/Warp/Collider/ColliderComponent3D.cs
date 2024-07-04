using System.Collections.Generic;
using UnityEngine;

public abstract class ColliderComponent3D : ColliderComponent
{

    protected Collider collider = null;
    public RaycastHit[] unfilteredHits { get; protected set; } = new RaycastHit[20];
    public List<RaycastHit> FilteredHits { get; protected set; } = new List<RaycastHit>(10);

    public Collider[] UnfiteredOverlaps { get; protected set; } = new Collider[20];

    public List<Collider> FilteredOverlaps { get; protected set; } = new List<Collider>(10);

    public PhysicsMaterial Material
    {
        get => collider.sharedMaterial;
        set => collider.sharedMaterial = value;
    }
    protected abstract int InternalOverlapBody(Vector3 position, Quaternion rotation, Collider[] unfilteredResults, List<Collider> filtered, OverlapFilterDelegate3D filter);

    public sealed override int OverlapBody(Vector3 position, Quaternion rotation)
    {
        return InternalOverlapBody(position, rotation, UnfiteredOverlaps, FilteredOverlaps, null);
    }
    public override bool ComputePenetration(ref Vector3 position, ref Quaternion rotation, PentrationDelegate Action)
    {
        int overlaps = OverlapBody(position, rotation);
        if (overlaps == 0)
            return false;
        for (int i = 0; i < overlaps; i++)
        {
            var otherCollider = FilteredOverlaps[i];
            if (otherCollider == collider)
                continue;
            if (otherCollider.isTrigger)
                continue;

            var overlapped = Physics.ComputePenetration(
                collider,
                position,
                rotation,
                otherCollider,
                otherCollider.transform.position,
                otherCollider.transform.rotation,
                out Vector3 direction,
                out float distance
                );
            if (!overlapped)
                continue;
            Action?.Invoke(ref position, ref rotation, otherCollider.transform, direction, distance);

        }
        return true;
    }

    protected bool InteralHitFillter(RaycastHit raycastHit)
    {
        if (raycastHit.collider == collider || raycastHit.collider.isTrigger)
            return false;
        return true;
    }

    protected int FilterVaidOverlaps(int hits, Collider[] unfilterOverlaps, List<Collider> filteredOverlaps, OverlapFilterDelegate3D Filter)
    {
        //������е�collider
        filteredOverlaps.Clear();
        for (int i = 0; i < hits; i++)
        {
            Collider collider = unfilterOverlaps[i];

            ///���˵���Щ����Ҫ��
            if (Filter != null)
            {
                bool validHit = Filter(collider);
                if (!validHit)
                    continue;
            }

            filteredOverlaps.Add(collider);
        }
        return filteredOverlaps.Count;
    }

    /// <summary>
    /// �ڲ��ļ����ˣ�Filter  ���ˣ�
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    protected bool InteralOverlapFilter(Collider collider)
    {
        if (collider == this.collider || collider.isTrigger)
            return false;
        return true;
    }
    protected override void Awake()
    {
        base.Awake();
        PhysicsMaterial material = new PhysicsMaterial("Frictionless 3D");
        material.staticFriction = 0f;
        material.dynamicFriction = 0f;
        material.frictionCombine = PhysicsMaterialCombine.Minimum;
        material.bounceCombine = PhysicsMaterialCombine.Minimum;
        material.bounciness = 0f;

        collider.sharedMaterial = material;
        collider.hideFlags = HideFlags.NotEditable;
    }
    protected override void OnEnable()
    {
        collider.enabled = true;
    }
    protected override void OnDisable()
    {
        collider.enabled = false;
    }

}
