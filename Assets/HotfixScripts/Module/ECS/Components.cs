using System;
using System.Runtime.CompilerServices;
using Framework.ECS;
using UnityEngine;

namespace Framework.ECS
{

    public interface IEcsComponent
    {
        void ReSize(int capacity);
        bool Has(int entity);
        void Remove(int entity);
        void AddRaw(int entity, object component);
        int GetId();
    }

    public class ECSPool<T> : IEcsComponent where T : struct
    {
        int[] sparseItem;
        T[] denseItems;
        //与其叫他count。不如叫做nextComponentIndex比较好
        int componentCount;

        int[] recycledItems;
        int recycledItemsCount;

        ECSWorld world;
        short componentId;
        public ECSPool(ECSWorld world, short id, int denseSize, int sparseSize, int recycleCount)
        {
            componentId = id;
            sparseItem = new int[sparseSize];
            //这里+1是因为sparseItem不能1存储0的下标，因为它在未存储的情况下是0
            denseItems = new T[denseSize + 1];
            componentCount = 1;
            this.world = world;
            recycledItems = new int[recycleCount];
            recycledItemsCount = 0;
        }

        public ref T Add(int entity)
        {
            int index;
            if (entity >= sparseItem.Length)
            {
                throw new Exception($"entity outOfRange {typeof(T)}");
            }
            if (sparseItem[entity] > 0)
            {
                throw new Exception($"Component already add {typeof(T)}");
            }
            if (recycledItemsCount > 0)
            {
                //因为复用了以前的下标，所以不需要componentCount++

                index = recycledItems[--recycledItemsCount];
            }
            else
            {
                index = componentCount;

                if (componentCount == denseItems.Length)
                {
                    Array.Resize(ref denseItems, componentCount << 1);
                }

                componentCount++;
            }
            sparseItem[entity] = index;
            world.OnEntityChangeInternal(entity, componentId, true);
            world.AddComponentToRawEntityInternal(entity, componentId);
            return ref denseItems[index];
        }

        public void Remove(int entity)
        {
            ref int index = ref sparseItem[entity];
            if (index > 0)
            {
                ref T component = ref denseItems[index];

                if (recycledItemsCount == recycledItems.Length)
                {
                    Array.Resize(ref recycledItems, recycledItemsCount << 1);
                }
                recycledItems[recycledItemsCount++] = index;
                index = 0;
                world.OnEntityChangeInternal(entity, componentId, false);
                var componentCount = world.RemoveComponentFromRawEntityInternal(entity, componentId);
                if (componentCount == 0)
                {
                    world.RemoveEntity(entity);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int entity)
        {
            return ref denseItems[sparseItem[entity]];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetId()
        {
            return componentId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int entity)
        {
            return sparseItem[entity] > 0;
        }


        public void ReSize(int capacity)
        {
            Array.Resize(ref sparseItem, capacity);
        }

        public void AddRaw(int entity, object component)
        {
            ref var data = ref Add(entity);
            data = (T)component;
        }
    }
}
