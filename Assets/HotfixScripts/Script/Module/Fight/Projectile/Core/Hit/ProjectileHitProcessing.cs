using Fight;

namespace Fight.Projectile
{
    public interface IProjectileHitFilter
    {
        bool CanHit(ProjectileRuntimeState state, in ProjectileHitContext context);
    }

    public interface IProjectileHitResponse
    {
        void OnHit(ProjectileRuntimeState state, in ProjectileHitContext context, ref ProjectilePose pose);
    }

    public readonly struct ProjectileHitContext
    {
        public readonly ProjectileHit Hit;
        public readonly CombatEntity Target;
        public readonly int HitIndex;

        public ProjectileHitContext(
            in ProjectileHit hit,
            int hitIndex)
        {
            Hit = hit;
            Target = hit.UserData as CombatEntity;
            HitIndex = hitIndex;
        }
    }

    public readonly struct ProjectileHitProcessResult
    {
        public readonly bool Passed;
        public readonly bool ShouldStop;
        public readonly ProjectileEndReason EndReason;
        public readonly ProjectileHitContext Context;

        public ProjectileHitProcessResult(
            bool passed,
            bool shouldStop,
            ProjectileEndReason endReason,
            in ProjectileHitContext context)
        {
            Passed = passed;
            ShouldStop = shouldStop;
            EndReason = endReason;
            Context = context;
        }

        public static ProjectileHitProcessResult Filtered => new ProjectileHitProcessResult(
            false,
            false,
            ProjectileEndReason.None,
            default);
    }

    public sealed class OwnerIgnoreProjectileHitFilter : IProjectileHitFilter
    {
        public bool CanHit(ProjectileRuntimeState state, in ProjectileHitContext context)
        {
            if (!state.Recipe.Hit.IgnoreOwner)
            {
                return true;
            }

            return context.Hit.TargetId == 0 || context.Hit.TargetId != state.OwnerId;
        }
    }

    public sealed class ProjectileHitProcessor
    {
        readonly IProjectileHitFilter[] filters;
        readonly IProjectileHitResponse[] responses;

        public ProjectileHitProcessor(
            IProjectileHitFilter[] filters = null,
            IProjectileHitResponse[] responses = null)
        {
            this.filters = filters ?? new IProjectileHitFilter[] { new OwnerIgnoreProjectileHitFilter() };
            this.responses = responses ?? System.Array.Empty<IProjectileHitResponse>();
        }

        public ProjectileHitProcessResult Process(
            ProjectileRuntimeState state,
            in ProjectileHit hit,
            ref ProjectilePose pose)
        {
            var context = new ProjectileHitContext(
                in hit,
                state.TotalHitCount);

            if (!PassFilters(state, in context))
            {
                return ProjectileHitProcessResult.Filtered;
            }

            if (!PassTargetCooldown(state, in context))
            {
                return ProjectileHitProcessResult.Filtered;
            }

            RecordTargetCooldown(state, in context);
            for (int i = 0; i < responses.Length; i++)
            {
                responses[i]?.OnHit(state, in context, ref pose);
            }

            if (state.CanResolveHit)
            {
                state.Recipe.HitResolver?.ResolveHit(state, in context);
            }

            state.TotalHitCount++;
            if (ShouldStopAfterHit(state, in context, out ProjectileEndReason reason))
            {
                return new ProjectileHitProcessResult(true, true, reason, in context);
            }

            return new ProjectileHitProcessResult(true, false, ProjectileEndReason.None, in context);
        }

        bool PassFilters(ProjectileRuntimeState state, in ProjectileHitContext context)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i] != null && !filters[i].CanHit(state, in context))
                {
                    return false;
                }
            }

            return true;
        }

        static bool ShouldStopAfterHit(
            ProjectileRuntimeState state,
            in ProjectileHitContext context,
            out ProjectileEndReason reason)
        {
            reason = ProjectileEndReason.None;
            if (context.Hit.TargetKind == ProjectileTargetKind.Entity)
            {
                if (state.Recipe.Stop.DestroyOnTargetHit)
                {
                    reason = ProjectileEndReason.TargetHit;
                    return true;
                }

                if (state.RemainingPierceCount > 0)
                {
                    state.RemainingPierceCount--;
                    return false;
                }

                if (state.Recipe.Hit.PierceCount > 0)
                {
                    reason = ProjectileEndReason.PierceExhausted;
                    return true;
                }
            }

            return false;
        }

        static bool PassTargetCooldown(ProjectileRuntimeState state, in ProjectileHitContext context)
        {
            if (state.Recipe.Hit.TargetHitCooldown <= 0f
                || context.Hit.TargetKind != ProjectileTargetKind.Entity
                || context.Hit.TargetId == 0
                || state.TargetHitCooldownExpireTimes == null)
            {
                return true;
            }

            return !state.TargetHitCooldownExpireTimes.TryGetValue(context.Hit.TargetId, out float expireTime)
                   || state.AliveTime >= expireTime;
        }

        static void RecordTargetCooldown(ProjectileRuntimeState state, in ProjectileHitContext context)
        {
            if (state.Recipe.Hit.TargetHitCooldown <= 0f
                || context.Hit.TargetKind != ProjectileTargetKind.Entity
                || context.Hit.TargetId == 0
                || state.TargetHitCooldownExpireTimes == null)
            {
                return;
            }

            state.TargetHitCooldownExpireTimes[context.Hit.TargetId] =
                state.AliveTime + state.Recipe.Hit.TargetHitCooldown;
        }
    }
}
