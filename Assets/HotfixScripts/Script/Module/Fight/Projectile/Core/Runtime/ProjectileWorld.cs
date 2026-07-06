using System;
using System.Collections.Generic;

namespace Fight.Projectile
{
    public interface IProjectileTargetProvider
    {
        bool TryGetTargetPosition(object targetObject, out ProjectileVector3 position);
    }

    public interface IProjectileInstanceFactory
    {
        object Create(in ProjectileInstanceCreateRequest request);
        void Release(object instanceObject);
    }

    public interface IProjectilePoseWriter
    {
        void ApplyPose(object instanceObject, in ProjectilePose pose);
    }

    public sealed class ProjectileWorldServices
    {
        public IProjectileQuery Query;
        public IProjectileCombatResolver CombatResolver;
        public IProjectileTargetProvider TargetProvider;
        public IProjectileInstanceFactory InstanceFactory;
        public IProjectilePoseWriter PoseWriter;
        public IProjectileHitFilter[] HitFilters;
        public IProjectileHitResponse[] HitResponses;
    }

    public sealed class ProjectileWorld
    {
        readonly ProjectileWorldServices services;
        readonly ProjectileSpawner spawner = new ProjectileSpawner();
        readonly ProjectileHitProcessor hitProcessor;
        readonly List<ProjectileRuntimeState> activeProjectiles = new List<ProjectileRuntimeState>(64);
        readonly Stack<ProjectileRuntimeState> statePool = new Stack<ProjectileRuntimeState>(64);
        readonly List<ProjectileHitWindowBuffer> hitWindowBuffers = new List<ProjectileHitWindowBuffer>(2);
        readonly int maxHitsPerQuery;
        int nextProjectileId;

        public event Action<ProjectileRuntimeState> ProjectileLaunched;
        public event Action<ProjectileRuntimeState, ProjectileHitContext> ProjectileHitResolved;
        public event Action<ProjectileRuntimeState, ProjectileEndReason> ProjectileStopped;

        public ProjectileWorld(ProjectileWorldServices services, int maxHitsPerQuery = 32)
        {
            this.services = services ?? new ProjectileWorldServices();
            this.maxHitsPerQuery = ProjectileMath.Max(1, maxHitsPerQuery);
            hitProcessor = new ProjectileHitProcessor(this.services.HitFilters, this.services.HitResponses);
        }

        public ProjectileHandle Spawn(in ProjectileFireRequest request)
        {
            return spawner.Spawn(in request, this);
        }

        public ProjectileHandle SpawnSingle(
            ProjectileRecipeSpec recipe,
            ProjectileVector3 position,
            ProjectileVector3 direction,
            ProjectileRuntimeContext runtimeContext,
            int spawnBatchCount,
            int spawnBatchIndex,
            int randomSeed)
        {
            if (recipe == null)
            {
                return ProjectileHandle.Invalid;
            }

            ProjectileRuntimeContext normalizedContext = runtimeContext.Normalized();
            var state = RentState();
            state.Handle = new ProjectileHandle(++nextProjectileId);
            state.Recipe = recipe;
            state.ProjectileAssetId = recipe.ProjectileAssetId;
            state.SpawnPosition = position;
            state.Pose = ProjectilePose.Create(position, direction, recipe.InitialSpeed * normalizedContext.SpeedMultiplier);
            state.SpawnBatchCount = ProjectileMath.Max(1, spawnBatchCount);
            state.SpawnBatchIndex = spawnBatchIndex;
            state.RandomSeed = randomSeed == 0 ? 1 : randomSeed;
            state.OwnerId = normalizedContext.OwnerId;
            state.TargetId = normalizedContext.TargetId;
            state.OwnerObject = normalizedContext.OwnerObject;
            state.TargetObject = normalizedContext.TargetObject;
            state.CanResolveHit = normalizedContext.CanResolveHit;
            state.BaseValue = normalizedContext.BaseValueOverride > 0
                ? normalizedContext.BaseValueOverride
                : recipe.Hit.BaseValue;
            state.RemainingPierceCount = recipe.Hit.PierceCount;
            state.IsRunning = true;
            state.EndReason = ProjectileEndReason.None;
            state.NextHitResolveTime = 0f;
            if (recipe.Hit.TargetHitCooldown > 0f && state.TargetHitCooldownExpireTimes == null)
            {
                state.TargetHitCooldownExpireTimes = new Dictionary<int, float>(8);
            }

            for (int i = 0; i < recipe.MotionModules.Length; i++)
            {
                recipe.MotionModules[i]?.OnAttach(state);
            }

            if (services.InstanceFactory != null)
            {
                var instanceRequest = new ProjectileInstanceCreateRequest(
                    state.Handle,
                    recipe,
                    in state.Pose,
                    state.OwnerId,
                    state.OwnerObject);
                state.InstanceObject = services.InstanceFactory.Create(in instanceRequest);
            }

            services.PoseWriter?.ApplyPose(state.InstanceObject, in state.Pose);
            activeProjectiles.Add(state);
            ProjectileLaunched?.Invoke(state);
            ResolveLaunchHitWindow(state);
            return state.Handle;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                ProjectileRuntimeState state = activeProjectiles[i];
                if (state == null || !state.IsRunning)
                {
                    RemoveAt(i);
                    continue;
                }

                TickSingle(state, deltaTime);
                if (!state.IsRunning)
                {
                    RemoveAt(i);
                }
            }
        }

