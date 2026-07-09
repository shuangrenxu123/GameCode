using System.Collections.Generic;

namespace Fight.Projectile
{
    public sealed class ProjectileHitWindowFilter : IProjectileHitFilter
    {
        readonly HashSet<int> hitTargetIds = new HashSet<int>();
        bool isWindowOpen;

        public void BeginWindow()
        {
            isWindowOpen = true;
            hitTargetIds.Clear();
        }

        public void EndWindow()
        {
            isWindowOpen = false;
            hitTargetIds.Clear();
        }

        public void Clear()
        {
            hitTargetIds.Clear();
        }

        public bool CanHit(ProjectileRuntimeState state, in ProjectileHitContext context)
        {
            if (!isWindowOpen
                || context.Hit.TargetKind != ProjectileTargetKind.Entity
                || context.Hit.TargetId == 0)
            {
                return true;
            }

            if (hitTargetIds.Contains(context.Hit.TargetId))
            {
                return false;
            }

            hitTargetIds.Add(context.Hit.TargetId);
            return true;
        }
    }
}
