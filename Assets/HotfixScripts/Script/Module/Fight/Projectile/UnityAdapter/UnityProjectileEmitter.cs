using System.Collections.Generic;
using Fight.Projectile.Authoring;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Projectile.UnityAdapter
{
    public sealed class UnityProjectileEmitter : MonoBehaviour
    {
        [SerializeField, LabelText("投射物配方")]
        ProjectileRecipeSO recipe;

        [SerializeField, LabelText("投射物执行器预制体")]
        UnityProjectileRuntimeRunner runnerPrefab;

        [SerializeField, LabelText("发射点")]
        Transform muzzle;

        [SerializeField, LabelText("发射者")]
        CombatEntity owner;

        [SerializeField, LabelText("目标对象")]
        Object targetObject;

        [SerializeField, LabelText("目标解析器")]
        MonoBehaviour targetResolverSource;

        [SerializeField, LabelText("战斗结算器")]
        MonoBehaviour combatResolverSource;

        [SerializeField, LabelText("命中层级")]
        LayerMask hitMask = ~0;

        [SerializeField, LabelText("触发器检测模式")]
        QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        [SerializeField, LabelText("单次查询缓存数量")]
        [MinValue(1)]
        int queryBufferSize = 32;

        [SerializeField, LabelText("对象池预热数量")]
        [MinValue(0)]
        int prewarmCount = 8;

        [SerializeField, LabelText("发射角度偏移")]
        float fireAngleOffset;

        readonly Stack<UnityProjectileRuntimeRunner> pool = new Stack<UnityProjectileRuntimeRunner>(16);
        readonly List<UnityProjectileRuntimeRunner> activeProjectiles = new List<UnityProjectileRuntimeRunner>(16);
        ProjectileWorldServices services;

        public CombatEntity Owner
        {
            get => owner;
            set => owner = value;
        }

        public Object TargetObject
        {
            get => targetObject;
            set => targetObject = value;
        }

        void Awake()
        {
            CreateServices();
            Prewarm();
        }

        void OnDestroy()
        {
            RecycleActiveProjectiles(ProjectileEndReason.ManualStop);
            pool.Clear();
        }

        public void Fire()
        {
            Fire(canResolveHit: true);
        }

        public void Fire(bool canResolveHit)
        {
            if (services == null)
            {
                CreateServices();
            }

            if (services == null || recipe == null || runnerPrefab == null)
            {
                return;
            }

            ProjectileRecipeSpec recipeSpec = recipe.BuildSpec();
            ProjectileSpawnSettings spawnSettings = recipe.SpawnSettings.Normalized();
            Quaternion spawnRotation = ResolveSpawnRotation();
            Vector3 baseDirection = spawnRotation * Vector3.forward;
            int seed = CreateSeed();
            var random = new ProjectileSpawnRandom(seed);
            float batchRandomYaw = spawnSettings.BatchRandomAngle
                ? random.Range(spawnSettings.RandomAngleMin, spawnSettings.RandomAngleMax)
                : 0f;

            for (int i = 0; i < spawnSettings.SpawnCount; i++)
            {
                Vector3 direction = ApplySpread(baseDirection, spawnSettings.SpawnCount, i, spawnSettings.SpawnAngle);
                float randomYaw = spawnSettings.BatchRandomAngle
                    ? batchRandomYaw
                    : random.Range(spawnSettings.RandomAngleMin, spawnSettings.RandomAngleMax);
                direction = ApplyYaw(direction, randomYaw);
                SpawnSingle(recipeSpec, direction, spawnSettings.SpawnCount, i, seed, canResolveHit);
            }
        }

        public void StartAttack()
        {
            Fire();
        }

        public void StopAttack()
        {
            RecycleActiveProjectiles(ProjectileEndReason.ManualStop);
        }

        public void RecycleActiveProjectiles(ProjectileEndReason reason)
        {
            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                UnityProjectileRuntimeRunner runner = activeProjectiles[i];
                if (runner == null)
                {
                    activeProjectiles.RemoveAt(i);
                    continue;
                }

                if (runner.IsRunning)
                {
                    runner.Stop(reason);
                }
            }
        }

        void SpawnSingle(
            ProjectileRecipeSpec recipeSpec,
            Vector3 direction,
            int spawnBatchCount,
            int spawnBatchIndex,
            int seed,
            bool canResolveHit)
        {
            UnityProjectileRuntimeRunner runner = RentRunner();
            if (runner == null)
            {
                return;
            }

            runner.transform.SetPositionAndRotation(ResolveSpawnPosition(), ResolveDirectionRotation(direction));
            runner.gameObject.SetActive(true);
            runner.Stopped -= OnProjectileStopped;
            runner.Stopped += OnProjectileStopped;
            activeProjectiles.Add(runner);

            int ownerId = ResolveObjectId(owner);
            int targetId = ResolveObjectId(targetObject);
            ProjectileRuntimeContext context = ProjectileRuntimeContextBuilder.CreateDefault(
                ownerId,
                targetId,
                owner,
                targetObject,
                canResolveHit);
            var request = new ProjectileRunnerLaunchRequest(
                CreateHandle(),
                recipeSpec,
                ResolveSpawnPosition().ToProjectile(),
                direction.ToProjectile(),
                spawnBatchCount,
                spawnBatchIndex,
                seed,
                context);
            runner.Launch(in request, services, Mathf.Max(1, queryBufferSize));
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

            while (pool.Count < prewarmCount)
            {
                UnityProjectileRuntimeRunner runner = Instantiate(runnerPrefab, transform);
                runner.gameObject.SetActive(false);
                pool.Push(runner);
            }
        }

        void CreateServices()
        {
            IUnityProjectileTargetResolver targetResolver = targetResolverSource as IUnityProjectileTargetResolver;
            IProjectileCombatResolver combatResolver = combatResolverSource as IProjectileCombatResolver;

            if (targetResolver == null)
            {
                Debug.LogError("UnityProjectileEmitter: 目标解析器未绑定或未实现 IUnityProjectileTargetResolver。", this);
                services = null;
                return;
            }

            if (combatResolver == null)
            {
                Debug.LogWarning("UnityProjectileEmitter: 战斗结算器未绑定，投射物只触发命中事件，不执行伤害或治疗结算。", this);
            }

            services = new ProjectileWorldServices
            {
                Query = new UnityProjectilePhysicsQuery(
                    hitMask,
                    triggerInteraction,
                    targetResolver,
                    Mathf.Max(1, queryBufferSize)),
                CombatResolver = combatResolver,
                TargetProvider = new UnityProjectileTargetProvider(),
            };
        }

        Vector3 ResolveSpawnPosition()
        {
            return muzzle != null ? muzzle.position : transform.position;
        }

        Quaternion ResolveSpawnRotation()
        {
            Quaternion baseRotation = muzzle != null ? muzzle.rotation : transform.rotation;
            return Mathf.Abs(fireAngleOffset) <= 0.0001f
                ? baseRotation
                : Quaternion.AngleAxis(fireAngleOffset, Vector3.up) * baseRotation;
        }

        static Quaternion ResolveDirectionRotation(Vector3 direction)
        {
            return direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
        }

        static Vector3 ApplySpread(Vector3 direction, int batchCount, int batchIndex, float spawnAngle)
        {
            Vector3 normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector3.forward;
            if (batchCount <= 1 || Mathf.Abs(spawnAngle) <= 0.0001f)
            {
                return normalizedDirection;
            }

            float center = (batchCount - 1) * 0.5f;
            float yawOffset = (batchIndex - center) * spawnAngle;
            return ApplyYaw(normalizedDirection, yawOffset);
        }

        static Vector3 ApplyYaw(Vector3 direction, float yawOffset)
        {
            Vector3 normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector3.forward;
            return Mathf.Abs(yawOffset) <= 0.0001f
                ? normalizedDirection
                : Quaternion.AngleAxis(yawOffset, Vector3.up) * normalizedDirection;
        }

        static int ResolveObjectId(object value)
        {
            switch (value)
            {
                case Object unityObject:
                    return unityObject.GetInstanceID();
                case null:
                    return 0;
                default:
                    return value.GetHashCode();
            }
        }

        static int CreateSeed()
        {
            int seed = Random.Range(1, int.MaxValue);
            return seed == 0 ? 1 : seed;
        }

        ProjectileHandle CreateHandle()
        {
            return new ProjectileHandle(++nextProjectileId);
        }

        struct ProjectileSpawnRandom
        {
            uint state;

            public ProjectileSpawnRandom(int seed)
            {
                state = (uint)(seed == 0 ? 1 : seed);
            }

            public float Range(float min, float max)
            {
                if (Mathf.Abs(max - min) <= 0.0001f)
                {
                    return min;
                }

                return min + (max - min) * Next01();
            }

            float Next01()
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return (state & 0x00FFFFFFu) / 16777216f;
            }
        }

        int nextProjectileId;
    }
}
