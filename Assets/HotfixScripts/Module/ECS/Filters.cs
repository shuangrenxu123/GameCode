
using System;
using System.Runtime.CompilerServices;

namespace Framework.ECS
{
    /// <summary>
    /// Filters是对mask的一层包裹，通过mask记录对应的ComponentId，
    /// 在创建的时候会检测一遍所有的符合条件的Entity
    /// 并同时在world 建立对应ComponentId  -  Filter的列表关系
    /// </summary>
    public class EcsFilters
    {
        readonly ECSWorld world;

        public Mask mask { get; private set; }

        public int[] denseEntities;
        int[] sparseEntities;

        public int entityCount;
        int lockCount;
        DelayedOp[] delayedOps;
        int delayedOpsCount;


        public EcsFilters(ECSWorld world, Mask mask, int denseCapacity, int sparseCapacity)
        {
            this.world = world;
            this.mask = mask;
            denseEntities = new int[denseCapacity];
            sparseEntities = new int[sparseCapacity];

            entityCount = 0;
            lockCount = 0;
            delayedOps = new DelayedOp[512];
        }

        bool AddDelayedOp(bool added, int entity)
        {
            if (lockCount <= 0)
            {
                return false;
            }
            if (delayedOps.Length == delayedOpsCount)
            {
                Array.Resize(ref delayedOps, delayedOpsCount << 1);
            }

            ref var op = ref delayedOps[delayedOpsCount++];

            op.Added = added;
            op.Entity = entity;
            return true;

        }

        public Enumerator GetEnumerator()
        {
            lockCount++;
            return new Enumerator(this);
        }

        public void AddEntity(int entity)
        {
            if (AddDelayedOp(true, entity))
            {
                return;
            }

            if (entityCount == denseEntities.Length)
            {
                Array.Resize(ref denseEntities, entityCount << 1);
            }

            denseEntities[entityCount++] = entity;
            sparseEntities[entity] = entityCount;
        }
        public void RemoveEntity(int entity)
        {
            if (AddDelayedOp(false, entity))
            {
                return;
            }

            var index = sparseEntities[entity] - 1;

            sparseEntities[entity] = 0;
            entityCount--;
            if (index < entityCount)
            {
                //这里的最末尾其实是 最开始末尾
                //即如果最开始有3个，减去了中间那个，那么末尾index是2.而由于前面的EntityCount已经--了，所以刚好是第三个的index
                denseEntities[index] = denseEntities[entityCount];
                //从下标机制改为 从1开始的Entity作为下标的计数
                sparseEntities[denseEntities[index]] = index + 1;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResizeSparseIndex(int capacity)
        {
            Array.Resize(ref sparseEntities, capacity);
        }

        internal void Unlock()
        {

            lockCount--;
            if (lockCount == 0 && delayedOpsCount != 0)
            {
                for (int i = 0; i < delayedOpsCount; i++)
                {
                    ref var op = ref delayedOps[i];
                    if (op.Added)
                    {
                        AddEntity(op.Entity);
                    }
                    else
                    {
                        RemoveEntity(op.Entity);
                    }
                }
                delayedOpsCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetEntitiesCount()
        {
            return entityCount;
        }
    }


    public struct Enumerator : IDisposable
    {
        EcsFilters ecsFilters;
        readonly int[] entities;
        int count;
        int index;
        public Enumerator(EcsFilters filters)
        {
            this.ecsFilters = filters;
            entities = filters.denseEntities;
            count = filters.entityCount;
            index = -1;
        }

        public int Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entities[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            index++;
            return index < count;
        }

        public void Dispose()
        {
            ecsFilters.Unlock();
        }
    }
    struct DelayedOp
    {
        public bool Added;
        public int Entity;
    }
}
