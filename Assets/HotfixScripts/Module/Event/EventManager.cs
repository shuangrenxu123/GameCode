using System;
using System.Collections.Generic;
using UnityEngine;


public interface IEvent { }
public interface IEventBinding<T>
{
    public Action<T> OnEvent { get; set; }
    public Action OnEventNoArgs { get; set; }
}

public class EventBinding<T> : IEventBinding<T> where T : IEvent
{
    // 存储有参数的回调
    private Action<T> _onEvent = _ => { };
    // 存储无参数的回调
    private Action _onEventNoArgs = () => { };

    public Action<T> OnEvent
    {
        get => _onEvent;
        set => _onEvent = value;
    }

    public Action OnEventNoArgs
    {
        get => _onEventNoArgs;
        set => _onEventNoArgs = value;
    }

    /// <summary>
    /// 构造函数：绑定带参数的回调
    /// </summary>
    public EventBinding(Action<T> onEvent) => _onEvent = onEvent;

    /// <summary>
    /// 构造函数：绑定无参数的回调
    /// </summary>
    public EventBinding(Action onEventNoArgs) => _onEventNoArgs = onEventNoArgs;

    /// <summary>
    /// 注册：将当前绑定添加到总线
    /// </summary>
    public void Add(Action<T> onEvent) => _onEvent += onEvent;
    public void Remove(Action<T> onEvent) => _onEvent -= onEvent;

    public void Add(Action onEvent) => _onEventNoArgs += onEvent;
    public void Remove(Action onEvent) => _onEventNoArgs -= onEvent;

    public static class EventManager<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> Bindings = new();

        public static void Register(EventBinding<T> binding) => Bindings.Add(binding);
        public static void Deregister(EventBinding<T> binding) => Bindings.Remove(binding);

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventItem">事件数据结构体</param>
        public static void Raise(T eventItem)
        {
            // 注意：这里如果不考虑多线程安全，直接遍历是最快的。
            // 如果在回调中修改了 Bindings 集合（比如在事件中注销自己），这里可能会报错。
            // 简单的做法是复制一份（有GC），高性能做法是倒序遍历或拷贝到临时数组（需维护对象池）。
            // 鉴于绝大多数 Unity 逻辑在主线程，且很少在事件回调中立即注销，直接遍历是性能最高的妥协。

            foreach (var binding in Bindings)
            {
                binding.OnEvent.Invoke(eventItem);
                binding.OnEventNoArgs.Invoke();
            }
        }

        /// <summary>
        /// 清理所有监听（通常在场景切换时调用，或者用于重置）
        /// </summary>
        public static void Clear()
        {
            Bindings.Clear();
        }
    }
}