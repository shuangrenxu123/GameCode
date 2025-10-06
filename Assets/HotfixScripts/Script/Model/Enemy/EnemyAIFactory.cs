using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI工厂：负责创建和装配不同类型的大脑
/// </summary>
public static class EnemyAIFactory
{
    private static Dictionary<string, Type> brainTypes = new Dictionary<string, Type>
    {
        ["Default"] = typeof(DefaultEnemyAI)
    };

    /// <summary>
    /// 创建指定类型的大脑
    /// </summary>
    /// <param name="aiType">AI类型标识</param>
    /// <param name="body">敌人身体</param>
    /// <returns>创建的大脑实例</returns>
    public static IEnemyBrain CreateBrain(string aiType, Enemy body)
    {
        if (brainTypes.TryGetValue(aiType, out Type brainType))
        {
            var brain = (IEnemyBrain)Activator.CreateInstance(brainType);
            brain.Initialize(body);
            return brain;
        }

        Debug.LogError($"Unknown AI type: {aiType}");
        return null;
    }

    /// <summary>
    /// 注册新的AI类型
    /// </summary>
    /// <param name="name">AI类型名称</param>
    /// <param name="brainType">大脑类型</param>
    public static void RegisterBrainType(string name, Type brainType)
    {
        brainTypes[name] = brainType;
    }

    /// <summary>
    /// 获取所有已注册的AI类型
    /// </summary>
    /// <returns>AI类型名称列表</returns>
    public static List<string> GetRegisteredAITypes()
    {
        return new List<string>(brainTypes.Keys);
    }
}