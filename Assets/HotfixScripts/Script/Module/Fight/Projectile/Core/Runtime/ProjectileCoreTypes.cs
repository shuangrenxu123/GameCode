namespace Fight.Projectile
{
    public enum ProjectileMotionType
    {
        Linear = 0,
        Static = 1,
        Round = 2,
        WeakHoming = 3,
    }

    public enum ProjectileHitResolveType
    {
        None = 0,
        Damage = 1,
        Regeneration = 2,
    }

    public enum ProjectileHitResolveMode
    {
        OnLaunchOnly = 0,
        Continuous = 1,
        Periodic = 2,
    }

    public enum ProjectileTargetKind
    {
        None = 0,
        Entity = 1,
        Environment = 2,
        ProjectileReceiver = 3,
    }

    public enum ProjectileEndReason
    {
        None = 0,
        ManualStop = 1,
        LifeTime = 2,
        MaxDistance = 3,
        TargetHit = 4,
        PierceExhausted = 5,
        AnchorLost = 6,
        TargetLost = 7,
    }

    public readonly struct ProjectileHandle
    {
        public readonly int Id;

        public ProjectileHandle(int id)
        {
            Id = id;
        }

        public bool IsValid => Id > 0;
        public static ProjectileHandle Invalid => new ProjectileHandle(0);
    }

    public struct ProjectilePose
    {
        public ProjectileVector3 Position;
        public ProjectileVector3 Forward;
        public ProjectileVector3 Velocity;
        public ProjectileVector3 Scale;

        public static ProjectilePose Create(
            ProjectileVector3 position,
            ProjectileVector3 forward,
            float speed)
        {
            ProjectileVector3 normalizedForward = ProjectileVector3.NormalizeOrForward(forward);
            return new ProjectilePose
            {
                Position = position,
                Forward = normalizedForward,
                Velocity = normalizedForward * ProjectileMath.Max(0f, speed),
                Scale = ProjectileVector3.One,
            };
        }
    }

    public readonly struct ProjectileTrace
    {
        public readonly ProjectileVector3 PreviousPosition;
        public readonly ProjectileVector3 CurrentPosition;
        public readonly ProjectileVector3 PreviousForward;
        public readonly ProjectileVector3 CurrentForward;
        public readonly ProjectileVector3 Delta;

        public ProjectileTrace(
            ProjectileVector3 previousPosition,
            ProjectileVector3 currentPosition,
            ProjectileVector3 previousForward,
            ProjectileVector3 currentForward)
        {
            PreviousPosition = previousPosition;
            CurrentPosition = currentPosition;
            PreviousForward = previousForward;
            CurrentForward = currentForward;
            Delta = currentPosition - previousPosition;
        }
    }

    public readonly struct ProjectileFrameContext
    {
        public readonly float DeltaTime;
        public readonly float AliveTime;
        public readonly ProjectilePose PreviousPose;
        public readonly ProjectilePose CurrentPose;
        public readonly ProjectileTrace Trace;
        public readonly IProjectileTargetProvider TargetProvider;

        public ProjectileFrameContext(
            float deltaTime,
            float aliveTime,
            in ProjectilePose previousPose,
            in ProjectilePose currentPose,
            in ProjectileTrace trace,
            IProjectileTargetProvider targetProvider = null)
        {
            DeltaTime = deltaTime;
            AliveTime = aliveTime;
            PreviousPose = previousPose;
            CurrentPose = currentPose;
            Trace = trace;
            TargetProvider = targetProvider;
        }
    }
}
