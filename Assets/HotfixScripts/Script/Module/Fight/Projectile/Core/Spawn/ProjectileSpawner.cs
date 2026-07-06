namespace Fight.Projectile
{
    public sealed class ProjectileSpawner
    {
        public ProjectileHandle Spawn(in ProjectileFireRequest request, ProjectileWorld world)
        {
            if (world == null || request.Recipe == null)
            {
                return ProjectileHandle.Invalid;
            }

            ProjectileSpawnSettings settings = request.SpawnSettings.Normalized();
            if (settings.SpawnCount <= 0)
            {
                settings = ProjectileSpawnSettings.Single;
            }

            var random = new ProjectileRandom(request.RandomSeed);
            float batchRandomYaw = settings.BatchRandomAngle
                ? random.Range(settings.RandomAngleMin, settings.RandomAngleMax)
                : 0f;

            ProjectileHandle firstHandle = ProjectileHandle.Invalid;
            ProjectileVector3 baseDirection = ProjectileVector3.NormalizeOrForward(request.Direction);
            for (int i = 0; i < settings.SpawnCount; i++)
            {
                ProjectileVector3 direction = ApplySpread(baseDirection, settings.SpawnCount, i, settings.SpawnAngle);
                float randomYaw = settings.BatchRandomAngle
                    ? batchRandomYaw
                    : random.Range(settings.RandomAngleMin, settings.RandomAngleMax);
                direction = ProjectileVector3.RotateYaw(direction, randomYaw);

                ProjectileHandle handle = world.SpawnSingle(
                    request.Recipe,
                    request.SpawnPosition,
                    direction,
                    request.RuntimeContext,
                    settings.SpawnCount,
                    i,
                    request.RandomSeed);

                if (!firstHandle.IsValid)
                {
                    firstHandle = handle;
                }
            }

            return firstHandle;
        }

        static ProjectileVector3 ApplySpread(
            ProjectileVector3 direction,
            int batchCount,
            int batchIndex,
            float spawnAngle)
        {
            if (batchCount <= 1 || spawnAngle > -0.0001f && spawnAngle < 0.0001f)
            {
                return ProjectileVector3.NormalizeOrForward(direction);
            }

            float center = (batchCount - 1) * 0.5f;
            float yawOffset = (batchIndex - center) * spawnAngle;
            return ProjectileVector3.RotateYaw(direction, yawOffset);
        }

        struct ProjectileRandom
        {
            uint state;

            public ProjectileRandom(int seed)
            {
                state = (uint)(seed == 0 ? 1 : seed);
            }

            public float Range(float min, float max)
            {
                if (max - min > -0.0001f && max - min < 0.0001f)
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
    }

    public static class ProjectileRuntimeContextBuilder
    {
        public static ProjectileRuntimeContext CreateDefault(
            int ownerId,
            int targetId,
            object ownerObject,
            object targetObject,
            bool canResolveHit)
        {
            return new ProjectileRuntimeContext
            {
                OwnerId = ownerId,
                TargetId = targetId,
                OwnerObject = ownerObject,
                TargetObject = targetObject,
                CanResolveHit = canResolveHit,
                SpeedMultiplier = 1f,
            };
        }
    }
}
