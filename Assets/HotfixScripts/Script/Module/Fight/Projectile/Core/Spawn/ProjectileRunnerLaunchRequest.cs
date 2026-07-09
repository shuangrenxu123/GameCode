namespace Fight.Projectile
{
    public readonly struct ProjectileRunnerLaunchRequest
    {
        public readonly ProjectileHandle Handle;
        public readonly ProjectileRecipeSpec Recipe;
        public readonly ProjectileVector3 SpawnPosition;
        public readonly ProjectileVector3 Direction;
        public readonly int SpawnBatchCount;
        public readonly int SpawnBatchIndex;
        public readonly int RandomSeed;
        public readonly ProjectileRuntimeContext RuntimeContext;

        public ProjectileRunnerLaunchRequest(
            ProjectileHandle handle,
            ProjectileRecipeSpec recipe,
            ProjectileVector3 spawnPosition,
            ProjectileVector3 direction,
            int spawnBatchCount,
            int spawnBatchIndex,
            int randomSeed,
            ProjectileRuntimeContext runtimeContext)
        {
            Handle = handle;
            Recipe = recipe;
            SpawnPosition = spawnPosition;
            Direction = direction;
            SpawnBatchCount = spawnBatchCount;
            SpawnBatchIndex = spawnBatchIndex;
            RandomSeed = randomSeed;
            RuntimeContext = runtimeContext;
        }
    }
}
