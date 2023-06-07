using System;
using System.Collections.Generic;
using UnityEngine;

public static class MotionEngine
{
    /// <summary>
    /// 对模块的封装
    /// </summary>
    private class ModuleWrapper
    {
        public int Priority { private set; get; }
        public IModule Module { private set; get; }
        public ModuleWrapper(int priority, IModule module)
        {
            Module = module;
            Priority = priority;
        }
    }

    private static readonly List<ModuleWrapper> _coms = new List<ModuleWrapper>(100);
    private static MonoBehaviour _behaviour;
    private static bool _isDirty = false;
    /// <summary>
    /// 框架的初始化
    /// </summary>
    /// <param name="behaviour"></param>
    public static void Initialize(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            throw new Exception("Monobehaviour is null");
        if (_behaviour != null)
            throw new Exception("_behaviour not is null");
        UnityEngine.Object.DontDestroyOnLoad(behaviour.gameObject);
        _behaviour = behaviour;

    }
    /// <summary>
    /// 框架更新
    /// </summary>
    public static void Update()
    {
        //判断是否更新了新的模块,则需要重新排序
        if (_isDirty)
        {
            _isDirty = false;
            _coms.Sort((left, right) =>
            {
                if (left.Priority > right.Priority)
                    return -1;
                else if (left.Priority == right.Priority)
                    return 0;
                else
                    return 1;
            });
        }
        //轮询所有模块
        for (int i = 0; i < _coms.Count; i++)
        {
            _coms[i].Module.OnUpdate();
        }
    }
    public static T CreateModule<T>(int priority = 0) where T : class, IModule
    {
        return CreateModule<T>(null, priority);
    }
    /// <summary>
    /// 创建组件
    /// </summary>
    /// <typeparam name="T">模组</typeparam>
    /// <param name="param">创建模组的参数</param>
    /// <param name="priority">模组的优先级</param>
    /// <returns></returns>
    public static T CreateModule<T>(System.Object param, int priority = 0) where T : class, IModule
    {
        if (priority < 0)
            throw new Exception("模块的优先级不能为负数");
        if (Contains(typeof(T)))
            throw new Exception("已经包含模块");
        Debug.Log($"创建了{typeof(T)}组件");
        T module = Activator.CreateInstance<T>();
        ModuleWrapper wrapper = new ModuleWrapper(priority, module);
        wrapper.Module.OnCreate(param);//模组初始化的时候的参数设置
        _coms.Add(wrapper);
        _isDirty = true;
        return module;
    }
    /// <summary>
    /// 判断是否拥有模块
    /// </summary>
    /// <param name="moduleType"></param>
    /// <returns></returns>
    public static bool Contains(System.Type moduleType)
    {
        for (int i = 0; i < _coms.Count; i++)
        {
            if (_coms[i].Module.GetType() == moduleType)
                return true;
        }
        return false;
    }
}

