using UnityEngine;

/// <summary>
/// 该类是对Collider的一个封装
/// </summary>
public abstract class ColliderComponent : MonoBehaviour
{
    /// <summary>
    /// collider 的大小
    /// </summary>
    public abstract Vector3 Size { get; set; }

    public abstract Vector3 Offset { get; set; }

    public abstract Vector3 BoundsSize { get; }

    public Vector3 Center => transform.position + transform.TransformVectorUnscaled(Offset);

    public static ColliderComponent CreateInstance(GameObject gameObject, bool includeChildren = true)
    {
        Collider collider3D = includeChildren ? gameObject.GetComponentInChildren<Collider>() : gameObject.GetComponent<Collider>();
        if (collider3D != null)
        {
            // Box collider ------------------------------------------------------------
            BoxCollider boxCollider3D = null;

            try
            {
                boxCollider3D = (BoxCollider)collider3D;
            }
            catch (System.Exception) { }

            if (boxCollider3D != null)
                return gameObject.AddComponent<BoxColliderComponent3D>();


            // Circle collider ------------------------------------------------------------
            SphereCollider sphereCollider3D = null;

            try
            {
                sphereCollider3D = (SphereCollider)collider3D;
            }
            catch (System.Exception) { }

            if (sphereCollider3D != null)
                return gameObject.AddComponent<SphereColliderComponent3D>();

            // Capsule collider ------------------------------------------------------------
            CapsuleCollider capsuleCollider3D = null;

            try
            {
                capsuleCollider3D = (CapsuleCollider)collider3D;
            }
            catch (System.Exception) { }

            if (capsuleCollider3D != null)
                return gameObject.AddComponent<CapsuleColliderComponent3D>();
        }


        return null;
    }

    public delegate void PentrationDelegate(ref Vector3 bodyPosition, ref Quaternion bodyRotation,
        Transform otherCollidertransform, Vector3 pentrationDirection, float penetrationDistance);

    /// <summary>
    ///计算这个身体和附近邻居之间的穿透量。或者，操作（委托）
    ///可以传入，因此可以根据需要修改结果的位置/旋转。
    /// </summary>
    /// <param name="position">The position reference.</param>
    /// <param name="rotation">The rotation reference.</param>
    /// <param name="Action">This delegate will be called after the penetration value is calculated.</param>
    /// <returns>True if there was any valid overlap.</returns>
    public abstract bool ComputePenetration(ref Vector3 position, ref Quaternion rotation, PentrationDelegate Action);

    public abstract int OverlapBody(Vector3 position, Quaternion rotation);

    protected abstract void OnEnable();
    protected abstract void OnDisable();
    protected virtual void Awake()
    {
        hideFlags = HideFlags.None;
    }
}
