using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereColliderComponent3D : ColliderComponent3D
{
    public override Vector3 Size { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public override Vector3 Offset { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public override Vector3 BoundsSize => throw new System.NotImplementedException();

    protected override int InternalOverlapBody(Vector3 position, Quaternion rotation, Collider[] unfilteredResults, List<Collider> filtered, OverlapFilterDelegate3D filter)
    {
        throw new System.NotImplementedException();
    }
}
