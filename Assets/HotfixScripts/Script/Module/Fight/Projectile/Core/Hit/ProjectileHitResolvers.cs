using System.Collections.Generic;
using Fight;

namespace Fight.Projectile
{
    public interface IProjectileHitResolver
    {
        int Order { get; }

        void ResolveHit(
            ProjectileRuntimeState state,
            in ProjectileHitContext context);
    }

    public sealed class CompositeProjectileHitResolver : IProjectileHitResolver
    {
        readonly IProjectileHitResolver[] resolvers;

        public CompositeProjectileHitResolver(
            IEnumerable<IProjectileHitResolver> resolvers,
            int order = 300)
        {
            var resolverList = new List<IProjectileHitResolver>();
            if (resolvers != null)
            {
                foreach (IProjectileHitResolver resolver in resolvers)
                {
                    if (resolver != null)
                    {
                        resolverList.Add(resolver);
                    }
                }
            }

            resolverList.Sort((left, right) => left.Order.CompareTo(right.Order));
            this.resolvers = resolverList.ToArray();
            Order = order;
        }

        public int Order { get; }

        public void ResolveHit(
            ProjectileRuntimeState state,
            in ProjectileHitContext context)
        {
            for (int i = 0; i < resolvers.Length; i++)
            {
                resolvers[i].ResolveHit(state, in context);
            }
        }
    }

    public sealed class DamageActionHitResolver : IProjectileHitResolver
    {
        readonly int baseValue;

        public DamageActionHitResolver(int baseValue = 0, int order = 300)
        {
            this.baseValue = baseValue;
            Order = order;
        }

        public int Order { get; }

        public void ResolveHit(
            ProjectileRuntimeState state,
            in ProjectileHitContext context)
        {
            if (!TryResolveActionData(state, in context, baseValue,
                    out CombatEntity owner, out CombatEntity target, out int value))
            {
                return;
            }

            CombatActionFactor.CreateActionAndExecute<DamageAction>(
                owner,
                new List<CombatEntity> { target },
                value);
        }

        internal static bool TryResolveActionData(
            ProjectileRuntimeState state,
            in ProjectileHitContext context,
            int configuredValue,
            out CombatEntity owner,
            out CombatEntity target,
            out int value)
        {
            owner = state?.OwnerObject as CombatEntity;
            target = context.Target;
            value = configuredValue > 0 ? configuredValue : state?.BaseValue ?? 0;
            return state != null
                   && state.CanResolveHit
                   && owner != null
                   && target != null
                   && value > 0;
        }
    }

    public sealed class RegenerationActionHitResolver : IProjectileHitResolver
    {
        readonly int baseValue;

        public RegenerationActionHitResolver(int baseValue = 0, int order = 300)
        {
            this.baseValue = baseValue;
            Order = order;
        }

        public int Order { get; }

        public void ResolveHit(
            ProjectileRuntimeState state,
            in ProjectileHitContext context)
        {
            if (!DamageActionHitResolver.TryResolveActionData(
                    state,
                    in context,
                    baseValue,
                    out CombatEntity owner,
                    out CombatEntity target,
                    out int value))
            {
                return;
            }

            CombatActionFactor.CreateActionAndExecute<global::RegenerationAction>(
                owner,
                new List<CombatEntity> { target },
                value);
        }
    }

    public sealed class ApplyBuffHitResolver : IProjectileHitResolver
    {
        readonly BuffId buffId;

        public ApplyBuffHitResolver(BuffId buffId, int order = 350)
        {
            this.buffId = buffId;
            Order = order;
        }

        public int Order { get; }

        public void ResolveHit(
            ProjectileRuntimeState state,
            in ProjectileHitContext context)
        {
            CombatEntity owner = state?.OwnerObject as CombatEntity;
            CombatEntity target = context.Target;
            if (state == null
                || !state.CanResolveHit
                || owner == null
                || target == null
                || buffId == BuffId.None)
            {
                return;
            }

            target.AddBuff(buffId, owner);
        }
    }
}
