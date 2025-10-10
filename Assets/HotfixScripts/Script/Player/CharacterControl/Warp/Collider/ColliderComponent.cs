using UnityEngine;

/// <summary>
/// �����Ƕ�Collider��һ����װ
/// </summary>
public abstract class ColliderComponent : MonoBehaviour
{
    /// <summary>
    /// collider �Ĵ�С
    /// </summary>
    public abstract Vector3 Size { get; set; }

    public abstract Vector3 Offset { get; set; }

    public abstract Vector3 BoundsSize { get; }

    public abstract Collider Collider { get; }

    public Vector3 Center => transform.position + transform.TransformVectorUnscaled(Offset);


    public static ColliderComponent CreateInstance(GameObject gameObject, bool includeChildren = true)
    {
        Collider collider3D = includeChildren ? gameObject.GetComponentInChildren<Collider>() : gameObject.GetComponent<Collider>();
        if (collider3D != null)
        {
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
    ///�����������͸����ھ�֮��Ĵ�͸�������ߣ�������ί�У�
    ///���Դ��룬��˿��Ը�����Ҫ�޸Ľ����λ��/��ת��
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
