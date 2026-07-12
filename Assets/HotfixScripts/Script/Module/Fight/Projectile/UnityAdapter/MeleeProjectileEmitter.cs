using System.Collections.Generic;
using Fight.Projectile.Authoring;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Projectile.UnityAdapter
{
    public sealed class MeleeProjectileEmitter : MonoBehaviour
    {
        const QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.Collide;
        const int QueryBufferSize = 32;
        const int MaxProjectilesPerFrame = 32;
        const int PrewarmCount = 4;

        [SerializeField, LabelText("投射物配置")]
        ProjectileRecipeSO recipe;

        [SerializeField, LabelText("投射物执行器预制体")]
        UnityProjectileRuntimeRunner runnerPrefab;

        [SerializeField, LabelText("刀身起点")]
        Transform pointA;

        [SerializeField, LabelText("刀身终点")]
        Transform pointB;

        [SerializeField, LabelText("发起者")]
        CombatEntity owner;

        [SerializeField, LabelText("命中层级")]
        LayerMask hitMask = ~0;

        [SerializeField, LabelText("时间插值采样数")]
        [MinValue(1)]
        int timeSampleCount = 3;

        [SerializeField, LabelText("刀身采样数")]
        [MinValue(1)]
        int bladeSampleCount = 5;

        readonly Stack<UnityProjectileRuntimeRunner> pool = new Stack<UnityProjectileRuntimeRunner>(32);
        readonly List<UnityProjectileRuntimeRunner> activeProjectiles = new List<UnityProjectileRuntimeRunner>(32);
        readonly ProjectileHitWindowFilter hitWindowFilter = new ProjectileHitWindowFilter();

        ProjectileWorldServices services;
        Vector3 previousPointA;
        Vector3 previousPointB;
        bool isEmitting;
        int nextProjectileId;

        public CombatEntity Owner
        {
            get => owner;
            set => owner = value;
        }

        void Awake()
        {
            CreateServices();
            Prewarm();
        }

        void Update()
        {
            if (!isEmitting)
            {
                return;
            }

            EmitFrame();
        }

        void OnDestroy()
        {
            DisableDamageCollider();
            pool.Clear();
        }

        public void EnableDamageCollider()
        {
            if (!CanEmit())
            {
                return;
            }

            if (services == null)
            {
                CreateServices();
            }

            if (services == null)
            {
                return;
            }

            previousPointA = pointA.position;
            previousPointB = pointB.position;
            hitWindowFilter.BeginWindow();
            isEmitting = true;
        }

        public void DisableDamageCollider()
        {
            if (!isEmitting)
            {
                hitWindowFilter.EndWindow();
                return;
            }

            isEmitting = false;
            hitWindowFilter.EndWindow();
        }

        public void StartAttack()
        {
            EnableDamageCollider();
        }

        public void StopAttack()
        {
            DisableDamageCollider();
        }

        void EmitFrame()
        {
            Vector3 currentPointA = pointA.position;
            Vector3 currentPointB = pointB.position;
            Vector3 forward = ResolveForward(previousPointA, previousPointB, currentPointA, currentPointB);
            int emittedCount = 0;

            int resolvedTimeSamples = Mathf.Max(1, timeSampleCount);
            int resolvedBladeSamples = Mathf.Max(1, bladeSampleCount);
            for (int timeIndex = 1; timeIndex <= resolvedTimeSamples; timeIndex++)
            {
                float timeT = timeIndex / (float)resolvedTimeSamples;
                for (int bladeIndex = 0; bladeIndex < resolvedBladeSamples; bladeIndex++)
                {
                    if (emittedCount >= MaxProjectilesPerFrame)
                    {
                        break;
                    }

                    float bladeT = resolvedBladeSamples <= 1
                        ? 0.5f
                        : bladeIndex / (resolvedBladeSamples - 1f);
                    Vector3 previousBladePoint = Vector3.Lerp(previousPointA, previousPointB, bladeT);
                    Vector3 currentBladePoint = Vector3.Lerp(currentPointA, currentPointB, bladeT);
                    Vector3 spawnPosition = Vector3.Lerp(previousBladePoint, currentBladePoint, timeT);
                    SpawnHitProjectile(spawnPosition, forward, emittedCount);
                    emittedCount++;
                }
            }

            previousPointA = currentPointA;
            previousPointB = currentPointB;
        }

        void SpawnHitProjectile(Vector3 position, Vector3 forward, int spawnIndex)
        {
            UnityProjectileRuntimeRunner runner = RentRunner();
            if (runner == null)
            {
                return;
            }

            runner.transform.SetPositionAndRotation(position, ResolveDirectionRotation(forward));
            runner.gameObject.SetActive(true);
            runner.Stopped -= OnProjectileStopped;
            runner.Stopped += OnProjectileStopped;
            activeProjectiles.Add(runner);

            ProjectileRecipeSpec recipeSpec = recipe.BuildSpec();
            ProjectileRuntimeContext context = ProjectileRuntimeContextBuilder.CreateDefault(
                ResolveObjectId(owner),
                0,
                owner,
                null,
                true);
            int seed = CreateSeed();
            var request = new ProjectileRunnerLaunchRequest(
                CreateHandle(),
                recipeSpec,
                position.ToProjectile(),
                forward.ToProjectile(),
                Mathf.Max(1, timeSampleCount * bladeSampleCount),
                spawnIndex,
                seed,
                context);
            runner.Launch(in request, services, QueryBufferSize);
        }

        UnityProjectileRuntimeRunner RentRunner()
        {
            while (pool.Count > 0)
            {
                UnityProjectileRuntimeRunner runner = pool.Pop();
                if (runner != null)
                {
                    return runner;
                }
            }

            return runnerPrefab != null
                ? Instantiate(runnerPrefab, transform)
                : null;
        }

        void OnProjectileStopped(UnityProjectileRuntimeRunner runner, ProjectileEndReason reason)
        {
            if (runner == null)
            {
                return;
            }

            runner.Stopped -= OnProjectileStopped;
            activeProjectiles.Remove(runner);
            runner.gameObject.SetActive(false);
            runner.transform.SetParent(transform, false);
            pool.Push(runner);
        }

        void Prewarm()
        {
            if (runnerPrefab == null)
            {
                return;
            }

            while (pool.Count < PrewarmCount)
            {
                UnityProjectileRuntimeRunner runner = Instantiate(runnerPrefab, transform);
                runner.gameObject.SetActive(false);
                pool.Push(runner);
            }
        }

        void CreateServices()
        {
            services = new ProjectileWorldServices
            {
                Query = new UnityProjectilePhysicsQuery(
                    hitMask,
                    TriggerInteraction,
                    QueryBufferSize),
                TargetProvider = new UnityProjectileTargetProvider(),
                HitFilters = new IProjectileHitFilter[]
                {
                    new OwnerIgnoreProjectileHitFilter(),
                    hitWindowFilter,
                },
            };
        }

        bool CanEmit()
        {
            return recipe != null
                   && runnerPrefab != null
                   && pointA != null
                   && pointB != null
                   && owner != null;
        }

        ProjectileHandle CreateHandle()
        {
            return new ProjectileHandle(++nextProjectileId);
        }

        static Vector3 ResolveForward(
            Vector3 previousA,
            Vector3 previousB,
            Vector3 currentA,
            Vector3 currentB)
        {
            Vector3 movement = ((currentA + currentB) - (previousA + previousB)) * 0.5f;
            if (movement.sqrMagnitude > 0.0001f)
            {
                return movement.normalized;
            }

            Vector3 bladeDirection = currentB - currentA;
            return bladeDirection.sqrMagnitude > 0.0001f
                ? bladeDirection.normalized
                : Vector3.forward;
        }

        static Quaternion ResolveDirectionRotation(Vector3 direction)
        {
            return direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
        }

        static int ResolveObjectId(Object value)
        {
            return value != null ? value.GetInstanceID() : 0;
        }

        static int CreateSeed()
        {
            int seed = Random.Range(1, int.MaxValue);
            return seed == 0 ? 1 : seed;
        }
    }
}
