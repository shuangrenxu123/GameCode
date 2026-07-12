namespace Fight.Projectile.CombatAdapter
{
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
