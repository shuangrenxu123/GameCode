namespace Fight.Projectile
{
    public static class ProjectileAPI
    {
        static ProjectileWorld world;

        public static bool IsInitialized => world != null;

        public static void Initialize(ProjectileWorld projectileWorld)
        {
            world = projectileWorld;
        }

        public static void Shutdown(ProjectileWorld projectileWorld)
        {
            if (ReferenceEquals(world, projectileWorld))
            {
                world = null;
            }
        }

        public static ProjectileHandle Spawn(in ProjectileFireRequest request)
        {
            return world != null ? world.Spawn(in request) : ProjectileHandle.Invalid;
        }

        public static void Tick(float deltaTime)
        {
            world?.Tick(deltaTime);
        }

        public static void Stop(ProjectileHandle handle, ProjectileEndReason reason)
        {
            world?.Stop(handle, reason);
        }
    }
}
