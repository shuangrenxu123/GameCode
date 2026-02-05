using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Framework.ECS
{

    public struct Entity
    {
        public int id;
        /// <summary>
        /// 避免entity的id相同的时候判定为同一个
        /// </summary>
        public int gen;
        public override int GetHashCode()
        {
            unchecked
            {
                return (23 * 31 + id) * 31 + gen;
            }
        }
    }
    public static class RawEntityOffsets
    {
        public const int ComponentsCount = 0;
        public const int Gen = 1;
        public const int Components = 2;
    }
    public class ECSWorld
    {
        short[] entities;

        //EntityID复用
        int[] recycledEntities;
        int recycledEntitiesCount;

        int entitiesItemSize;
        int entitiesCount;


        //一个componentPool所容纳的Entity默认值
        readonly int componentDenseSize;
        readonly int componentRecycledSize;

        IEcsComponent[] components;
        short componentCount;
        readonly Dictionary<Type, IEcsComponent> componentPools;

        #region  Filter

        readonly Dictionary<int, EcsFilters> hashFilters;
        readonly List<EcsFilters> allFilters;

        //key - ComponentId  Value -他所涉及的Filter，用于Component改变的时候，
        // 即某个Entity的增删了Component的时候，方便修改他的相关的Filter
        List<EcsFilters>[] filtersByIncludedComponents;

        //key - componentId  Value -他所涉及的Filter
        List<EcsFilters>[] filtersByExcludedComponents;

        #endregion

        #region Mask

        /// <summary>
        /// mask的回收池
        /// </summary>
        public Mask[] maskRecycledPool;
        public int maskRecycledCount;

        #endregion

        public ECSWorld(in Config cfg = default)
        {
            //entities
            var capacity = cfg.Entities > 0 ? cfg.Entities : Config.EntitiesDefault;
            entitiesItemSize = RawEntityOffsets.Components +
                (cfg.EntityComponentsSize > 0 ?
                    cfg.EntityComponentsSize : Config.EntityComponentsSizeDefault);

            entities = new short[capacity * entitiesItemSize];
            entitiesCount = 0;

            var recycledCapacity = cfg.RecycledEntities > 0 ? cfg.RecycledEntities : Config.RecycledEntitiesDefault;
            recycledEntities = new int[recycledCapacity];
            recycledEntitiesCount = 0;

            //component
            var componentCapacity = cfg.Pools > 0 ? cfg.Pools : Config.PoolsDefault;
            components = new IEcsComponent[componentCapacity];
            componentCount = 0;
            componentPools = new(componentCapacity);
            filtersByIncludedComponents = new List<EcsFilters>[componentCapacity];
            filtersByExcludedComponents = new List<EcsFilters>[componentCapacity];

            componentDenseSize = cfg.PoolDenseSize > 0 ? cfg.PoolDenseSize : Config.PoolDenseSizeDefault;
            componentRecycledSize = cfg.PoolRecycledSize > 0 ? cfg.PoolRecycledSize : Config.PoolRecycledSizeDefault;

            //filter
            var filterCapacity = cfg.Filters > 0 ? cfg.Filters : Config.FiltersDefault;
            hashFilters = new Dictionary<int, EcsFilters>(filterCapacity);
            allFilters = new List<EcsFilters>(filterCapacity);

            //Mask
            maskRecycledPool = new Mask[64];
            maskRecycledCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRawEntityOffset(int entity)
        {
            return entity * entitiesItemSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Mask CreateFilter<T>() where T : struct
        {
            var mask = maskRecycledCount > 0 ? maskRecycledPool[--maskRecycledCount] : new Mask(this);
            return mask.Inc<T>();
        }

        public int AddEntity()
        {
            int entity;
            if (recycledEntitiesCount > 0)
            {
                entity = recycledEntities[--recycledEntitiesCount];
                entities[GetRawEntityOffset(entity) + RawEntityOffsets.Gen] *= -1;
            }
            else
            {
                if (entitiesCount * entitiesItemSize == entities.Length)
                {
                    //左移一位 = 乘以2
                    var newSize = entitiesCount << 1;
                    //数组扩容
                    Array.Resize(ref entities, newSize * entitiesItemSize);

                    for (int i = 0; i < componentCount; i++)
                    {
                        components[i].ReSize(newSize);
                    }
                    for (int i = 0; i < allFilters.Count; i++)
                    {
                        allFilters[i].ResizeSparseIndex(newSize);
                    }
                }

                entity = entitiesCount++;
                entities[GetRawEntityOffset(entity) + RawEntityOffsets.Gen] = 1;
            }
            return entity;
        }
        public void RemoveEntity(int entity)
        {
            var entitiesOffset = GetRawEntityOffset(entity);
            ref var entityGen = ref entities[entitiesOffset + RawEntityOffsets.Gen];

            if (entityGen < 0)
            {
                return;
            }
            ref var componentCount = ref entities[entitiesOffset + RawEntityOffsets.ComponentsCount];
            if (componentCount > 0)
            {
                for (int i = entitiesOffset + RawEntityOffsets.Components + componentCount - 1; i >= entitiesOffset + RawEntityOffsets.Components; i--)
                {
                    components[entities[i]].Remove(entity);
                }
            }
            else
            {
                entityGen = (short)(entityGen == short.MaxValue ? -1 : -(entityGen + 1));
                if (recycledEntitiesCount == recycledEntities.Length)
                {
                    Array.Resize(ref recycledEntities, recycledEntitiesCount << 1);
                }
                recycledEntities[recycledEntitiesCount++] = entity;
            }

        }

        public void AddComponentToRawEntityInternal(int entity, short pooId)
        {
            var offset = GetRawEntityOffset(entity);
            var dataCount = entities[offset + RawEntityOffsets.ComponentsCount];
            //如果一个entity所能容纳的component满了
            if (dataCount + RawEntityOffsets.Components == entitiesItemSize)
            {
                ExtendEntitiesCache();
                //这里是因为前面的entity扩容了，因此entity的Offset需要重新获得
                offset = GetRawEntityOffset(entity);
            }
            entities[offset + RawEntityOffsets.ComponentsCount]++;
            entities[offset + RawEntityOffsets.Components + dataCount] = pooId;
        }


        /// <summary>
        /// 当entity的Component发生变化的时候调用
        /// 用于更新Filter
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        /// <param name="added"></param>
        internal void OnEntityChangeInternal(int entity, short component, bool added)
        {
            var includeList = filtersByIncludedComponents[component];
            var excludeList = filtersByExcludedComponents[component];
            if (added)
            {
                if (includeList != null)
                {
                    //找到所有他应该有的Filter
                    foreach (var filter in includeList)
                    {
                        //如果他有。说明它满足这个Filter
                        if (IsMaskCompatible(filter.mask, entity))
                        {
                            filter.AddEntity(entity);
                        }
                    }
                }
                if (excludeList != null)
                {
                    //找到所有他不应该拥有的Component
                    foreach (var filter in excludeList)
                    {
                        //因为在未获得该组件的时候，他是满足mask的，那么获得了该组件一定就不满足了
                        if (IsMaskCompatibleWithout(filter.mask, entity, component))
                        {
                            filter.RemoveEntity(entity);
                        }
                    }
                }
            }
            else
            {
                if (includeList != null)
                {
                    foreach (var filter in includeList)
                    {
                        if (IsMaskCompatible(filter.mask, entity))
                        {
                            filter.RemoveEntity(entity);
                        }
                    }
                }
                if (excludeList != null)
                {
                    foreach (var filter in excludeList)
                    {
                        if (IsMaskCompatibleWithout(filter.mask, entity, component))
                        {
                            filter.AddEntity(entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 返回当前的能容纳的Entity数量
        /// </summary>
        /// <returns></returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntitySize()
        {
            return entities.Length / entitiesItemSize;
        }
        /// <summary>
        /// 调整一个Entity所占用的大小
        /// </summary>
        private void ExtendEntitiesCache()
        {
            var newItemSize = RawEntityOffsets.Components + ((entitiesItemSize - RawEntityOffsets.ComponentsCount) << 1);
            var newEntities = new short[GetEntitySize() * newItemSize];
            int oldStart = 0;
            int newStart = 0;

            for (int i = 0; i < entitiesCount; i++)
            {
                int DataLen = RawEntityOffsets.Components + entities[oldStart + RawEntityOffsets.ComponentsCount];

                for (int j = 0; j < DataLen; j++)
                {
                    newEntities[newStart + j] = entities[oldStart + j];
                }

                oldStart += entitiesItemSize;
                newStart += newItemSize;
            }
            entities = newEntities;
            entitiesItemSize = newItemSize;
        }

        internal short RemoveComponentFromRawEntityInternal(int entity, short poolId)
        {
            var offset = GetRawEntityOffset(entity);

            ref short count = ref entities[offset + RawEntityOffsets.ComponentsCount];
            //entity上剩余的组件数量
            count -= 1;
            //第一个组件的开始下标
            var componentOffset = offset + RawEntityOffsets.Components;

            for (int i = 0; i <= count; i++)
            {
                if (entities[componentOffset + i] == poolId)
                {
                    //如果移除的是最后一个，那么就不管他。反之就应该让最后一个填充进去
                    //这里的componentOffset+count已经是最后一个，因为他的下标是从0开始，因此前面减一了才是正确的
                    if (i < count)
                    {
                        entities[componentOffset + i] = entities[componentOffset + count];
                    }
                    return count;
                }
            }
            return 0;
        }

        internal ECSPool<T> GetOrAddComponent<T>() where T : struct
        {
            var componentType = typeof(T);
            if (componentPools.TryGetValue(componentType, out var c))
            {
                return c as ECSPool<T>;
            }

            var component = new ECSPool<T>(this,
                                           componentCount,
                                           componentDenseSize,
                                           GetEntitySize(),
                                           componentRecycledSize);

            componentPools[componentType] = component;

            if (componentCount == components.Length)
            {
                var newSize = componentCount << 1;
                Array.Resize(ref components, newSize);
                Array.Resize(ref filtersByIncludedComponents, newSize);
                Array.Resize(ref filtersByExcludedComponents, newSize);
            }

            components[componentCount++] = component;
            return component;
        }

        /// <summary>
        /// 尝试获得一个Filter
        /// </summary>
        /// <param name="mask">目标Mask</param>
        /// <param name="capacity">一个Filter包含的entity对象</param>
        /// <returns></returns>
        internal (EcsFilters, bool) GetOrAddFilterInternal(Mask mask, int capacity = 512)
        {
            var hash = mask.hash;
            if (hashFilters.TryGetValue(hash, out var f))
            {
                return (f, false);
            }
            var filter = new EcsFilters(this, mask, capacity, GetEntitySize());
            hashFilters.Add(hash, filter);
            allFilters.Add(filter);

            for (int i = 0; i < mask.IncludeCount; i++)
            {
                var list = filtersByIncludedComponents[mask.Include[i]];
                if (list == null)
                {
                    list = new List<EcsFilters>(8);
                    filtersByIncludedComponents[mask.Include[i]] = list;
                }
                list.Add(filter);
            }
            for (int i = 0; i < mask.ExcludeCount; i++)
            {
                var list = filtersByExcludedComponents[mask.Exclude[i]];
                if (list == null)
                {
                    list = new List<EcsFilters>(8);
                    filtersByExcludedComponents[mask.Exclude[i]] = list;
                }
                list.Add(filter);
            }

            //如果新创建了一个Filter。那么就把他涉及到的相关的Entity给添加进去

            for (int i = 0; i < entitiesCount; i++)
            {
                if (entities[GetRawEntityOffset(i) + RawEntityOffsets.ComponentsCount] > 0
                    && IsMaskCompatible(mask, i))
                {
                    filter.AddEntity(i);
                }
            }

            return (filter, true);
        }

        /// <summary>
        /// 看看某个实体是否满足mask的条件
        /// </summary>
        /// <param name="filterMask"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsMaskCompatible(Mask filterMask, int entity)
        {
            for (int i = 0; i < filterMask.IncludeCount; i++)
            {
                if (!components[filterMask.Include[i]].Has(entity))
                {
                    return false;
                }
            }

            for (int i = 0; i < filterMask.ExcludeCount; i++)
            {
                if (components[filterMask.Exclude[i]].Has(entity))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查当没有某个组件的时候是否还符合mask条件
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="entity"></param>
        /// <param name="componentId"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsMaskCompatibleWithout(Mask mask, int entity, int componentId)
        {
            for (int i = 0; i < mask.IncludeCount; i++)
            {
                var cId = mask.Include[i];
                if (cId == componentId || !components[cId].Has(entity))
                {
                    return false;
                }
            }

            for (int i = 0; i < mask.ExcludeCount; i++)
            {
                var cId = mask.Exclude[i];

                //1.遍历所有他不应该拥有的Component。如果他有其中的一个，并且不是我们想忽略的，那就直接返回false
                if (components[cId].Has(entity)
                    && cId != componentId)
                {
                    return false;
                }
            }

            return true;

        }
    }


    public sealed class Mask
    {
        readonly ECSWorld world;

        internal int[] Include;

        internal int[] Exclude;

        internal int IncludeCount;
        internal int ExcludeCount;

        internal int hash;

        public Mask(ECSWorld world)
        {
            this.world = world;
            IncludeCount = ExcludeCount = 0;
            Include = new int[8];
            Exclude = new int[2];
            hash = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Mask Exc<T>() where T : struct
        {
            var componentId = world.GetOrAddComponent<T>().GetId();
            if (Exclude.Length == ExcludeCount)
            {
                Array.Resize(ref Exclude, ExcludeCount << 1);
            }
            Exclude[ExcludeCount++] = componentId;
            return this;
        }
        public Mask Inc<T>() where T : struct
        {
            var componentId = world.GetOrAddComponent<T>().GetId();

            if (Include.Length == IncludeCount)
            {
                Array.Resize(ref Include, IncludeCount << 1);
            }
            Include[IncludeCount++] = componentId;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsFilters End(int capacity = 512)
        {
            Array.Sort(Include, 0, IncludeCount);
            Array.Sort(Exclude, 0, ExcludeCount);

            unchecked
            {
                unchecked
                {
                    hash = IncludeCount + ExcludeCount;
                    for (int i = 0, iMax = IncludeCount; i < iMax; i++)
                    {
                        hash = hash * 314159 + Include[i];
                    }
                    for (int i = 0, iMax = ExcludeCount; i < iMax; i++)
                    {
                        hash = hash * 314159 - Exclude[i];
                    }
                }

                var (filter, isNew) = world.GetOrAddFilterInternal(this, capacity);
                if (!isNew)
                {
                    Recycle();
                }

                return filter;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Recycle()
        {
            Reset();
            if (world.maskRecycledCount == world.maskRecycledPool.Length)
            {
                Array.Resize(ref world.maskRecycledPool, world.maskRecycledCount << 1);
            }
            world.maskRecycledPool[world.maskRecycledCount++] = this;
        }

        void Reset()
        {
            IncludeCount = 0;
            ExcludeCount = 0;
            hash = 0;
        }
    }


    public struct Config
    {
        public int Entities;
        public int RecycledEntities;
        public int Pools;
        public int Filters;
        public int PoolDenseSize;
        public int PoolRecycledSize;
        public int EntityComponentsSize;

        internal const int EntitiesDefault = 512;
        internal const int RecycledEntitiesDefault = 512;
        internal const int PoolsDefault = 512;
        internal const int FiltersDefault = 512;
        internal const int PoolDenseSizeDefault = 512;
        internal const int PoolRecycledSizeDefault = 512;
        internal const int EntityComponentsSizeDefault = 8;
    }


}
