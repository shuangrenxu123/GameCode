using Fight.Projectile.Authoring;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Projectile.UnityAdapter
{
    public sealed class UnityProjectileRunner : MonoBehaviour
    {
        [SerializeField, LabelText("默认投射物配方")]
        ProjectileRecipeSO defaultRecipe;

        [SerializeField, LabelText("实例工厂")]
        UnityProjectilePrefabFactory instanceFactory;

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

        [SerializeField, LabelText("启用时初始化静态API")]
        bool initializeStaticApi = true;

        [SerializeField, LabelText("自动Update驱动")]
        bool tickInUpdate = true;

        ProjectileWorld world;

        public ProjectileWorld World => world;

        void OnEnable()
        {
            CreateWorld();
        }

        void OnDisable()
        {
            if (initializeStaticApi)
            {
                ProjectileAPI.Shutdown(world);
            }

            world?.StopAll(ProjectileEndReason.ManualStop);
            world = null;
        }

        void Update()
        {
            if (tickInUpdate)
            {
                Tick(Time.deltaTime);
            }
        }

        public void Tick(float deltaTime)
        {
            world?.Tick(deltaTime);
        }

        public ProjectileHandle SpawnDefault(
            Vector3 position,
            Vector3 direction,
            object ownerObject = null,
            object targetObject = null,
            bool canResolveHit = true)
        {
            return Spawn(defaultRecipe, position, direction, ownerObject, targetObject, canResolveHit);
        }

        public ProjectileHandle Spawn(
            ProjectileRecipeSO recipe,
            Vector3 position,
            Vector3 direction,
            object ownerObject = null,
            object targetObject = null,
            bool canResolveHit = true)
        {
            if (world == null || recipe == null)
            {
                return ProjectileHandle.Invalid;
            }

            int ownerId = ResolveObjectId(ownerObject);
            int targetId = ResolveObjectId(targetObject);
            ProjectileRuntimeContext context = ProjectileRuntimeContextBuilder.CreateDefault(
                ownerId,
                targetId,
                ownerObject,
                targetObject,
                canResolveHit);
            var request = new ProjectileFireRequest
            {
                Recipe = recipe.BuildSpec(),
                SpawnPosition = position.ToProjectile(),
                Direction = direction.ToProjectile(),
                SpawnSettings = recipe.SpawnSettings,
                RuntimeContext = context,
                RandomSeed = CreateSeed(),
            };
            return world.Spawn(in request);
        }

        void CreateWorld()
        {
            if (!TryResolveServices(
                    out IUnityProjectileTargetResolver targetResolver,
                    out IProjectileCombatResolver combatResolver))
            {
                world = null;
                return;
            }

            var services = new ProjectileWorldServices
            {
                Query = new UnityProjectilePhysicsQuery(
                    hitMask,
                    triggerInteraction,
                    targetResolver,
                    queryBufferSize),
                CombatResolver = combatResolver,
                TargetProvider = new UnityProjectileTargetProvider(),
                InstanceFactory = instanceFactory,
                PoseWriter = new UnityProjectilePoseWriter(),
            };
            world = new ProjectileWorld(services, queryBufferSize);
            if (initializeStaticApi)
            {
                ProjectileAPI.Initialize(world);
            }
        }

        bool TryResolveServices(
            out IUnityProjectileTargetResolver targetResolver,
            out IProjectileCombatResolver combatResolver)
        {
            targetResolver = targetResolverSource as IUnityProjectileTargetResolver;
            combatResolver = combatResolverSource as IProjectileCombatResolver;

            bool success = true;
            if (targetResolver == null)
            {
                Debug.LogError("UnityProjectileRunner: 目标解析器未绑定或未实现 IUnityProjectileTargetResolver。", this);
                success = false;
            }

            if (combatResolver == null)
            {
                Debug.LogWarning("UnityProjectileRunner: 战斗结算器未绑定，投射物只触发命中事件，不执行伤害或治疗结算。", this);
            }

            if (instanceFactory == null)
            {
                Debug.LogError("UnityProjectileRunner: 实例工厂未绑定。", this);
                success = false;
            }

            return success;
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
    }
}