        public void Stop(ProjectileHandle handle, ProjectileEndReason reason)
        {
            if (!handle.IsValid)
            {
                return;
            }

            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                ProjectileRuntimeState state = activeProjectiles[i];
                if (state != null && state.Handle.Id == handle.Id)
                {
                    StopInternal(state, reason);
                    RemoveAt(i);
                    return;
                }
            }
        }

        public void StopAll(ProjectileEndReason reason)
        {
            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                ProjectileRuntimeState state = activeProjectiles[i];
                if (state != null && state.IsRunning)
                {
                    StopInternal(state, reason);
                }

                RemoveAt(i);
            }
        }

        void TickSingle(ProjectileRuntimeState state, float deltaTime)
        {
            ProjectilePose previousPose = state.Pose;
            state.AliveTime += deltaTime;

            var preMotionTrace = new ProjectileTrace(
                previousPose.Position,
                previousPose.Position,
                previousPose.Forward,
                previousPose.Forward);
            var preMotionContext = new ProjectileFrameContext(
                deltaTime,
                state.AliveTime,
                in previousPose,
                in state.Pose,
                in preMotionTrace,
                services.TargetProvider);

            for (int i = 0; i < state.Recipe.MotionModules.Length; i++)
            {
                state.Recipe.MotionModules[i]?.Evaluate(state, in preMotionContext, ref state.Pose);
            }

            state.TravelDistance += ProjectileVector3.Distance(previousPose.Position, state.Pose.Position);
            var trace = new ProjectileTrace(
                previousPose.Position,
                state.Pose.Position,
                previousPose.Forward,
                state.Pose.Forward);
            var frameContext = new ProjectileFrameContext(
                deltaTime,
                state.AliveTime,
                in previousPose,
                in state.Pose,
                in trace,
                services.TargetProvider);

            services.PoseWriter?.ApplyPose(state.InstanceObject, in state.Pose);
            if (ShouldResolveHit(state) && ResolveHitWindow(state, in frameContext))
            {
                return;
            }

            if (ProjectileStopPolicy.ShouldStop(state, services.TargetProvider, out ProjectileEndReason reason))
            {
                StopInternal(state, reason);
            }
        }

        bool ShouldResolveHit(ProjectileRuntimeState state)
        {
            switch (state.Recipe.Hit.ResolveMode)
            {
                case ProjectileHitResolveMode.Continuous:
                    return true;
                case ProjectileHitResolveMode.Periodic:
                    return state.Recipe.Hit.HitInterval > 0f && state.AliveTime >= state.NextHitResolveTime;
                case ProjectileHitResolveMode.OnLaunchOnly:
                default:
                    return false;
            }
        }

        void ResolveLaunchHitWindow(ProjectileRuntimeState state)
        {
            if (state.Recipe.Hit.ResolveMode != ProjectileHitResolveMode.OnLaunchOnly
                && state.Recipe.Hit.ResolveMode != ProjectileHitResolveMode.Periodic)
            {
                return;
            }

            var trace = new ProjectileTrace(
                state.Pose.Position,
                state.Pose.Position,
                state.Pose.Forward,
                state.Pose.Forward);
            var frameContext = new ProjectileFrameContext(
                0f,
                state.AliveTime,
                in state.Pose,
                in state.Pose,
                in trace,
                services.TargetProvider);
            ResolveHitWindow(state, in frameContext);
        }

        bool ResolveHitWindow(ProjectileRuntimeState state, in ProjectileFrameContext frameContext)
        {
            if (!state.Recipe.Detection.HasShape || services.Query == null)
            {
                ScheduleNextHitWindow(state);
                return false;
            }

            ProjectileHitWindowBuffer hitWindowBuffer = RentHitWindowBuffer(state.Recipe.Detection.MaxHits);
            try
            {
                var queryRequest = new ProjectileQueryRequest(
                    in state.Recipe.Detection,
                    in frameContext.Trace,
                    in state.Pose,
                    state.OwnerId,
                    state.OwnerObject);
                int rawCount = services.Query.Query(in queryRequest, hitWindowBuffer.RawHits);
                int hitCount = ProjectileHitAccumulator.Accumulate(
                    hitWindowBuffer.RawHits,
                    rawCount,
                    hitWindowBuffer.Hits);

                for (int i = 0; i < hitCount; i++)
                {
                    ProjectileHit hit = hitWindowBuffer.Hits[i];
                    ProjectileHitProcessResult result = hitProcessor.Process(
                        state,
                        in hit,
                        ref state.Pose,
                        services.CombatResolver);
                    if (!result.Passed)
                    {
                        continue;
                    }

                    ProjectileHitResolved?.Invoke(state, result.Context);
                    services.PoseWriter?.ApplyPose(state.InstanceObject, in state.Pose);
                    if (result.ShouldStop)
                    {
                        StopInternal(state, result.EndReason);
                        return true;
                    }
                }
            }
            finally
            {
                hitWindowBuffer.InUse = false;
            }

            ScheduleNextHitWindow(state);
            return false;
        }

        void ScheduleNextHitWindow(ProjectileRuntimeState state)
        {
            if (state.Recipe.Hit.ResolveMode != ProjectileHitResolveMode.Periodic
                || state.Recipe.Hit.HitInterval <= 0f)
            {
                return;
            }

            state.NextHitResolveTime = state.AliveTime + state.Recipe.Hit.HitInterval;
        }

        void StopInternal(ProjectileRuntimeState state, ProjectileEndReason reason)
        {
            if (state == null || !state.IsRunning)
            {
                return;
            }

            state.IsRunning = false;
            state.EndReason = reason;
            for (int i = state.Recipe.MotionModules.Length - 1; i >= 0; i--)
            {
                state.Recipe.MotionModules[i]?.OnDetach(state);
            }

            ProjectileStopped?.Invoke(state, reason);
            if (state.InstanceObject != null)
            {
                services.InstanceFactory?.Release(state.InstanceObject);
                state.InstanceObject = null;
            }
        }

        ProjectileRuntimeState RentState()
        {
            return statePool.Count > 0 ? statePool.Pop() : new ProjectileRuntimeState();
        }

        ProjectileHitWindowBuffer RentHitWindowBuffer(int recipeMaxHits)
        {
            int capacity = ResolveHitCapacity(recipeMaxHits);
            for (int i = 0; i < hitWindowBuffers.Count; i++)
            {
                ProjectileHitWindowBuffer buffer = hitWindowBuffers[i];
                if (!buffer.InUse && buffer.Capacity >= capacity)
                {
                    buffer.InUse = true;
                    return buffer;
                }
            }

            var newBuffer = new ProjectileHitWindowBuffer(capacity)
            {
                InUse = true,
            };
            hitWindowBuffers.Add(newBuffer);
            return newBuffer;
        }

        int ResolveHitCapacity(int recipeMaxHits)
        {
            int capacity = ProjectileMath.Max(1, recipeMaxHits);
            return capacity < maxHitsPerQuery ? capacity : maxHitsPerQuery;
        }

        void RemoveAt(int index)
        {
            ProjectileRuntimeState state = activeProjectiles[index];
            int lastIndex = activeProjectiles.Count - 1;
            activeProjectiles[index] = activeProjectiles[lastIndex];
            activeProjectiles.RemoveAt(lastIndex);
            if (state != null)
            {
                state.Reset();
                statePool.Push(state);
            }
        }

        sealed class ProjectileHitWindowBuffer
        {
            public readonly ProjectileRawHit[] RawHits;
            public readonly ProjectileHit[] Hits;
            public bool InUse;

            public ProjectileHitWindowBuffer(int capacity)
            {
                Capacity = ProjectileMath.Max(1, capacity);
                RawHits = new ProjectileRawHit[Capacity];
                Hits = new ProjectileHit[Capacity];
            }

            public int Capacity { get; }
        }
    }
}
