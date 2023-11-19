using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public abstract class CharacterCollisions : MonoBehaviour
{
    public HitInfo[] Hitbuffer { get; private set; } = new HitInfo[20];

    protected CharacterActor CharacterActor { get; private set; }
    public PhysicsComponent PhysicsComponent { get; protected set; }

    readonly CollisionInfo _collisionInfo = new CollisionInfo();
    float BackstepDistance => 2f * ContactOffset;
    public abstract float ContactOffset { get; }
    public abstract float CollisionRadius { get; }

    protected Transform Transform;
    public static CharacterCollisions CreateInstance(GameObject gameObject)
    {
        Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        Rigidbody rigidbody3D = gameObject.GetComponent<Rigidbody>();

        if (rigidbody2D != null)
        {
            return null;
        }
        else if (rigidbody3D != null)
        {
            return gameObject.GetOrAddComponent<CharacterCollisions3D>();
        }
        return null;
    }

    /// <summary>
    /// 使用投射球体垂直检查地面。
    /// </summary>
    /// <param name="position"></param>
    /// <param name="stepOffset"></param>
    /// <param name="stepDownDistance"></param>
    /// <param name="hitInfoFilter"></param>
    /// <param name="hitFilter"></param>
    /// <returns></returns>
    public CollisionInfo CheckForGround(Vector3 position, float stepOffset, float stepDownDistance, in HitInfoFilter hitInfoFilter, HitFilterDelegate hitFilter = null)
    {
        float preDistance = stepOffset + BackstepDistance;
        //displacement : 位移
        Vector3 displacement = CustomUtilities.Multiply(-CharacterActor.Up, Mathf.Max(CharacterConstants.GroundCheckDistance, stepDownDistance));
        Vector3 origin = CharacterActor.GetBottomCenter(position, preDistance);
        Vector3 castDisplacement = displacement + CustomUtilities.Multiply(Vector3.Normalize(displacement), preDistance + ContactOffset);

        PhysicsComponent.SphereCast(out HitInfo hitInfo, origin, CollisionRadius, castDisplacement, in hitInfoFilter, false, hitFilter);

        UpdateCollisionInfo(_collisionInfo, position, in hitInfo, castDisplacement, preDistance, true, in hitInfoFilter);
        return _collisionInfo;
    }
    public CollisionInfo CheckForGroundRay(Vector3 position, in HitInfoFilter hitInfoFilter, HitFilterDelegate hitFilter = null)
    {
        float preDistance = CharacterActor.BodySize.x / 2f;
        Vector3 origin = CharacterActor.GetBottomCenter(position);
        Vector3 displacement = CustomUtilities.Multiply(-CharacterActor.Up, Mathf.Max(CharacterConstants.GroundCheckDistance, CharacterActor.stepDownDistance));
        Vector3 castDisplacement = displacement + CustomUtilities.Multiply(Vector3.Normalize(displacement), preDistance);
        PhysicsComponent.SphereCast(out HitInfo hitInfo, origin, CollisionRadius, castDisplacement, in hitInfoFilter, false, hitFilter);
        UpdateCollisionInfo(_collisionInfo, position, in hitInfo, castDisplacement, preDistance, true, in hitInfoFilter);
        return _collisionInfo;
    }
    /// <summary>
    /// 施放当前体型获得最接近的命中。
    /// </summary>
    /// <param name="position"></param>
    /// <param name="displacement"></param>
    /// <param name="bottomOffsetm"></param>
    /// <param name="hitInfoFilter"></param>
    /// <param name="hitFilter"></param>
    /// <returns></returns>
    public CollisionInfo CastBody(Vector3 position, Vector3 displacement, float bottomOffsetm, in HitInfoFilter hitInfoFilter, bool allowOverlaps = false, HitFilterDelegate hitFilter = null)
    {
        float preDistance = BackstepDistance;
        Vector3 direction = Vector3.Normalize(displacement);
        Vector3 bottom = CharacterActor.GetBottomCenter(position, bottomOffsetm);
        Vector3 top = CharacterActor.GetTopCenter(position);
        bottom -= CustomUtilities.Multiply(direction, preDistance);
        top -= CustomUtilities.Multiply(direction, preDistance);

        Vector3 castDisplacement = displacement + CustomUtilities.Multiply(Vector3.Normalize(displacement), preDistance + ContactOffset);
        float radius = CharacterActor.BodySize.x / 2f;
        PhysicsComponent.CapsuleCast(out HitInfo hitInfo, bottom, top, radius, castDisplacement, in hitInfoFilter, allowOverlaps, hitFilter);
        UpdateCollisionInfo(_collisionInfo, position, in hitInfo, castDisplacement, preDistance, false, in hitInfoFilter);
        return _collisionInfo;

    }
    public bool CheckOverlap(Vector3 position, float bottomOffset, in HitInfoFilter hitInfoFilter, HitFilterDelegate hitFilter = null)
    {
        Vector3 bottom = CharacterActor.GetBottomCenter(position, bottomOffset);
        Vector3 top = CharacterActor.GetTopCenter(position);
        float radius = CharacterActor.BodySize.x / 2f - CharacterConstants.SkinWidth;

        bool overlap = PhysicsComponent.OverlapCapsule(
            bottom,
            top,
            radius,
            in hitInfoFilter,
            hitFilter
        );

        return overlap;
    }
    public bool CheckBodySize(Vector3 size, Vector3 position, in HitInfoFilter hitInfoFilter, HitFilterDelegate hitFilter = null)
    {
        Vector3 bottom = CharacterActor.GetBottomCenter(position, size);
        float radius = size.x / 2f;

        // BottomCenterToTopCenter = Up displacement
        Vector3 castDisplacement = CharacterActor.GetBottomCenterToTopCenter(size);
        PhysicsComponent.SphereCast(
            out HitInfo hitInfo,
            bottom,
            radius,
            castDisplacement,
            in hitInfoFilter,
            false,
            hitFilter
        );

        bool overlap = hitInfo.hit;
        return !overlap;
    }
    public bool CheckBodySize(Vector3 size, in HitInfoFilter hitInfoFilter, HitFilterDelegate hitFilter = null) => CheckBodySize(size, CharacterActor.Position, in hitInfoFilter, hitFilter);
    private void UpdateCollisionInfo(CollisionInfo collisionInfo, Vector3 position, in HitInfo hitInfo, Vector3 castDisplacement, float preDistance, bool calculateEdge, in HitInfoFilter hitInfoFilter)
    {
        if (hitInfo.hit)
        {
            Vector3 castDirection = Vector3.Normalize(castDisplacement);

            float closestDistance = hitInfo.distance - preDistance - ContactOffset;

            var displacement = castDirection * closestDistance;

            if (calculateEdge)
            {
                Vector3 edgeCenterReference = CharacterActor.GetBottomCenter(position + displacement, 0f);
                UpdateEdgeInfo(in edgeCenterReference, in hitInfo.point, in hitInfoFilter, out HitInfo upperHitInfo, out HitInfo lowerHitInfo);

                collisionInfo.SetData(in hitInfo, CharacterActor.Up, displacement, in upperHitInfo, in lowerHitInfo);
            }
            else
            {
                collisionInfo.SetData(in hitInfo, CharacterActor.Up, displacement);
            }
        }
        else
        {
            collisionInfo.Reset();
        }
    }
    void UpdateEdgeInfo(in Vector3 edgeCenterReference, in Vector3 contactPoint, in HitInfoFilter hitInfoFilter, out HitInfo upperHitInfo, out HitInfo lowerHitInfo)
    {
        Vector3 castDirection = Vector3.Normalize(contactPoint - edgeCenterReference);
        Vector3 castDisplacement = CustomUtilities.Multiply(castDirection, CharacterConstants.EdgeRaysCastDistance);
        Vector3 upperHitPosition = edgeCenterReference + CustomUtilities.Multiply(CharacterActor.Up, CharacterConstants.EdgeRaysSeparation);
        Vector3 lowerHitPosition = edgeCenterReference - CustomUtilities.Multiply(CharacterActor.Up, CharacterConstants.EdgeRaysSeparation);

        PhysicsComponent.Raycast(
            out upperHitInfo,
            upperHitPosition,
            castDisplacement,
            in hitInfoFilter
        );

        PhysicsComponent.Raycast(
            out lowerHitInfo,
            lowerHitPosition,
            castDisplacement,
            in hitInfoFilter
        );
    }
    public virtual void Awake()
    {
        CharacterActor = GetComponent<CharacterActor>();
        if (CharacterActor == null)
        {
            Debug.LogError("没有获取到Character");
        }
    }
}
