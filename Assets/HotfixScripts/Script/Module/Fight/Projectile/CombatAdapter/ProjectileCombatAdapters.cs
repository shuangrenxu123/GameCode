using System.Collections.Generic;
using Fight;
using Fight.Projectile.UnityAdapter;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Projectile.CombatAdapter
{
    public sealed class UnityCombatProjectileTargetResolver : MonoBehaviour, IUnityProjectileTargetResolver
    {
        [SerializeField, LabelText("非战斗目标作为环境")]
        bool treatNonCombatTargetAsEnvironment = true;

        [SerializeField, LabelText("目标缓存上限")]
        [MinValue(0)]
        int targetCacheLimit = 512;

        readonly Dictionary<Collider, CombatEntity> combatCache = new Dictionary<Collider, CombatEntity>(64);

        public bool TryResolve(
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

            CombatEntity entity = ResolveCombatEntity(collider);
            if (entity != null)
            {
                hit = new ProjectileRawHit
                {
                    TargetKind = ProjectileTargetKind.Entity,
                    TargetId = entity.GetInstanceID(),
                    Point = point,
                    Normal = normal,
                    Distance = distance,
                    UserData = entity,
                };
                return true;
            }

            if (!treatNonCombatTargetAsEnvironment)
            {
                return false;
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

        CombatEntity ResolveCombatEntity(Collider collider)
        {
            if (combatCache.TryGetValue(collider, out CombatEntity cachedEntity))
            {
                if (cachedEntity != null)
                {
                    return cachedEntity;
                }

                combatCache.Remove(collider);
            }

            CombatEntity entity = collider.GetComponentInParent<CombatEntity>();
            if (entity == null || targetCacheLimit <= 0)
            {
                return entity;
            }

            if (combatCache.Count >= targetCacheLimit)
            {
                combatCache.Clear();
            }

            combatCache[collider] = entity;
            return entity;
        }
    }

    public sealed class ProjectileCombatResolver : MonoBehaviour, IProjectileCombatResolver
    {
        [SerializeField, LabelText("允许伤害结算")]
        bool enableDamage = true;

        [SerializeField, LabelText("允许治疗结算")]
        bool enableRegeneration = true;

        readonly List<CombatEntity> singleTargetBuffer = new List<CombatEntity>(1);

        public void ResolveHit(ProjectileRuntimeState state, in ProjectileHitContext context)
        {
            if (state == null || context.Hit.UserData is not CombatEntity target)
            {
                return;
            }

            CombatEntity owner = state.OwnerObject as CombatEntity;
            if (owner == null)
            {
                return;
            }

            switch (context.ResolveType)
            {
                case ProjectileHitResolveType.Damage:
                    ResolveDamage(owner, target, context.BaseValue);
                    break;
                case ProjectileHitResolveType.Regeneration:
                    ResolveRegeneration(owner, target, context.BaseValue);
                    break;
            }
        }

        void ResolveDamage(CombatEntity owner, CombatEntity target, int baseValue)
        {
            if (!enableDamage || baseValue <= 0)
            {
                return;
            }

            ExecuteSingleTarget<DamageAction>(owner, target, baseValue);
        }

        void ResolveRegeneration(CombatEntity owner, CombatEntity target, int baseValue)
        {
            if (!enableRegeneration || baseValue <= 0)
            {
                return;
            }

            ExecuteSingleTarget<global::RegenerationAction>(owner, target, baseValue);
        }

        void ExecuteSingleTarget<T>(CombatEntity owner, CombatEntity target, int baseValue)
            where T : CombatAction, new()
        {
            singleTargetBuffer.Clear();
            singleTargetBuffer.Add(target);
            CombatActionFactor.CreateActionAndExecute<T>(owner, singleTargetBuffer, baseValue);
            singleTargetBuffer.Clear();
        }
    }

    public sealed class ProjectileActionPointBridge
    {
        public void Attach(ProjectileWorld world)
        {
            if (world == null)
            {
                return;
            }

            world.ProjectileLaunched += OnProjectileLaunched;
            world.ProjectileHitResolved += OnProjectileHit;
            world.ProjectileStopped += OnProjectileStopped;
        }

        public void Detach(ProjectileWorld world)
        {
            if (world == null)
            {
                return;
            }

            world.ProjectileLaunched -= OnProjectileLaunched;
            world.ProjectileHitResolved -= OnProjectileHit;
            world.ProjectileStopped -= OnProjectileStopped;
        }

        void OnProjectileLaunched(ProjectileRuntimeState state)
        {
        }

        void OnProjectileHit(ProjectileRuntimeState state, ProjectileHitContext context)
        {
        }

        void OnProjectileStopped(ProjectileRuntimeState state, ProjectileEndReason reason)
        {
        }
    }
}
