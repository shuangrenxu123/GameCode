namespace Fight.Projectile
{
    public struct ProjectileRuntimeContext
    {
        public int OwnerId;
        public int TargetId;
        public object OwnerObject;
        public object TargetObject;
        public bool CanResolveHit;
        public int BaseValueOverride;
        public float SpeedMultiplier;

        public ProjectileRuntimeContext Normalized()
        {
            SpeedMultiplier = SpeedMultiplier > 0f ? SpeedMultiplier : 1f;
            return this;
        }
    }

    public struct ProjectileFireRequest
    {
        public ProjectileRecipeSpec Recipe;
        public ProjectileVector3 SpawnPosition;
        public ProjectileVector3 Direction;
        public ProjectileSpawnSettings SpawnSettings;
        public ProjectileRuntimeContext RuntimeContext;
        public int RandomSeed;
    }

    public readonly struct ProjectileInstanceCreateRequest
    {
        public readonly ProjectileHandle Handle;
        public readonly ProjectileRecipeSpec Recipe;
        public readonly ProjectilePose Pose;
        public readonly int OwnerId;
        public readonly object OwnerObject;

        public ProjectileInstanceCreateRequest(
            ProjectileHandle handle,
            ProjectileRecipeSpec recipe,
            in ProjectilePose pose,
            int ownerId,
            object ownerObject)
        {
            Handle = handle;
            Recipe = recipe;
            Pose = pose;
            OwnerId = ownerId;
            OwnerObject = ownerObject;
        }
    }
}
