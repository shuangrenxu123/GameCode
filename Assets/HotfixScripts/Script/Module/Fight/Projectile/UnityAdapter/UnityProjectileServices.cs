using System.Collections.Generic;
using Fight;
using Fight.Projectile;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Projectile.UnityAdapter
{
    public sealed class UnityProjectilePhysicsQuery : IProjectileQuery
    {
        readonly LayerMask hitMask;
        readonly QueryTriggerInteraction triggerInteraction;
        readonly RaycastHit[] raycastHits;
        readonly Collider[] colliderHits;

        public UnityProjectilePhysicsQuery(
            LayerMask hitMask,
            QueryTriggerInteraction triggerInteraction,
            int maxHits)
        {
            this.hitMask = hitMask;
            this.triggerInteraction = triggerInteraction;
            int capacity = Mathf.Max(1, maxHits);
            raycastHits = new RaycastHit[capacity];
            colliderHits = new Collider[capacity];
        }

        public int Query(in ProjectileQueryRequest request, ProjectileRawHit[] results)
        {
            if (results == null || results.Length == 0)
            {
                return 0;
            }

            switch (request.Detection.Shape)
            {
                case OverlapSphereProjectileDetectionShape overlapSphere:
                    return QueryOverlapSphere(in request, overlapSphere, results);
                case OverlapBoxProjectileDetectionShape overlapBox:
                    return QueryOverlapBox(in request, overlapBox, results);
                case ConeOverlapProjectileDetectionShape coneOverlap:
                    return QueryConeOverlap(in request, coneOverlap, results);
                case RayFanProjectileDetectionShape rayFan:
                    return QueryRayFan(in request, rayFan, results);
                case SphereCastProjectileDetectionShape sphereCast:
                    return QuerySphereCast(in request, sphereCast, results);
                default:
                    return 0;
            }
        }

        int QuerySphereCast(
            in ProjectileQueryRequest request,
            SphereCastProjectileDetectionShape shape,
            ProjectileRawHit[] results)
        {
            Vector3 origin = request.Trace.PreviousPosition.ToUnity();
            Vector3 delta = request.Trace.Delta.ToUnity();
            float distance = delta.magnitude;
            if (distance <= 0.0001f)
            {
                return QueryOverlapSphereAt(request.Pose.Position.ToUnity(), shape.Radius, results);
            }

            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                shape.Radius,
                delta / distance,
                raycastHits,
                distance,
                hitMask,
                triggerInteraction);
            int count = 0;
            for (int i = 0; i < hitCount && count < results.Length; i++)
            {
                RaycastHit hit = raycastHits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                if (TryResolve(
                        hit.collider,
                        hit.point.ToProjectile(),
                        hit.normal.ToProjectile(),
                        hit.distance,
                        out ProjectileRawHit rawHit))
                {
                    results[count++] = rawHit;
                }
            }

            return count;
        }

        int QueryOverlapSphere(
            in ProjectileQueryRequest request,
            OverlapSphereProjectileDetectionShape shape,
            ProjectileRawHit[] results)
        {
            return QueryOverlapSphereAt(request.Pose.Position.ToUnity(), shape.Radius, results);
        }

        int QueryOverlapSphereAt(Vector3 center, float radius, ProjectileRawHit[] results)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                radius,
                colliderHits,
                hitMask,
                triggerInteraction);
            return FillColliderHits(center, hitCount, results);
        }

        int QueryConeOverlap(
            in ProjectileQueryRequest request,
            ConeOverlapProjectileDetectionShape shape,
            ProjectileRawHit[] results)
        {
            Vector3 center = request.Pose.Position.ToUnity();
            Vector3 forward = ResolveForward(request.Pose.Forward.ToUnity());
            float halfAngle = Mathf.Clamp(shape.Angle, 0f, 360f) * 0.5f;
            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                shape.Radius,
                colliderHits,
                hitMask,
                triggerInteraction);
            return FillConeColliderHits(center, forward, halfAngle, hitCount, results);
        }

        int QueryRayFan(
            in ProjectileQueryRequest request,
            RayFanProjectileDetectionShape shape,
            ProjectileRawHit[] results)
        {
            Vector3 origin = request.Pose.Position.ToUnity();
            Vector3 forward = ResolveForward(request.Pose.Forward.ToUnity());
            Vector3 up = Vector3.up;
            float range = Mathf.Max(0f, shape.Range);
            if (range <= 0.0001f)
            {
                return 0;
            }

            int count = 0;
            int rayCount = Mathf.Max(1, shape.RayCount);
            float angle = Mathf.Clamp(shape.Angle, 0f, 360f);
            for (int i = 0; i < rayCount && count < results.Length; i++)
            {
                Vector3 direction = ResolveFanRayDirection(forward, up, angle, rayCount, i);
                int hitCount = Physics.RaycastNonAlloc(
                    origin,
                    direction,
                    raycastHits,
                    range,
                    hitMask,
                    triggerInteraction);
                count = FillRaycastHits(origin, hitCount, results, count);
            }

            return count;
        }

        int QueryOverlapBox(
            in ProjectileQueryRequest request,
            OverlapBoxProjectileDetectionShape shape,
            ProjectileRawHit[] results)
        {
            Vector3 forward = ResolveForward(request.Pose.Forward.ToUnity());

            Quaternion rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            Vector3 size = new Vector3(
                shape.Width,
                shape.Height,
                shape.Length);
            Vector3 center = request.Pose.Position.ToUnity()
                             + forward.normalized * (size.z * 0.5f)
                             + Vector3.up * (size.y * 0.5f);
            int hitCount = Physics.OverlapBoxNonAlloc(
                center,
                size * 0.5f,
                colliderHits,
                rotation,
                hitMask,
                triggerInteraction);
            return FillColliderHits(center, hitCount, results);
        }

        int FillColliderHits(Vector3 origin, int hitCount, ProjectileRawHit[] results)
        {
            int count = 0;
            for (int i = 0; i < hitCount && count < results.Length; i++)
            {
                Collider hitCollider = colliderHits[i];
                if (hitCollider == null)
                {
                    continue;
                }

                Vector3 point = hitCollider.ClosestPoint(origin);
                Vector3 normal = origin - point;
                if (normal.sqrMagnitude <= 0.0001f)
                {
                    normal = Vector3.up;
                }

                float distance = Vector3.Distance(origin, point);
                if (TryResolve(
                        hitCollider,
                        point.ToProjectile(),
                        normal.normalized.ToProjectile(),
                        distance,
                        out ProjectileRawHit rawHit))
                {
                    results[count++] = rawHit;
                }
            }

            return count;
        }

        int FillConeColliderHits(
            Vector3 origin,
            Vector3 forward,
            float halfAngle,
            int hitCount,
            ProjectileRawHit[] results)
        {
            int count = 0;
            for (int i = 0; i < hitCount && count < results.Length; i++)
            {
                Collider hitCollider = colliderHits[i];
                if (hitCollider == null || !IsInsideCone(origin, forward, halfAngle, hitCollider))
                {
                    continue;
                }

                if (TryResolveColliderHit(origin, hitCollider, out ProjectileRawHit rawHit))
                {
                    results[count++] = rawHit;
                }
            }

            return count;
        }

        int FillRaycastHits(
            Vector3 origin,
            int hitCount,
            ProjectileRawHit[] results,
            int startIndex)
        {
            int count = startIndex;
            hitCount = Mathf.Min(hitCount, raycastHits.Length);
            SortRaycastHitsByDistance(raycastHits, hitCount);
            for (int i = 0; i < hitCount && count < results.Length; i++)
            {
                RaycastHit hit = raycastHits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                if (TryResolve(
                        hit.collider,
                        hit.point.ToProjectile(),
                        hit.normal.ToProjectile(),
                        hit.distance,
                        out ProjectileRawHit rawHit))
                {
                    count = AddRawHitDistinct(results, count, in rawHit);
                    if (rawHit.TargetKind == ProjectileTargetKind.Environment)
                    {
                        break;
                    }
                }
            }

            return count;
        }

        static int AddRawHitDistinct(ProjectileRawHit[] results, int count, in ProjectileRawHit rawHit)
        {
            if (rawHit.TargetKind == ProjectileTargetKind.None)
            {
                return count;
            }

            if (rawHit.TargetKind != ProjectileTargetKind.Environment)
            {
                for (int i = 0; i < count; i++)
                {
                    if (results[i].TargetKind == rawHit.TargetKind && results[i].TargetId == rawHit.TargetId)
                    {
                        if (rawHit.Distance < results[i].Distance)
                        {
                            results[i] = rawHit;
                        }

                        return count;
                    }
                }
            }

            if (count >= results.Length)
            {
                return count;
            }

            results[count] = rawHit;
            return count + 1;
        }

        bool TryResolveColliderHit(Vector3 origin, Collider hitCollider, out ProjectileRawHit rawHit)
        {
            Vector3 point = hitCollider.ClosestPoint(origin);
            Vector3 normal = origin - point;
            if (normal.sqrMagnitude <= 0.0001f)
            {
                normal = Vector3.up;
            }

            float distance = Vector3.Distance(origin, point);
            return TryResolve(
                hitCollider,
                point.ToProjectile(),
                normal.normalized.ToProjectile(),
                distance,
                out rawHit);
        }

        static bool TryResolve(
            Collider collider,
            in ProjectileVector3 point,
            in ProjectileVector3 normal,
            float distance,
            out ProjectileRawHit hit)
        {
            hit = default;
            if (collider == null)
            {
                return false;
            }

            CombatEntity target = collider.GetComponentInParent<CombatEntity>();
            if (target != null)
            {
                hit = new ProjectileRawHit
                {
                    TargetKind = ProjectileTargetKind.Entity,
                    TargetId = target.GetInstanceID(),
                    Point = point,
                    Normal = normal,
                    Distance = distance,
                    UserData = target,
                };
                return true;
            }

            hit = new ProjectileRawHit
            {
                TargetKind = ProjectileTargetKind.Environment,
                TargetId = collider.GetInstanceID(),
                Point = point,
                Normal = normal,
                Distance = distance,
                UserData = collider,
            };
            return true;
        }

        static bool IsInsideCone(Vector3 origin, Vector3 forward, float halfAngle, Collider collider)
        {
            if (halfAngle >= 180f)
            {
                return true;
            }

            Vector3 point = collider.ClosestPoint(origin);
            Vector3 direction = point - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = collider.bounds.center - origin;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    return true;
                }
            }

            return Vector3.Angle(forward, direction) <= halfAngle;
        }

        static Vector3 ResolveFanRayDirection(
            Vector3 forward,
            Vector3 up,
            float angle,
            int rayCount,
            int rayIndex)
        {
            if (rayCount <= 1 || angle <= 0.0001f)
            {
                return forward;
            }

            float halfAngle = angle * 0.5f;
            float t = rayIndex / (rayCount - 1f);
            float yawOffset = Mathf.Lerp(-halfAngle, halfAngle, t);
            return Quaternion.AngleAxis(yawOffset, up) * forward;
        }

        static Vector3 ResolveForward(Vector3 forward)
        {
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        static void SortRaycastHitsByDistance(RaycastHit[] hits, int count)
        {
            for (int i = 1; i < count; i++)
            {
                RaycastHit value = hits[i];
                int index = i - 1;
                while (index >= 0 && hits[index].distance > value.distance)
                {
                    hits[index + 1] = hits[index];
                    index--;
                }

                hits[index + 1] = value;
            }
        }
    }

    public sealed class UnityProjectileTargetProvider : IProjectileTargetProvider
    {
        public bool TryGetTargetPosition(object targetObject, out ProjectileVector3 position)
        {
            switch (targetObject)
            {
                case Transform transform:
                    if (transform == null)
                    {
                        break;
                    }

                    position = transform.position.ToProjectile();
                    return transform.gameObject.activeInHierarchy;
                case Component component:
                    if (component == null)
                    {
                        break;
                    }

                    position = component.transform.position.ToProjectile();
                    return component.gameObject.activeInHierarchy;
                case GameObject gameObject:
                    if (gameObject == null)
                    {
                        break;
                    }

                    position = gameObject.transform.position.ToProjectile();
                    return gameObject.activeInHierarchy;
                default:
                    position = ProjectileVector3.Zero;
                    return false;
            }

            position = ProjectileVector3.Zero;
            return false;
        }
    }

    public sealed class UnityProjectilePoseWriter : IProjectilePoseWriter
    {
        public void ApplyPose(object instanceObject, in ProjectilePose pose)
        {
            Transform transform = ResolveTransform(instanceObject);
            if (transform == null)
            {
                return;
            }

            transform.position = pose.Position.ToUnity();
            transform.rotation = pose.Forward.ToUnityRotation();
            transform.localScale = pose.Scale.ToUnity();
        }

        static Transform ResolveTransform(object instanceObject)
        {
            switch (instanceObject)
            {
                case Transform transform:
                    return transform;
                case GameObject gameObject:
                    return gameObject.transform;
                case Component component:
                    return component.transform;
                default:
                    return null;
            }
        }
    }

    public sealed class UnityProjectilePrefabFactory : MonoBehaviour, IProjectileInstanceFactory
    {
        [SerializeField, LabelText("投射物预制体")]
        GameObject projectilePrefab;

        [SerializeField, LabelText("预热数量")]
        [MinValue(0)]
        int prewarmCount = 8;

        readonly Stack<GameObject> pool = new Stack<GameObject>(16);

        void Awake()
        {
            Prewarm();
        }

        public object Create(in ProjectileInstanceCreateRequest request)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("UnityProjectilePrefabFactory: 未绑定投射物预制体。", this);
                return null;
            }

            GameObject instance = pool.Count > 0 ? pool.Pop() : Instantiate(projectilePrefab, transform);
            instance.transform.position = request.Pose.Position.ToUnity();
            instance.transform.rotation = request.Pose.Forward.ToUnityRotation();
            instance.transform.localScale = request.Pose.Scale.ToUnity();
            instance.SetActive(true);
            return instance;
        }

        public void Release(object instanceObject)
        {
            GameObject instance = ResolveGameObject(instanceObject);
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            pool.Push(instance);
        }

        void Prewarm()
        {
            if (projectilePrefab == null)
            {
                return;
            }

            while (pool.Count < prewarmCount)
            {
                GameObject instance = Instantiate(projectilePrefab, transform);
                instance.SetActive(false);
                pool.Push(instance);
            }
        }

        static GameObject ResolveGameObject(object instanceObject)
        {
            switch (instanceObject)
            {
                case GameObject gameObject:
                    return gameObject;
                case Component component:
                    return component.gameObject;
                default:
                    return null;
            }
        }
    }
}
