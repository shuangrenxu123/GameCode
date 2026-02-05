using System.Collections.Generic;
using UnityEngine;

namespace Framework.ECS
{
    public interface IEcsSystem
    {

    }
    public interface IEcsSystems
    {
        T GetShared<T>() where T : class;
        IEcsSystems AddWorld(ECSWorld world, string name);
        ECSWorld GetWorld(string name = null);
        Dictionary<string, ECSWorld> GetAllNamedWorlds();
        IEcsSystems Add(IEcsSystem system);
        List<IEcsSystem> GetAllSystems();
        void Init();
        void Run();
        void Destroy();
    }
    public interface IEcsRunSystem : IEcsSystem
    {
        public void Run(IEcsSystems systems);
    }

    public interface IEcsInitSystem : IEcsSystem
    {
        public void Init(IEcsSystems systems);
    }
    public interface IEcsPostRunSystem : IEcsSystem
    {
        public void PostRun(IEcsSystems systems);
    }

    public class EcsSystem : IEcsSystems
    {
        readonly ECSWorld world;

        readonly Dictionary<string, ECSWorld> worlds;

        readonly List<IEcsSystem> allSystems;
        readonly List<IEcsRunSystem> runSystems;
        readonly List<IEcsPostRunSystem> postRunSystems;
        readonly object shared;

        public EcsSystem(ECSWorld world, object shared = null)
        {
            this.shared = shared;
            this.world = world;

            worlds = new();
            allSystems = new(128);
            runSystems = new(128);
            postRunSystems = new(128);
        }
        public IEcsSystems Add(IEcsSystem system)
        {
            allSystems.Add(system);
            if (system is IEcsRunSystem runSystem)
            {
                runSystems.Add(runSystem);
            }
            if (system is IEcsPostRunSystem postRunSystem)
            {
                postRunSystems.Add(postRunSystem);
            }
            return this;
        }

        public IEcsSystems AddWorld(ECSWorld world, string name)
        {
            worlds[name] = world;
            return this;
        }

        public void Destroy()
        {
            allSystems.Clear();
            runSystems.Clear();
            postRunSystems.Clear();
        }

        public Dictionary<string, ECSWorld> GetAllNamedWorlds()
        {
            return worlds;
        }

        public List<IEcsSystem> GetAllSystems()
        {
            return allSystems;
        }

        public T GetShared<T>() where T : class
        {
            return shared as T;
        }

        public ECSWorld GetWorld(string name = null)
        {
            if (name == null)
            {

                return world;
            }
            else
            {
                return worlds[name];
            }
        }

        public void Init()
        {
            foreach (var system in allSystems)
            {
                if (system is IEcsInitSystem initSystem)
                {
                    initSystem.Init(this);
                }
            }
        }

        public virtual void Run()
        {

            foreach (var system in runSystems)
            {
                system.Run(this);
            }
            foreach (var system in postRunSystems)
            {
                system.PostRun(this);
            }
        }
    }
}
